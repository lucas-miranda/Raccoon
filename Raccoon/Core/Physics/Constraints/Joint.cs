using Raccoon.Components;
using Raccoon.Graphics;

namespace Raccoon {
    public abstract class Joint : IConstraint {
        public Joint(Body a, Body b) {
            if (a == null || b == null) {
                throw new System.ArgumentNullException("Body A and B can't be null.");
            }

            A = a;
            B = b;
        }

        public Body A { get; private set; }
        public Body B { get; private set; }

        public abstract void Solve();

        public virtual void DebugRender() {
            Debug.DrawLine(A.Position, B.Position, Color.Orange);
        }
    }
}
