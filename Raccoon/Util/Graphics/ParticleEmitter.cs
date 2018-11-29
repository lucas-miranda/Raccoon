using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon.Util.Graphics {
    public class ParticleEmitter {
        public Dictionary<string, (Particle, EmissionOptions)> _particleModels = new Dictionary<string, (Particle, EmissionOptions)>();

        public Scene Scene { get; set; }

        public Particle AddModel(string label, Particle particle, EmissionOptions emissionOptions) {
            _particleModels.Add(label, (particle, emissionOptions));
            return particle;
        }

        public void Emit(string label, Vector2 position) {
            InternalEmit(label, position, out List<Particle> particles);
        }

        public void Emit(string label, Entity entity) {
            InternalEmit(label, entity.Transform.Position, out List<Particle> particles);

            foreach (Particle particle in particles) {
                particle.Transform.Parent = entity.Transform;
            }
        }

        private void InternalEmit(string label, Vector2 position, out List<Particle> particles) {
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
                    Animation = new Animation(particleModel.Animation),
                };

                Vector2 displacement = Random.Vector2(emissionOptions.DisplacementMin, emissionOptions.DisplacementMax);
                particle.Transform.Position = position + displacement;

                uint duration = (uint) Random.Integer((int) emissionOptions.DurationMin, (int) emissionOptions.DurationMax);

                particle.Prepare(duration, timeToStart, emissionOptions.AnimationKey);
                timeToStart += emissionOptions.DelayBetweenEmissions;

                Scene.Add(particle);
                particles.Add(particle);
            }
        }
    }
}
