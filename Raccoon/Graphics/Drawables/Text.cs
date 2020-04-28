namespace Raccoon.Graphics {
    public partial class Text : Graphic, System.IDisposable {
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
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
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

                if (!string.IsNullOrEmpty(_value)) {
                    Data = _font.RenderMap.PrepareText(_value, out _unscaledSize);
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
                Data = _font.RenderMap.PrepareText(_value, out _unscaledSize);
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

        public new float ScaleXY {
            get {
                return base.ScaleXY;
            }

            set {
                base.ScaleXY = value;
                Size = _unscaledSize * value;
            }
        }

        public Size UnscaledSize {
            get {
                return _unscaledSize;
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal RenderData Data { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            _font = null;
            _value = null;
            Data = null;

            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            Renderer.DrawString(
                Font,
                Data,
                new Rectangle(Position + position, _unscaledSize),
                Rotation + rotation,
                Scale * scale,
                Flipped ^ flip,
                (color * Color) * Opacity,
                Origin,
                Scroll + scroll,
                shader,
                shaderParameters,
                //origin,
                layerDepth
            );
        }

        #endregion Protected Methods
    }
}
