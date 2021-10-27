using Raccoon.Util;

namespace Raccoon.Graphics {
    public partial class Text : Graphic, System.IDisposable {
        #region Private Members

        private Font _font;
        private string _value;
        private Size _unscaledSize;
        private int _startIndex, _endIndex = -1;

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

        public Text(Font font) : this(string.Empty, font, Color.White) {
        }

        public Text(string value) : this(value, Game.Instance.StdFont, Color.White) {
        }

        public Text() : this(string.Empty, Game.Instance.StdFont, Color.White) {
        }

        #endregion Constructors

        #region Public Properties

        public int StartIndex {
            get {
                return _startIndex;
            }

            set {
                if (value == _startIndex) {
                    return;
                }

                if (value < 0) {
                    throw new System.ArgumentException("Index must be zero or positive value.");
                } else if (Data.GlyphCount == 0) {
                    throw new System.InvalidOperationException("Can't define a valid start index, data is empty.");
                } else if (value >= Data.GlyphCount) {
                    throw new System.IndexOutOfRangeException($"Start index out of valid range [0, {Data.GlyphCount - 1}]");
                }

                _startIndex = value;

                int endRealIndex = _endIndex < 0 ? (Data.GlyphCount + _endIndex) : _endIndex;
                float w = 0f;

                for (int i = _startIndex; i <= endRealIndex; i++) {
                    RenderData.Glyph g = Data[i];

                    if (g.Position.X + g.Data.Width > w) {
                        w = g.Position.X + g.Data.Width;
                    }
                }

                RenderData.Glyph lastGlyph = Data[endRealIndex];

                _unscaledSize = new Size(
                    w,
                    (lastGlyph.OriginalPosition.Y + lastGlyph.Data.Height)
                        - Data[_startIndex].OriginalPosition.Y
                );

                Size = _unscaledSize * Scale;
            }
        }

        public int EndIndex {
            get {
                return _endIndex;
            }

            set {
                if (value == _endIndex) {
                    return;
                }

                if (value >= Data.GlyphCount) {
                    if (Data.GlyphCount == 0) {
                        throw new System.InvalidOperationException("Can't define a valid end index, data is empty.");
                    }

                    throw new System.IndexOutOfRangeException($"End index out of valid range [0, {Data.GlyphCount - 1}]");
                }

                _endIndex = value;

                // recalculate content size
                int endRealIndex = _endIndex < 0 ? (Data.GlyphCount + _endIndex) : _endIndex;
                float minX = float.PositiveInfinity,
                      maxX = float.NegativeInfinity,
                      minY = float.PositiveInfinity,
                      maxY = float.NegativeInfinity;

                for (int i = StartIndex; i <= endRealIndex; i++) {
                    RenderData.Glyph g = Data[i];

                    if (g.Position.X < minX) {
                        minX = g.Position.X;
                    }

                    if (g.Position.X + g.Data.Width > maxX) {
                        maxX = g.Position.X + g.Data.Width;
                    }

                    // glyph size doesn't includes line size aspects
                    // to calculate correctly (at horizontal lines), we need to manually calculate line size
                    // using glypg's vertical values
                    int line = (int) Math.Floor(g.Position.Y / (Font.LineHeight + Font.SpaceBetweenLines));
                    float lineY = line * (Font.LineHeight + Font.SpaceBetweenLines);

                    if (lineY < minY) {
                        minY = lineY;
                    }

                    if (lineY + Font.LineHeight > maxY) {
                        maxY = lineY + Font.LineHeight;
                    }
                }

                _unscaledSize = new Size(maxX - minX, maxY - minY);
                Size = _unscaledSize * Scale;
            }
        }

        public RenderData Data { get; private set; }

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
                StartIndex,
                EndIndex < 0 ? (Data.GlyphCount + EndIndex + 1) : (EndIndex - StartIndex + 1),
                new Rectangle(position, _unscaledSize),
                rotation,
                scale,
                flip,
                color * Opacity,
                origin,
                scroll,
                shader,
                shaderParameters,
                layerDepth
            );
        }

        #endregion Protected Methods
    }
}
