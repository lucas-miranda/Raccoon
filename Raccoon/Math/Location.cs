
namespace Raccoon {
    public struct Location {
        public int X, Y;

        public Location(int x, int y) {
            X = x;
            Y = y;
        }

        public override int GetHashCode() {
            int hashCode = 486187739;

            unchecked {
                hashCode = hashCode * 23 + X.GetHashCode();
                hashCode = hashCode * 23 + Y.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object o) {
            return o is Location location && location.X == X && location.Y == Y;
        }

        public override string ToString() {
            return $"{X}, {Y}";
        }
    }
}
