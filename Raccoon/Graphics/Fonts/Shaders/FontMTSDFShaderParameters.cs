using Raccoon.Graphics;

namespace Raccoon.Fonts {
    public class FontMTSDFShaderParameters : IShaderParameters, IFontSizeShaderParameter {
        #region Constructors

        public FontMTSDFShaderParameters(float fontBaseSize, float pixelDistanceRange) {
            if (fontBaseSize <= 0) {
                throw new System.ArgumentException($"Font base size must be a positive number.");
            }

            PixelDistanceRange = pixelDistanceRange;
            FontSize = FontBaseSize = fontBaseSize;
        }

        #endregion Constructors

        #region Public Properties

        public float PixelDistanceRange { get; }
        public float FontBaseSize { get; }
        public float FontSize { get; set; }
        public float FontScale { get; set; } = 1.0f;

        public float ScreenPixelRange {
            get {
                return ((FontScale * FontSize) / FontBaseSize) * PixelDistanceRange;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void ApplyParameters(Shader shader) {
            if (shader == null) {
                throw new System.ArgumentNullException(nameof(shader));
            }

            if (!(shader is FontMTSDFShader fontMTSDFShader)) {
                throw new System.ArgumentException($"Expected '{nameof(FontMTSDFShader)}', but got '{shader.GetType().Name}' instead.");
            }

            fontMTSDFShader.ScreenPixelRange = ScreenPixelRange;
        }

        public IShaderParameters Clone() {
            return new FontMTSDFShaderParameters(FontBaseSize, PixelDistanceRange) {
                FontSize = FontSize,
                FontScale = FontScale
            };
        }

        public float CalculateScreenPixelRange(float fontSize, float fontScale) {
            return ((fontScale * fontSize) / FontBaseSize) * PixelDistanceRange;
        }

        public bool IsSafeFontScale(float fontSize, float fontScale) {
            return CalculateScreenPixelRange(fontSize, fontScale) >= FontMTSDFShader.SafeMinScreenPixelRange;
        }

        public void SafeSetFontScale(float fontScale) {
            if (IsSafeFontScale(FontSize, fontScale)) {
                FontScale = fontScale;
            }
        }

        public bool Equals(IShaderParameters other) {
            return other != null
                && other is FontMTSDFShaderParameters otherFontMTSDFShaderParameters
                && otherFontMTSDFShaderParameters.PixelDistanceRange == PixelDistanceRange
                && otherFontMTSDFShaderParameters.FontBaseSize == FontBaseSize
                && otherFontMTSDFShaderParameters.FontSize == FontSize
                && otherFontMTSDFShaderParameters.FontScale == FontScale;
        }

        #endregion Public Methods
    }
}
