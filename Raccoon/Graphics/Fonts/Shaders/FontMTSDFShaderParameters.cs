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

        #endregion Public Properties

        #region Public Methods

        public void ApplyParameters(Shader shader) {
            if (shader == null) {
                throw new System.ArgumentNullException(nameof(shader));
            }

            if (!(shader is FontMTSDFShader fontMTSDFShader)) {
                throw new System.ArgumentException($"Expected '{nameof(FontMTSDFShader)}', but got '{shader.GetType().Name}' instead.");
            }

            fontMTSDFShader.ScreenPixelRange = (FontSize / FontBaseSize) * PixelDistanceRange;
        }

        public IShaderParameters Clone() {
            return new FontMTSDFShaderParameters(FontBaseSize, PixelDistanceRange) {
                FontSize = FontSize
            };
        }

        public bool Equals(IShaderParameters other) {
            return other != null
                && other is FontMTSDFShaderParameters otherFontMTSDFShaderParameters
                && otherFontMTSDFShaderParameters.PixelDistanceRange == PixelDistanceRange
                && otherFontMTSDFShaderParameters.FontBaseSize == FontBaseSize
                && otherFontMTSDFShaderParameters.FontSize == FontSize;
        }

        #endregion Public Methods
    }
}
