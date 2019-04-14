namespace Raccoon.Graphics {
    public class Text : Image {
        #region Private Members

        private Font _font;
        private string _value;

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

        public Font Font {
            get {
                return _font;
            }

            set {
                if (value == _font) {
                    return;
                }

                _font = value;
                NeedsReload = true;
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
                Size = new Size(Font.MeasureText(_value));
                NeedsReload = true;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            base.Dispose();

            if (!Font.IsDisposed) {
                Font.Dispose();
                Font = null;
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();
            Texture = Font.Rasterize(Renderer.SpriteBatch.GraphicsDevice, Value, out Size size);
            Size = size;
        }

        #endregion Protected Methods
    }
}
