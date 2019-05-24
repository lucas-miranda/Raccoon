namespace Raccoon.Util {
    public static class Bit {
        public static void Set(ref int flags, int bit) {
            flags |= 1 << bit;
        }

        public static void Set(ref uint flags, int bit) {
            flags |= (uint) 1 << bit;
        }

        public static void Clear(ref int flags, int bit) {
            flags &= ~(1 << bit);
        }

        public static void Clear(ref uint flags, int bit) {
            flags &= ~((uint) 1 << bit);
        }

        public static bool HasSet(ref int flags, int bit) {
            return (flags & (1 << bit)) != 0;
        }

        public static bool HasSet(ref uint flags, int bit) {
            return (flags & ((uint) 1 << bit)) != 0;
        }
    }
}
