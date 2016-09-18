using Raccoon.Graphics;

namespace Raccoon.Components {
    public enum ColliderType {
        Box,
        Circle,
        Polygon,
        Line
    }

    public abstract class ColliderComponent : Component {
        protected ColliderComponent(ColliderType type, string tagName) {
            Type = type;
            Physics.Instance.AddCollider(tagName, this);
        }

        public ColliderType Type { get; private set; }
        protected Graphic Graphic { get; set; }

        public bool Collides(string tagName) {
            return Physics.Instance.Collides(this, tagName);
        }
    }
}
