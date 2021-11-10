using Raccoon.Fonts;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public partial class Text : Graphic, System.IDisposable {
        #region Private Members

        private Font _font;
        private string _value;
        private double _emWidth, _emHeight;
        private int _startIndex, _endIndex = -1;

        #endregion Private Members

        #region Constructors

        public Text(string value, Font font, Color color) {
            if (font == null || font.RenderMap == null) {
                throw new System.ArgumentException("Invalid Font.");
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

        public override Size Size {
            get {
                return new Size(
                    (float) (Font.ConvertEmToPx(_emWidth) * Scale.X),
                    (float) (Font.ConvertEmToPx(_emHeight) * Scale.Y)
                );
            }

            protected set {
                base.Size = value;
            }
        }

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
                double w = 0.0;

                for (int i = _startIndex; i <= endRealIndex; i++) {
                    RenderData.Glyph g = Data[i];

                    if (g.X + g.Data.Width > w) {
                        w = g.X + g.Data.Width;
                    }
                }

                RenderData.Glyph lastGlyph = Data[endRealIndex];

                _emWidth = w;
                _emHeight = (lastGlyph.OriginalY + lastGlyph.Data.Height) - Data[_startIndex].OriginalY;
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
                double minX = double.PositiveInfinity,
                       maxX = double.NegativeInfinity,
                       minY = double.PositiveInfinity,
                       maxY = double.NegativeInfinity;

                for (int i = StartIndex; i <= endRealIndex; i++) {
                    RenderData.Glyph g = Data[i];

                    if (g.X < minX) {
                        minX = g.X;
                    }

                    if (g.X + g.Data.Width > maxX) {
                        maxX = g.X + g.Data.Width;
                    }

                    // glyph size doesn't includes line size aspects
                    // to calculate correctly (at horizontal lines), we need to manually calculate line size
                    // using glypg's vertical values
                    int line = (int) Math.Floor(g.Y / Font.RenderMap.LineHeight);
                    double lineY = line * Font.RenderMap.LineHeight;

                    if (lineY < minY) {
                        minY = lineY;
                    }

                    if (lineY + Font.LineHeight > maxY) {
                        maxY = lineY + Font.RenderMap.LineHeight;
                    }
                }

                _emWidth = maxX - minX;
                _emHeight = maxY - minY;
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
                    Data = _font.RenderMap.PrepareTextRenderData(_value, out _emWidth, out _emHeight);
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
                Data = _font.RenderMap.PrepareTextRenderData(_value, out _emWidth, out _emHeight);
            }
        }

        public Size UnscaledSize {
            get {
                if (_font == null) {
                    return Size.Empty;
                }

                return new Size(
                    (float) Font.ConvertEmToPx(_emWidth),
                    (float) Font.ConvertEmToPx(_emHeight)
                );
            }
        }

        /// <summary>
        /// A special kind of scale which only applies to font resolution size itself.
        ///
        /// Some methods, such as MTSDF, can use a dynamic font scaling and any kind of view scaling which works as a transformation matrix, which we can't retrieve it's numeric value (e.g zoom) to properly calculate font size, should be applied here. Using it don't mess anything related to position or vertices scale, only font resolution.
        /// </summary>
        public float FontResolutionScale { get; set; } = 1.0f;

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
            if (Font == null || Data == null) {
                return;
            }

            if (shaderParameters == ShaderParameters) {
                if (ShaderParameters == null) {
                    if (Font.ShaderParameters != null) {
                        shaderParameters = ShaderParameters = new TextShaderParameters(Font) {
                            FontResolutionScale = FontResolutionScale,
                        };
                    }
                } else if (ShaderParameters is TextShaderParameters textShaderParameters) {
                    if (textShaderParameters.Font != Font) {
                        textShaderParameters.Font = Font;
                    }

                    textShaderParameters.FontResolutionScale = FontResolutionScale;
                }
            }

            Renderer.DrawString(
                Font,
                Data,
                StartIndex,
                EndIndex < 0 ? (Data.GlyphCount + EndIndex + 1) : (EndIndex - StartIndex + 1),
                new Rectangle(position, UnscaledSize),
                rotation,
                scale,
                flip,
                color * Opacity,
                origin,
                scroll,
                shader ?? Font.Shader,
                shaderParameters,
                layerDepth
            );
        }

        #endregion Protected Methods
    }
}
