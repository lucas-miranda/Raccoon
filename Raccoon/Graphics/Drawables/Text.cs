namespace Raccoon.Graphics {
    public class Text : Graphic, System.IDisposable {
        #region Private Members

        private Font _font;
        private string _value;
        private Size _unscaledSize;

        #endregion Private Members

        #region Constructors

        public Text(string value, Font font, Color color) {
            if (font == null || font.Face == null) {
                throw new System.ArgumentException("Invalid Font value.");
            }

            Font = font;
            Value = value;
            Color = color;
        }

        public Text(string value, Font font) : this(value, font, Color.White) {
        }

        public Text(string value) : this(value, Game.Instance.StdFont, Color.White) {
        }

        public Text() : this("", Game.Instance.StdFont, Color.White) {
        }

        #endregion Constructors

        #region Public Properties

        public bool IsDisposed { get; private set; }

        public Font Font {
            get {
                return _font;
            }

            set {
                if (value == _font) {
                    return;
                }

                _font = value;

                if (!string.IsNullOrEmpty(_value)) {
                    _unscaledSize = new Size(_font.MeasureText(_value));
                    Size = _unscaledSize * Scale;
                }
            }
        }

        public string Value {
            get {
                return _value;
            }

            set {
                if (value.Equals(_value)) {
                    return;
                }

                _value = value;
                _unscaledSize = new Size(_font.MeasureText(_value));
                Size = _unscaledSize * Scale;
            }
        }

        public new Vector2 Scale {
            get {
                return base.Scale;
            }

            set {
                base.Scale = value;
                Size = _unscaledSize * value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            _font = null;

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            Renderer.DrawString(Font, Value, Position + position, Rotation + rotation, Scale * scale, Flipped ^ flip, (color * Color) * Opacity, Origin, Scroll + scroll, shader, layerDepth);
        }

        #endregion Protected Methods
    }
}
