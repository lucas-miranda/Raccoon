namespace Raccoon {
    public struct Point {
        #region Static Readonly

        public static readonly Point Zero = new Point(0, 0);

        #endregion Static Readonly

        #region Public Members

        public float X, Y;

        #endregion Public Members

        #region Constructors

        public Point(float x, float y) {
            X = x;
            Y = y;
        }

        public Point(float xy) {
            X = xy;
            Y = xy;
        }

        #endregion Constructors

        #region Public Methods

        public override bool Equals(object obj) {
            return obj is Point && Equals((Point) obj);
        }

        public bool Equals(Point p) {
            return this == p;
        }

        public override int GetHashCode() {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public Vector2 ToVector2() {
            return new Vector2(X, Y);
        }

        public override string ToString() {
            return $"[Point | X: {X}, Y: {Y}]";
        }

        #endregion Public Methods

        #region Implicit Conversions

        public static implicit operator Microsoft.Xna.Framework.Point(Point p) {
            return new Microsoft.Xna.Framework.Point((int) p.X, (int) p.Y);
        }

        #endregion Implicit Conversions

        #region Operators

        public static bool operator ==(Point l, Point r) {
            return l.X == r.X && l.Y == r.Y;
        }

        public static bool operator !=(Point l, Point r) {
            return !(l == r);
        }

        public static Point operator +(Point l, Vector2 r) {
            return new Point(l.X + r.X, l.Y + r.Y);
        }

        public static Point operator -(Point l, Vector2 r) {
            return l + (-r);
        }

        public static Point operator *(Point l, Vector2 r) {
            return new Point(l.X * r.X, l.Y * r.Y);
        }

        public static Point operator /(Point l, Vector2 r) {
            return new Point(l.X / r.X, l.Y / r.Y);
        }

        public static Point operator +(Point l, float v) {
            return new Point(l.X + v, l.Y + v);
        }

        public static Point operator *(Point l, float v) {
            return new Point(l.X * v, l.Y * v);
        }

        public static Point operator *(float v, Point l) {
            return l * v;
        }

        public static Point operator /(Point l, float v) {
            return new Point(l.X / v, l.Y / v);
        }

        #endregion Operators
    }
}
