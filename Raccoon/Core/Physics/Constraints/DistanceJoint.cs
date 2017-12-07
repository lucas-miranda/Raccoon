using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon.Constraints {
    public class DistanceJoint : Joint {
        public DistanceJoint(Collider colliderA, Collider colliderB, float restingDistance, float stiffness) : base(colliderA, colliderB) {
            RestingDistance = restingDistance;
            Stiffness = stiffness;
        }

        public float RestingDistance { get; set; }
        public float Stiffness { get; set; }

        public override void Solve() {
            Vector2 posDiff = ColliderA.Position - ColliderB.Position;
            float distance = Math.Distance(ColliderA.Position, ColliderB.Position);
            float difference = (RestingDistance - distance) / distance;

            float invMassA = 1f / ColliderA.Mass, invMassB = 1f / ColliderB.Mass;
            float scalarCollA = (invMassA / (invMassA + invMassB)) * Stiffness;
            float scalarCollB = Stiffness - scalarCollA;

            // push/pull based on mass
            ColliderA.Position += posDiff * scalarCollA * difference;
            ColliderB.Position -= posDiff * scalarCollB * difference;
        }
    }
}
