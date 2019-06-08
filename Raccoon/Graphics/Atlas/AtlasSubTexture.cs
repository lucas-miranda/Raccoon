namespace Raccoon.Graphics {
    public class AtlasSubTexture {
        public AtlasSubTexture(Texture texture, Rectangle sourceRegion, Rectangle clippingRegion) {
            Texture = texture;
            SourceRegion = sourceRegion;
            ClippingRegion = clippingRegion;
            OriginalFrame = new Rectangle(clippingRegion.Size);
        }

        public Texture Texture { get; }
        public Rectangle SourceRegion { get; protected set; }
        public Rectangle ClippingRegion { get; protected set; }
        public Rectangle OriginalFrame { get; set; }
    }
}
