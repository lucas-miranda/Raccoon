using Raccoon.Graphics;

namespace Raccoon.Components {
    public enum ColliderType {
        Box,
        Circle,
        Polygon,
        Line,
        Grid
    }

    public abstract class ColliderComponent : Component {
        protected ColliderComponent(ColliderType type, string tagName) {
            Type = type;
            Color = Color.Red;
            Physics.Instance.AddCollider(tagName, this);
        }

        public ColliderType Type { get; private set; }
        protected Graphic Graphic { get; set; }
        protected Color Color { get; set; }

        public override void Update(int delta) {
        }

        public override void Render() {
        }

        public void DebugRender(Color color) {
            Color = color;
            DebugRender();
        }

        public bool Collides(string tagName) {
            return Physics.Instance.Collides(this, tagName);
        }
    }
}
