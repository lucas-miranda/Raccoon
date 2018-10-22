namespace Raccoon.Graphics {
    public class AtlasSubTexture {
        public AtlasSubTexture(Texture texture, Rectangle sourceRegion) {
            Texture = texture;
            SourceRegion = sourceRegion;
        }

        public Texture Texture { get; }
        public Rectangle SourceRegion { get; protected set; }
    }
}
