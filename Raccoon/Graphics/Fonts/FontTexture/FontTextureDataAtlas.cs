
namespace Raccoon.Fonts {
    public class FontTextureDataAtlas {
        public FontTextureDataAtlas(
            FontTextureAtlasKind kind,
            float distanceRange,
            float size,
            float width,
            float height,
            FontTextureYOriginKind yOrigin
        ) {
            Kind = kind;
            DistanceRange = distanceRange;
            Size = size;
            Width = width;
            Height = height;
            YOrigin = yOrigin;
        }

        public FontTextureAtlasKind Kind { get; }
        public float DistanceRange { get; }
        public float Size { get; }
        public float Width { get; }
        public float Height { get; }
        public FontTextureYOriginKind YOrigin { get; }
    }
}
