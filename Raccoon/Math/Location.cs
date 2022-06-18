
namespace Raccoon {
    public struct Location {
        public static readonly Location Zero = new Location(0, 0),
                                        Unit = new Location(1, 1),
                                        UnitX = new Location(1, 0),
                                        UnitY = new Location(0, 1);

        public int X, Y;

        public Location(int x, int y) {
            X = x;
            Y = y;
        }

        public Location PerpendicularCCW() {
            return new Location(Y, -X);
        }

        public Location PerpendicularCW() {
            return new Location(-Y, X);
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

        public static bool operator ==(Location l, Location r) {
            return l.X == r.X && l.Y == r.Y;
        }

        public static bool operator !=(Location l, Location r) {
            return l.X != r.X || l.Y != r.Y;
        }

        public static Location operator -(Location l) {
            return new Location(-l.X, -l.Y);
        }

        public static Location operator +(Location l, Location r) {
            return new Location(l.X + r.X, l.Y + r.Y);
        }

        public static Location operator -(Location l, Location r) {
            return new Location(l.X - r.X, l.Y - r.Y);
        }

        public static Location operator *(Location l, Location r) {
            return new Location(l.X * r.X, l.Y * r.Y);
        }

        public static Location operator *(Location l, int v) {
            return new Location(l.X * v, l.Y * v);
        }

        public static Location operator *(int v, Location r) {
            return new Location(v * r.X, v * r.Y);
        }
    }
}
