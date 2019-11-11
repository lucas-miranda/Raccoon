using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon.Util.Graphics {
    public class ParticleEmitter : System.IDisposable {
        #region Private Members

        private Dictionary<string, (Particle, EmissionOptions)> _particleModels = new Dictionary<string, (Particle, EmissionOptions)>();

        private List<Particle> _aliveParticles = new List<Particle>();

        #endregion Private Members

        #region Constructors

        public ParticleEmitter() {
        }

        ~ParticleEmitter() {
            _particleModels = null;
            Dispose();
        }

        #endregion Constructors

        #region Public Properties

        public Scene Scene { get; set; }
        public bool IsDisposed { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public Particle AddModel(string label, Particle particle, EmissionOptions emissionOptions) {
            _particleModels.Add(label, (particle, emissionOptions));
            return particle;
        }

        public void ClearModels() {
            _particleModels.Clear();
        }

        public void Emit(string label, Vector2 position, float rotation = 0f, ImageFlip flip = ImageFlip.None, Vector2? movementDirection = null) {
            InternalEmit(
                label, 
                position, 
                rotation, 
                flip, 
                movementDirection, 
                out List<Particle> particles
            );
        }

        public void Emit(string label, Entity entity, float rotation = 0f, ImageFlip flip = ImageFlip.None, Vector2? movementDirection = null) {
            InternalEmit(
                label, 
                entity.Transform.Position, 
                rotation, 
                flip, 
                movementDirection, 
                out List<Particle> particles
            );

            foreach (Particle particle in particles) {
                particle.Transform.Parent = entity.Transform;
            }
        }

        public void ClearAllParticles() {
            if (Scene != null) {
                for (int i = 0; i < _aliveParticles.Count; i++) {
                    if (Scene.RemoveEntity(_aliveParticles[i])) {
                        i--;
                    }
                }
            } else {
                _aliveParticles.Clear();
            }
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            ClearAllParticles();
            Scene = null;
            _particleModels = null;

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Private Methods

        private void InternalEmit(string label, Vector2 position, float rotation, ImageFlip flip, Vector2? movementDirection, out List<Particle> particles) {
            (Particle particleModel, EmissionOptions emissionOptions) = _particleModels[label];

            int count = emissionOptions.Count;
            if (emissionOptions.MinCount != emissionOptions.MaxCount) {
                count = Random.Integer(emissionOptions.MinCount, emissionOptions.MaxCount);
            }

            count = Math.Max(1, count);
            particles = new List<Particle>(count);

            uint timeToStart = 0;
            for (int i = 0; i < count; i++) {
                Particle particle = new Particle() {
                    Layer = particleModel.Layer,
                    Animation = new Animation(particleModel.Animation) {
                        Position = particleModel.Animation.Position,
                        Rotation = rotation,
                        Flipped = flip
                    }
                };

                particle.OnSceneRemoved += () => {
                    _aliveParticles.Remove(particle);
                };

                Vector2 displacement = emissionOptions.DisplacementMin;

                if (emissionOptions.DisplacementMin != emissionOptions.DisplacementMax) {
                    displacement = Random.Vector2(emissionOptions.DisplacementMin, emissionOptions.DisplacementMax);
                }

                particle.Transform.Position = position + displacement;

                uint duration = emissionOptions.DurationMin;

                if (emissionOptions.DurationMin != emissionOptions.DurationMax) {
                    duration = (uint) Random.Integer((int) emissionOptions.DurationMin, (int) emissionOptions.DurationMax);
                }

                particle.Prepare(duration, timeToStart, emissionOptions.AnimationKey);
                timeToStart += emissionOptions.DelayBetweenEmissions;

                // movement
                if (movementDirection != null && movementDirection.HasValue) {
                    particle.PrepareSimpleMovement(
                        movementDirection.Value,
                        emissionOptions.MaxVelocity,
                        emissionOptions.Acceleration
                    );
                } else if (emissionOptions.MovementDirection != Vector2.Zero) {
                    particle.PrepareSimpleMovement(
                        emissionOptions.MovementDirection,
                        emissionOptions.MaxVelocity,
                        emissionOptions.Acceleration
                    );
                }

                Scene.Add(particle);
                particles.Add(particle);
            }

            _aliveParticles.AddRange(particles);
        }

        #endregion Private Methods
    }
}
