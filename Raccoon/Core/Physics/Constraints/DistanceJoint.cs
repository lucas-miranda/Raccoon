using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public class DistanceJoint : Joint {
        public DistanceJoint(Body a, Body b, float restingDistance, float stiffness) : base(a, b) {
            RestingDistance = restingDistance;
            Stiffness = stiffness;
        }

        public float RestingDistance { get; set; }
        public float Stiffness { get; set; }

        public override void Solve() {
            Vector2 posDiff = A.Position - B.Position;
            float distance = Math.Distance(A.Position, B.Position);
            float difference = (RestingDistance - distance) / distance;

            float invMassA = 1f / A.Mass, invMassB = 1f / B.Mass;
            float scalarCollA = (invMassA / (invMassA + invMassB)) * Stiffness;
            float scalarCollB = Stiffness - scalarCollA;

            // push/pull based on mass
            A.Position += posDiff * scalarCollA * difference;
            B.Position -= posDiff * scalarCollB * difference;
        }
    }
}
