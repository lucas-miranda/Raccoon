using Raccoon.Graphics;

namespace Raccoon.Fonts {
    public class TextShaderParameters : IShaderParameters, IFontSizeShaderParameter {
        #region Private Members

        private float? _fontSize;

        #endregion Private Members

        #region Constructors

        public TextShaderParameters(Font font) {
            Font = font;
        }

        #endregion Constructors

        #region Public Properties

        public Font Font { get; set; }

        public float FontSize {
            get {
                if (_fontSize.HasValue) {
                    return _fontSize.Value;
                } else if (Font.ShaderParameters != null
                    && Font.ShaderParameters is IFontSizeShaderParameter fontSizeShaderParameter
                ) {
                    return fontSizeShaderParameter.FontSize;
                }

                return 0.0f;
            }

            set {
                if (value <= 0.0f) {
                    _fontSize = null;
                    return;
                }

                _fontSize = value;
            }
        }

        public float FontScale { get; set; } = 1.0f;

        /// <summary>
        /// A special kind of scale which only applies to font resolution size itself.
        ///
        /// Some methods, such as MTSDF, can use a dynamic font scaling and any kind of view scaling which works as a transformation matrix, which we can't retrieve it's numeric value (e.g zoom) to properly calculate font size, should be applied here. Using it don't mess anything related to position or vertices scale, only font resolution.
        /// </summary>
        public float FontResolutionScale { get; set; } = 1.0f;

        #endregion Public Properties

        #region Public Methods

        public void ApplyParameters(Shader shader) {
            if (shader == null) {
                throw new System.ArgumentNullException(nameof(shader));
            }

            if (Font == null || Font.ShaderParameters == null) {
                return;
            }

            if (Font.ShaderParameters is FontMTSDFShaderParameters fontMTSDFShaderParameters) {
                if (!(shader is FontMTSDFShader fontMTSDFShader)) {
                    throw new System.ArgumentException($"Expected '{nameof(FontMTSDFShader)}', but got '{shader.GetType().Name}' instead. (Because font shader parameters is '{nameof(FontMTSDFShaderParameters)}')");
                }

                fontMTSDFShaderParameters.ApplyParameters(shader);
                fontMTSDFShader.ScreenPixelRange =
                    fontMTSDFShaderParameters.CalculateScreenPixelRange(FontSize, FontScale);
            } else if (Font.ShaderParameters != null) {
                Font.ShaderParameters.ApplyParameters(shader);
            }
        }

        public IShaderParameters Clone() {
            return new TextShaderParameters(Font) {
                _fontSize = _fontSize,
                FontScale = FontScale
            };
        }

        public bool IsSafeFontScale(float fontSize, float fontScale) {
            if (Font == null || Font.ShaderParameters == null) {
                throw new System.InvalidOperationException($"{nameof(Font.ShaderParameters)} isn't defined.");
            }

            if (!(Font.ShaderParameters is IFontSizeShaderParameter fontSizeShaderParameter)) {
                throw new System.InvalidOperationException($"Current {nameof(Font.ShaderParameters)} doesn't implements {nameof(IFontSizeShaderParameter)}.");
            }

            return fontSizeShaderParameter.IsSafeFontScale(fontSize, fontScale);
        }

        public void SafeSetFontScale(float fontScale) {
            if (Font == null || Font.ShaderParameters == null) {
                throw new System.InvalidOperationException($"{nameof(Font.ShaderParameters)} isn't defined.");
            }

            if (!(Font.ShaderParameters is IFontSizeShaderParameter fontSizeShaderParameter)) {
                throw new System.InvalidOperationException($"Current {nameof(Font.ShaderParameters)} doesn't implements {nameof(IFontSizeShaderParameter)}.");
            }

            if (fontSizeShaderParameter.IsSafeFontScale(FontSize, fontScale)) {
                FontScale = fontScale;
            }
        }

        public bool Equals(IShaderParameters other) {
            return other != null
                && other is TextShaderParameters otherTextShaderParameters
                && otherTextShaderParameters.FontSize == FontSize
                && otherTextShaderParameters.FontScale == FontScale;
        }

        #endregion Public Methods
    }
}
