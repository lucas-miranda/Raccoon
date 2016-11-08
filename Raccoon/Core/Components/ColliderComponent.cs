using Raccoon.Graphics;
using System;

namespace Raccoon.Components {
    public enum ColliderType {
        Box,
        Circle,
        Polygon,
        Line,
        Grid
    }

    public abstract class ColliderComponent : Component {
        protected ColliderComponent(ColliderType type, string tag) {
            Type = type;
            Color = Color.Red;
            Physics.Instance.AddCollider(this, tag);
        }

        protected ColliderComponent(ColliderType type, Enum tag) : this(type, tag.ToString()) { }

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

        public bool Collides(string tag) {
            return Physics.Instance.Collides(this, tag);
        }

        public bool Collides(Enum tag) {
            return Physics.Instance.Collides(this, tag);
        }
    }
}
