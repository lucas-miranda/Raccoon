namespace Raccoon.Graphics {
    public class Text : Graphic {
        #region Private Members

        private string _value;

        #endregion Private Members

        #region Constructors

        public Text(string value, Font font, Color color) {
            Font = font;
            Value = value;
            Color = color;
        }

        #endregion Constructors

        #region Public Properties

        public Text(string value, Font font) : this(value, font, Color.White) { }

        public Font Font { get; set; }

        public string Value {
            get {
                return _value;
            }

            set {
                _value = value;
                Size = new Size(Font.MeasureText(_value));
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
            Surface.DrawString(Font, Value, Position + position, Rotation + rotation, Scale * scale, Flipped ^ flip, (color * Color) * Opacity, Origin / scale, Scroll + scroll, Shader);
        }

        public override void Dispose() { }

        #endregion Public Methods
    }
}
