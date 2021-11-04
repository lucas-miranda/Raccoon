namespace Raccoon.Fonts {
    public class FontTextureDataGlyph {
        public FontTextureDataGlyph(
            uint unicode,
            double advance
        ) {
            Unicode = unicode;
            Advance = advance;
            PlaneBounds = new FontTextureDataGlyphBounds();
            AtlasBounds = new FontTextureDataGlyphBounds();
        }

        public FontTextureDataGlyph(
            uint unicode,
            double advance,
            FontTextureDataGlyphBounds planeBounds,
            FontTextureDataGlyphBounds atlasBounds
        ) {
            Unicode = unicode;
            Advance = advance;
            PlaneBounds = planeBounds;
            AtlasBounds = atlasBounds;
        }

        public uint Unicode { get; }
        public double Advance { get; }
        public FontTextureDataGlyphBounds PlaneBounds { get; }
        public FontTextureDataGlyphBounds AtlasBounds { get; }
    }
}
