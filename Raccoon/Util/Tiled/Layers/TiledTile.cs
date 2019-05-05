namespace Raccoon.Tiled {
    public struct TiledTile {
        public const uint FlippedHorizontallyFlag = 0x80000000,
                          FlippedVerticallyFlag = 0x40000000,
                          FlippedDiagonallyFlag = 0x20000000,
                          FlippedAllFlags = FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag;

        public TiledTile(uint gid) {
            Flipped = ImageFlip.None;
            if ((gid & FlippedHorizontallyFlag) == FlippedHorizontallyFlag) {
                Flipped |= ImageFlip.Horizontal;
            }

            if ((gid & FlippedVerticallyFlag) == FlippedVerticallyFlag) {
                Flipped |= ImageFlip.Vertical;
            }

            IsFlippedDiagonally = (gid & FlippedDiagonallyFlag) == FlippedDiagonallyFlag;
            Id = gid & ~(FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag); // clear flags
        }

        public uint Id { get; }
        public uint Gid { get { return Id | (IsFlippedHorizontally ? FlippedHorizontallyFlag : 0) | (IsFlippedVertically ? FlippedVerticallyFlag : 0) | (IsFlippedDiagonally ? FlippedDiagonallyFlag : 0); } }
        public ImageFlip Flipped { get; }
        public bool IsFlippedHorizontally { get { return Flipped.HasFlag(ImageFlip.Horizontal); } }
        public bool IsFlippedVertically { get { return Flipped.HasFlag(ImageFlip.Vertical); } }
        public bool IsFlippedDiagonally { get; }
    }
}
