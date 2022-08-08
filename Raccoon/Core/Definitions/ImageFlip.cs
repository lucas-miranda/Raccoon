using System;

namespace Raccoon {
    [Flags]
    public enum ImageFlip {
        None = 0,
        Horizontal = 1,
        Vertical = 1 << 1,
        Both = Horizontal | Vertical
    }

    public static class ImageFlipExtensions {
        public static bool IsHorizontal(this ImageFlip flip) {
            return (flip & ImageFlip.Horizontal) == ImageFlip.Horizontal;
        }

        public static bool IsVertical(this ImageFlip flip) {
            return (flip & ImageFlip.Vertical) == ImageFlip.Vertical;
        }

        public static bool IsBoth(this ImageFlip flip) {
            return flip == ImageFlip.Both;
        }

        public static bool IsNone(this ImageFlip flip) {
            return flip == ImageFlip.None;
        }
    }
}
