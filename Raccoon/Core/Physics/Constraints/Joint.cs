using Raccoon.Components;
using Raccoon.Graphics;

namespace Raccoon.Constraints {
    public abstract class Joint : IConstraint {
        public Joint(Collider colliderA, Collider colliderB) {
            if (colliderA == null || colliderB == null) {
                throw new System.ArgumentNullException("ColliderA and colliderB can't be null.");
            }

            ColliderA = colliderA;
            ColliderB = colliderB;
        }

        public Collider ColliderA { get; private set; }
        public Collider ColliderB { get; private set; }

        public abstract void Solve();

        public virtual void DebugRender() {
            Debug.DrawLine(ColliderA.Position, ColliderB.Position, Color.Orange);
        }
    }
}
