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

        public override void Render(Vector2 position, float rotation) {
            Game.Instance.Core.SpriteBatch.DrawString(
                Font.SpriteFont, 
                Value, 
                position, 
                Color,
                rotation * Util.Math.DegToRad,
                Origin,
                Scale,
                (Microsoft.Xna.Framework.Graphics.SpriteEffects) Flipped,
                LayerDepth
            );
        }

        public override void Dispose() {
        }
    }
}
