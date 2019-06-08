namespace Raccoon.Graphics {
    public struct AtlasAnimationFrame {
        public AtlasAnimationFrame(int duration, Rectangle clippingRegion, Rectangle originalFrame) {
            Duration = duration;
            ClippingRegion = clippingRegion;
            OriginalFrame = originalFrame;
        }

        public AtlasAnimationFrame(int duration, Rectangle clippingRegion) : this(duration, clippingRegion, new Rectangle(clippingRegion.Size)) {
        }

        public int Duration { get; }
        public Rectangle ClippingRegion { get; }
        public Rectangle OriginalFrame { get; }
    }
}
