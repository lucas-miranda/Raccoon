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
            if (Scene == null) {
                throw new System.InvalidOperationException($"Can't emit, ParticleEmitter.Scene is null.\nMaybe you forgot to set ParticleEmitter.Scene to current Scene?");
            }

            if (!_particleModels.TryGetValue(label, out (Particle model, EmissionOptions emissionOptions) particle)) {
                throw new System.ArgumentException($"Could not find particle model with label '{label}'.");
            }

            int count = particle.emissionOptions.Count;
            if (particle.emissionOptions.MinCount != particle.emissionOptions.MaxCount) {
                count = Random.Integer(particle.emissionOptions.MinCount, particle.emissionOptions.MaxCount);
            }

            count = Math.Max(1, count);
            particles = new List<Particle>(count);

            uint timeToStart = 0;
            for (int i = 0; i < count; i++) {
                Particle p = new Particle() {
                    Layer = particle.model.Layer,
                    Animation = new Animation(particle.model.Animation) {
                        Position = particle.model.Animation.Position,
                        Rotation = rotation,
                        Flipped = flip
                    }
                };

                p.OnSceneRemoved += () => {
                    _aliveParticles.Remove(p);
                };

                Vector2 displacement = particle.emissionOptions.DisplacementMin;

                if (particle.emissionOptions.DisplacementMin != particle.emissionOptions.DisplacementMax) {
                    displacement = Random.Vector2(particle.emissionOptions.DisplacementMin, particle.emissionOptions.DisplacementMax);
                }

                p.Transform.Position = position + displacement;

                uint duration = particle.emissionOptions.DurationMin;

                if (particle.emissionOptions.DurationMin != particle.emissionOptions.DurationMax) {
                    duration = (uint) Random.Integer((int) particle.emissionOptions.DurationMin, (int) particle.emissionOptions.DurationMax);
                }

                p.Prepare(duration, timeToStart, particle.emissionOptions.AnimationKey);
                timeToStart += particle.emissionOptions.DelayBetweenEmissions;

                // movement
                if (movementDirection != null && movementDirection.HasValue) {
                    p.PrepareSimpleMovement(
                        movementDirection.Value,
                        particle.emissionOptions.MaxVelocity,
                        particle.emissionOptions.Acceleration
                    );
                } else if (particle.emissionOptions.MovementDirection != Vector2.Zero) {
                    p.PrepareSimpleMovement(
                        particle.emissionOptions.MovementDirection,
                        particle.emissionOptions.MaxVelocity,
                        particle.emissionOptions.Acceleration
                    );
                }

                Scene.Add(p);
                particles.Add(p);
            }

            _aliveParticles.AddRange(particles);
        }

        #endregion Private Methods
    }
}
