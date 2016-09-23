using System;

namespace Raccoon {
    [Flags]
    public enum ImageFlip {
        None = 0,
        Horizontal,
        Vertical,
        Both = Horizontal | Vertical
    }
}
