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

        public override void Update(int delta) {
        }

        public override void Render() {
            if (Graphic == null) {
                Graphic = new Graphics.Primitive.Rectangle((int) Width, (int) Height, Color.Red, false);
            }

            Graphic.Position = Position;
            Graphic.Render();
        }
    }
}
