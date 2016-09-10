namespace Raccoon {
    public struct Circle {
        #region Static Readonly

        public static readonly Circle Empty = new Circle(new Vector2(0, 0), 0);

        #endregion Static Readonly

        #region Public Members

        public Vector2 Center;
        public float Radius;

        #endregion Public Members

        #region Constructors

        public Circle(Vector2 center, float radius) {
            Center = center;
            Radius = radius;
        }

        #endregion Constructors

        #region Public Properties

        public float Diameter { get { return Radius * 2; } }
        public float Circumference { get { return 2 * Math.PI * Radius; } }
        public float Top { get { return Center.Y - Radius; } }
        public float Right { get { return Center.X + Radius; } }
        public float Bottom { get { return Center.Y + Radius; } }
        public float Left { get { return Center.X - Radius; } }
        public bool IsEmpty { get { return Radius == 0; } }

        #endregion Public Properties

        #region Public Methods

        public bool Contains(Vector2 v) {
            return (v - Center).LengthSquared() <= Radius * Radius;
        }

        public bool Intersects(Circle c) {
            Vector2 centerDist = c.Center - Center;
            float radiusDiff = Radius - c.Radius;
            return centerDist.X * centerDist.X + centerDist.Y * centerDist.Y <= Math.Abs(radiusDiff * radiusDiff);
        }

        public override bool Equals(object obj) {
            return obj is Circle && Equals((Circle) obj);
        }

        public bool Equals(Circle r) {
            return this == r;
        }

        public override int GetHashCode() {
            return Center.GetHashCode() ^ Radius.GetHashCode();
        }

        public Rectangle ToRectangle() {
            return new Rectangle(Left, Top, Diameter, Diameter);
        }

        public override string ToString() {
            return $"[Circle | Center: {Center}, Radius: {Radius}]";
        }

        #endregion Public Methods

        #region Operators

        public static bool operator ==(Circle l, Circle r) {
            return l.Radius == r.Radius;
        }

        public static bool operator !=(Circle l, Circle r) {
            return !(l.Radius == r.Radius);
        }

        public static bool operator <(Circle l, Circle r) {
            return l.Radius < r.Radius;
        }

        public static bool operator >(Circle l, Circle r) {
            return l.Radius > r.Radius;
        }

        public static bool operator <=(Circle l, Circle r) {
            return !(l.Radius > r.Radius);
        }

        public static bool operator >=(Circle l, Circle r) {
            return !(l.Radius < r.Radius);
        }

        public static bool operator &(Circle l, Vector2 r) {
            return l.Contains(r);
        }

        public static bool operator &(Circle l, Circle r) {
            return l.Intersects(r);
        }

        #endregion Operators
    }
}
