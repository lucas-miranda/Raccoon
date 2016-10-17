namespace Raccoon.Tiled {
    public struct TiledTile {
        public const uint FlippedHorizontallyFlag = 0x80000000, FlippedVerticallyFlag = 0x40000000, FlippedDiagonallyFlag = 0x20000000;

        public int Gid;
        public ImageFlip Flipped;
        public bool FlippedDiagonally;

        public TiledTile(uint gid) {
            Flipped = ImageFlip.None;
            if ((gid & FlippedHorizontallyFlag) == FlippedHorizontallyFlag) {
                Flipped |= ImageFlip.Horizontal;
            }

            if ((gid & FlippedVerticallyFlag) == FlippedVerticallyFlag) {
                Flipped |= ImageFlip.Vertical;
            }

            FlippedDiagonally = (gid & FlippedDiagonallyFlag) == FlippedDiagonallyFlag;
            Gid = (int) (gid & ~(FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag)); // clear flags
        }

        public bool FlippedHorizontally { get { return Flipped.HasFlag(ImageFlip.Horizontal); } }
        public bool FlippedVertically { get { return Flipped.HasFlag(ImageFlip.Vertical); } }
    }
}
