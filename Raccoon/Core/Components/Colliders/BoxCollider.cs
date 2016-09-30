using Raccoon.Graphics;

namespace Raccoon.Components {
    public class BoxCollider : ColliderComponent {
        public BoxCollider(float width, float height, string tagName) : base(ColliderType.Box, tagName) {
            Size = new Size(width, height);
        }

        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public Size Size { get; set; }
        public float Width { get { return Size.Width; } set { Size = new Size(value, Size.Height); } }
        public float Height { get { return Size.Height; } set { Size = new Size(Size.Width, value); } }
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }

        public override void Update(int delta) {
        }

        public override void DebugRender() {
            if (Graphic == null || Size * Game.Instance.Scale != Graphic.Size) {
                Graphic = new Graphics.Primitive.Rectangle(Width * Game.Instance.Scale, Height * Game.Instance.Scale, Color, false);
            }

            Graphic.Position = Position * Game.Instance.Scale;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render();
        }
    }
}
