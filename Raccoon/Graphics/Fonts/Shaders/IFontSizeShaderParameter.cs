
namespace Raccoon.Fonts {
    public interface IFontSizeShaderParameter {
        float FontSize { get; set; }
        float FontScale { get; set; }

        bool IsSafeFontScale(float fontSize, float fontScale);
        void SafeSetFontScale(float fontScale);
    }
}
