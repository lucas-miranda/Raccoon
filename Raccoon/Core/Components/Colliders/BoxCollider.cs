using Raccoon.Graphics;
using System;

namespace Raccoon.Components {
    public class BoxCollider : ColliderComponent {
        private Size _size;

        public BoxCollider(float width, float height, string tag) : base(ColliderType.Box, tag) {
            Size = new Size(width, height);
        }

        public BoxCollider(float width, float height, Enum tag) : this(width, height, tag.ToString()) { }

        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public float Width { get { return Size.Width; } set { Size = new Size(value, Size.Height); } }
        public float Height { get { return Size.Height; } set { Size = new Size(Size.Width, value); } }
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }

        public Size Size {
            get {
                return _size;
            }

            set {
                _size = value;
                if (Graphic != null) {
                    (Graphic as Graphics.Primitives.Rectangle).Size = new Size(value.Width * Game.Instance.Scale, value.Height * Game.Instance.Scale);
                }
            }
        }

        public override void DebugRender() {
            if (Graphic == null) {
                Graphic = new Graphics.Primitives.Rectangle(Width * Game.Instance.Scale, Height * Game.Instance.Scale, Color, false);
            }

            Graphic.Position = Position * Game.Instance.Scale;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render();
        }
    }
}
