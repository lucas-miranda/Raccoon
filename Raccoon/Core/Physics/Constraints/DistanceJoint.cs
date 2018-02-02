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
            float distance = Math.DistanceSquared(A.Position, B.Position);
            float difference = ((RestingDistance * RestingDistance) - distance) / distance;

            float scalarCollA = (A.InverseMass / (A.InverseMass + B.InverseMass)) * Stiffness;
            float scalarCollB = Stiffness - scalarCollA;

            // push/pull based on mass
            A.Position += posDiff * scalarCollA * difference;
            B.Position -= posDiff * scalarCollB * difference;
        }
    }
}
