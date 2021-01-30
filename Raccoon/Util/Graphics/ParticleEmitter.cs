using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon.Util.Graphics {
    public class ParticleEmitter : System.IDisposable {
        #region Private Members

        private Dictionary<string, (Particle Model, EmissionOptions EmissionOptions)> _particleModels = new Dictionary<string, (Particle, EmissionOptions)>();

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
            ParticlesEmit(
                label,
                position,
                rotation,
                flip,
                movementDirection
            );
        }

        public void Emit(string label, Entity entity, float rotation = 0f, ImageFlip flip = ImageFlip.None, Vector2? movementDirection = null) {
            List<Particle> particles = ParticlesEmit(
                label,
                Vector2.Zero,
                rotation,
                flip,
                movementDirection
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

        private List<Particle> ParticlesEmit(string label , Vector2 position, float rotation, ImageFlip flip, Vector2? movementDirection) {
            if (Scene == null) {
                throw new System.InvalidOperationException($"Can't emit, ParticleEmitter.Scene is null.\nMaybe you forgot to set ParticleEmitter.Scene to current Scene?");
            }

            if (!_particleModels.TryGetValue(label, out var particle)) {
                throw new System.ArgumentException($"Could not find particle model with label '{label}'.");
            }

            int count = particle.EmissionOptions.Count;
            if (particle.EmissionOptions.MinCount != particle.EmissionOptions.MaxCount) {
                count = Random.Integer(particle.EmissionOptions.MinCount, particle.EmissionOptions.MaxCount);
            }

            count = Math.Max(1, count);
            List<Particle> particles = new List<Particle>(count);

            uint timeToStart = 0;
            for (int i = 0; i < count; i++) {
                Particle p = new Particle() {
                    Layer = particle.Model.Layer,
                    Animation = new Animation(particle.Model.Animation) {
                        Position = particle.Model.Animation.Position,
                        Rotation = rotation,
                        Flipped = flip
                    }
                };

                p.OnSceneRemoved += () => {
                    _aliveParticles.Remove(p);
                };

                Vector2 displacement = particle.EmissionOptions.DisplacementMin;

                if (particle.EmissionOptions.DisplacementMin != particle.EmissionOptions.DisplacementMax) {
                    displacement = Random.Vector2(particle.EmissionOptions.DisplacementMin, particle.EmissionOptions.DisplacementMax);
                }

                p.Transform.Position = position + displacement;

                uint duration = particle.EmissionOptions.DurationMin;

                if (particle.EmissionOptions.DurationMin != particle.EmissionOptions.DurationMax) {
                    duration = (uint) Random.Integer((int) particle.EmissionOptions.DurationMin, (int) particle.EmissionOptions.DurationMax);
                }

                p.Prepare(duration, timeToStart, particle.EmissionOptions.AnimationKey);
                timeToStart += particle.EmissionOptions.DelayBetweenEmissions;

                // movement
                if (movementDirection.HasValue) {
                    p.PrepareSimpleMovement(
                        movementDirection.Value,
                        particle.EmissionOptions.MaxVelocity,
                        particle.EmissionOptions.Acceleration
                    );
                } else if (particle.EmissionOptions.MovementDirection != Vector2.Zero) {
                    p.PrepareSimpleMovement(
                        particle.EmissionOptions.MovementDirection,
                        particle.EmissionOptions.MaxVelocity,
                        particle.EmissionOptions.Acceleration
                    );
                }

                Scene.Add(p);
                particles.Add(p);
            }

            _aliveParticles.AddRange(particles);
            return particles;
        }

        #endregion Private Methods
    }
}
