namespace Raccoon.Graphics {
    public struct AtlasAnimationFrame {
        public AtlasAnimationFrame(
            int globalIndex,
            int duration,
            Rectangle clippingRegion,
            Rectangle originalFrame
        ) {
            GlobalIndex = globalIndex;
            Duration = duration;
            ClippingRegion = clippingRegion;
            OriginalFrame = originalFrame;
        }

        public AtlasAnimationFrame(
            int globalIndex,
            int duration,
            Rectangle clippingRegion
        )
            : this(globalIndex, duration, clippingRegion, new Rectangle(clippingRegion.Size))
        {
        }

        public int GlobalIndex { get; }
        public int Duration { get; }
        public Rectangle ClippingRegion { get; }
        public Rectangle OriginalFrame { get; }
    }
}
