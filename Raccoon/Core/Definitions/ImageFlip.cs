﻿using System;

namespace Raccoon {
    [Flags]
    public enum ImageFlip {
        None = 0,
        Horizontal = 1,
        Vertical = 1 << 1,
        Both = Horizontal | Vertical
    }
}
