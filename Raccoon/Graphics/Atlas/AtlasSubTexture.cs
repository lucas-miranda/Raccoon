namespace Raccoon.Graphics {
    public class AtlasSubTexture {
        public AtlasSubTexture(Texture texture, Rectangle region) {
            Texture = texture;
            Region = region;
        }

        public Texture Texture { get; }
        public Rectangle Region { get; protected set; }
    }
}
