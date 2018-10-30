namespace Raccoon.Graphics {
    public struct AtlasAnimationFrame {
        public AtlasAnimationFrame(int duration, Rectangle clippingRegion) {
            Duration = duration;
            ClippingRegion = clippingRegion;
        }

        public int Duration { get; }
        public Rectangle ClippingRegion { get; }
    }
}
