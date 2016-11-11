using Raccoon.Graphics;
using System;
using System.Collections.Generic;

namespace Raccoon.Components {
    public abstract class Collider : Component {
        protected Collider(string tag) {
            Color = Color.Red;
            Physics.Instance.AddCollider(this, tag);
        }

        protected Collider(Enum tag) : this(tag.ToString()) { }

        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public Size Size { get; protected set; }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public float Top { get { return Y - Origin.Y; } }
        public float Right { get { return X + Width - Origin.X; } }
        public float Bottom { get { return Y + Height - Origin.Y; } }
        public float Left { get { return X - Origin.X; } }
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }
        protected Graphic Graphic { get; set; }
        protected Color Color { get; set; }

        public override void Update(int delta) { }
        public override void Render() { }

        public void DebugRender(Color color) {
            Color = color;
            DebugRender();
        }

        public bool Collides(Vector2 position, string tag) {
            return Physics.Instance.Collides(position, this, tag);
        }

        public bool Collides(Vector2 position, Enum tag) {
            return Physics.Instance.Collides(position, this, tag);
        }

        public bool Collides(int x, int y, string tag) {
            return Physics.Instance.Collides(x, y, this, tag);
        }

        public bool Collides(int x, int y, Enum tag) {
            return Physics.Instance.Collides(x, y, this, tag);
        }

        public bool Collides(string tag) {
            return Physics.Instance.Collides(this, tag);
        }

        public bool Collides(Enum tag) {
            return Physics.Instance.Collides(this, tag);
        }

        public bool Collides(Vector2 position, IEnumerable<string> tags) {
            return Physics.Instance.Collides(position, this, tags);
        }

        public bool Collides(Vector2 position, IEnumerable<Enum> tags) {
            return Physics.Instance.Collides(position, this, tags);
        }

        public bool Collides(int x, int y, IEnumerable<string> tags) {
            return Physics.Instance.Collides(x, y, this, tags);
        }

        public bool Collides(int x, int y, IEnumerable<Enum> tags) {
            return Physics.Instance.Collides(x, y, this, tags);
        }

        public bool Collides(IEnumerable<string> tags) {
            return Physics.Instance.Collides(this, tags);
        }

        public bool Collides(IEnumerable<Enum> tags) {
            return Physics.Instance.Collides(this, tags);
        }
    }
}
