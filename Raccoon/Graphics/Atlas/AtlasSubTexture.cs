namespace Raccoon.Graphics {
    public class AtlasSubTexture : System.IDisposable {
        public AtlasSubTexture(Texture texture, Rectangle sourceRegion, Rectangle clippingRegion) {
            Texture = texture;
            SourceRegion = sourceRegion;
            ClippingRegion = clippingRegion;
            OriginalFrame = new Rectangle(clippingRegion.Size);
        }

        public Texture Texture { get; private set; }
        public Rectangle SourceRegion { get; protected set; }
        public Rectangle ClippingRegion { get; protected set; }
        public Rectangle OriginalFrame { get; set; }
        public bool IsDisposed { get; private set; }

        public virtual void Dispose() {
            if (IsDisposed) {
                return;
            }

            Texture = null;

            IsDisposed = true;
        }
    }
}
