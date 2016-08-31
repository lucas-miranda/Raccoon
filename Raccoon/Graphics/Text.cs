namespace Raccoon.Graphics {
    public class Text : Graphic {
        public Text(string value, Font font, Color color) {
            Font = font;
            Value = value;
            Color = color;
        }

        public Text(string value, Font font) : this(value, font, Color.White) {
        }

        public Font Font { get; set; }
        public string Value { get; set; }
        public new Color Color { get; set; }

        public override void Update(int delta) {
        }

        public override void Draw() {
            Game.Instance.Core.SpriteBatch.DrawString(
                Font.SpriteFont, 
                Value, 
                Position, 
                Color,
                Rotation,
                Origin,
                Scale,
                Flipped,
                Layer
            );
        }

        internal override void Load() {
        }
    }
}
