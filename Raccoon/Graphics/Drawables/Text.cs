namespace Raccoon.Graphics {
    public class Text : Graphic {
        private string _value;

        public Text(string value, Font font, Color color) {
            Font = font;
            Value = value;
            Color = color;
        }

        public Text(string value, Font font) : this(value, font, Color.White) { }

        public Font Font { get; set; }

        public string Value {
            get {
                return _value;
            }

            set {
                _value = value;
                Size = new Size(Font.MeasureText(_value) * Scale);
            }
        }

        public new Vector2 Scale {
            get {
                return base.Scale;
            }

            set {
                base.Scale = value;
                Size = new Size(Font.MeasureText(_value) * Scale);
            }
        }

        public override void Render(Vector2 position, float rotation) {
            Surface.DrawString(Font, Value, position, FinalColor, rotation, Origin / Scale, Scale, Flipped, Scroll, Shader);
        }

        public override void Dispose() { }
    }
}
