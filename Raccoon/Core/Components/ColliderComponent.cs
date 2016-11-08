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

        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public Size Size { get; protected set; }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }
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
