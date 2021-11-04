
namespace Raccoon.Fonts {
    public class FontTextureDataMetrics {
        public FontTextureDataMetrics(
            double emSize,
            float lineHeight,
            float ascender,
            float descender,
            float underlineY,
            float underlineThickness
        ) {
            EMSize = emSize;
            LineHeight = lineHeight;
            Ascender = ascender;
            Descender = descender;
            UnderlineY = underlineY;
            UnderlineThickness = underlineThickness;
        }

        public double EMSize { get; }
        public float LineHeight { get; }
        public float Ascender { get; }
        public float Descender { get; }
        public float UnderlineY { get; }
        public float UnderlineThickness { get; }
    }
}
