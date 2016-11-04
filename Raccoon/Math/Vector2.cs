namespace Raccoon {
    public struct Vector2 {
        #region Static Readonly

        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);
        public static readonly Vector2 Up = new Vector2(0, -1);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Down = new Vector2(0, 1);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 UpLeft = new Vector2(-1, -1);
        public static readonly Vector2 UpRight = new Vector2(-1, 1);
        public static readonly Vector2 DownRight = new Vector2(1, 1);
        public static readonly Vector2 DownLeft = new Vector2(1, -1);

        #endregion Static Readonly          

        #region Public Members

        public float X, Y;

        #endregion Public Members

        #region Constructors

        public Vector2(float x, float y) {
            X = x;
            Y = y;
        }

        public Vector2(float xy) : this(xy, xy) {
        }

        internal Vector2(Microsoft.Xna.Framework.Vector2 vec2) : this(vec2.X, vec2.Y) { }

        #endregion Constructors

        #region Public Static Methods

        public static Vector2 Normalize(Vector2 v) {
            return v / v.Length();
        }

        #endregion Public Static Methods

        #region Public Methods

        public float Length() {
            return (float) System.Math.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared() {
            return X * X + Y * Y;
        }

        public void Normalize() {
            float n = 1 / Length();
            X *= n;
            Y *= n;
        }

        public override bool Equals(object obj) {
            return obj is Vector2 && Equals((Vector2) obj);
        }

        public bool Equals(Vector2 v) {
            return this == v;
        }

        public override int GetHashCode() {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public Direction ToDirection() {
            Direction dir = Direction.None;
            if (X > 0)
                dir |= Direction.Right;
            else if (X < 0)
                dir |= Direction.Left;

            if (Y > 0)
                dir |= Direction.Up;
            else if (Y < 0)
                dir |= Direction.Down;

            return dir;
        }

        public override string ToString() {
            return $"[Vector2 | X: {X}, Y: {Y}]";
        }

        #endregion Public Methods

        #region Implicit Conversions

        public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 v) {
            return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
        }

        #endregion Implicit Conversions

        #region Operators

        public static bool operator ==(Vector2 l, Vector2 r) {
            return l.X == r.X && l.Y == r.Y;
        }

        public static bool operator !=(Vector2 l, Vector2 r) {
            return !(l == r);
        }

        public static Vector2 operator -(Vector2 v) {
            return new Vector2(-v.X, -v.Y);
        }

        public static Vector2 operator +(Vector2 l, Vector2 r) {
            return new Vector2(l.X + r.X, l.Y + r.Y);
        }

        public static Rectangle operator +(Vector2 l, Rectangle r) {
            return r + l;
        }

        public static Vector2 operator -(Vector2 l, Vector2 r) {
            return l + (-r);
        }

        public static Vector2 operator *(Vector2 l, Vector2 r) {
            return new Vector2(l.X * r.X, l.Y * r.Y);
        }

        public static Vector2 operator /(Vector2 l, Vector2 r) {
            return new Vector2(l.X / r.X, l.Y / r.Y);
        }

        public static Vector2 operator +(Vector2 l, float v) {
            return new Vector2(l.X + v, l.Y + v);
        }

        public static Vector2 operator -(Vector2 l, float v) {
            return new Vector2(l.X - v, l.Y - v);
        }

        public static Vector2 operator *(Vector2 l, float v) {
            return new Vector2(l.X * v, l.Y * v);
        }

        public static Vector2 operator *(float v, Vector2 l) {
            return l * v;
        }

        public static Vector2 operator /(Vector2 l, float v) {
            return new Vector2(l.X / v, l.Y / v);
        }

        #endregion Operators
    }
}
