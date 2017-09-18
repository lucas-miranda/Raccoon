namespace Raccoon {
    public struct Circle {
        #region Static Readonly

        public static readonly Circle Empty = new Circle(0);
        public static readonly Circle Unit = new Circle(1);

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

        public Circle(float radius) : this(Vector2.Zero, radius) { }

        #endregion Constructors

        #region Public Properties

        public float Diameter { get { return Radius * 2f; } }
        public float Circumference { get { return (float) (2.0 * Util.Math.PI * Radius); } }
        public float Top { get { return Center.Y - Radius; } }
        public float Right { get { return Center.X + Radius; } }
        public float Bottom { get { return Center.Y + Radius; } }
        public float Left { get { return Center.X - Radius; } }
        public bool IsEmpty { get { return (int) Radius == 0; } }

        #endregion Public Properties

        #region Public Methods

        public bool Contains(Vector2 v) {
            return (v - Center).LengthSquared() <= Radius * Radius;
        }

        public bool Intersects(Circle c) {
            Vector2 centerDist = c.Center - Center;
            float radiusDiff = Radius - c.Radius;
            return centerDist.X * centerDist.X + centerDist.Y * centerDist.Y <= System.Math.Abs(radiusDiff * radiusDiff);
        }

        public Vector2[] IntersectionPoints(Line line) {
            Vector2 p1 = line.PointA - Center, p2 = line.PointB - Center;
            Vector2 dist = p2 - p1;

            float det = p1.X * p2.Y - p2.X * p1.Y;
            float distSquared = dist.LengthSquared();
            float discrimant = Radius * Radius * distSquared - det * det;

            if (discrimant < 0) {
                return new Vector2[0];
            }

            if (discrimant == 0) {
                return new Vector2[] { new Vector2(det * dist.Y / distSquared + Center.X, -det * dist.X / distSquared + Center.Y) };
            }

            double discrimantSqrt = System.Math.Sqrt(discrimant);
            float signal = dist.Y < 0 ? -1 : 1;

            return new Vector2[] {
                new Vector2((float) ((det * dist.Y + signal * dist.X * discrimantSqrt) / distSquared + Center.X), (float) ((-det * dist.X + System.Math.Abs(dist.Y) * discrimantSqrt) / distSquared + Center.Y)),
                new Vector2((float) ((det * dist.Y - signal * dist.X * discrimantSqrt) / distSquared + Center.X), (float) ((-det * dist.X - System.Math.Abs(dist.Y) * discrimantSqrt) / distSquared + Center.Y))
            };
        }

        public override bool Equals(object obj) {
            return obj is Circle && Equals((Circle) obj);
        }

        public bool Equals(Circle r) {
            return this == r;
        }

        public Rectangle ToRectangle() {
            return new Rectangle(Left, Top, Diameter, Diameter);
        }

        public override string ToString() {
            return $"[{Center} {Radius}]";
        }

        public override int GetHashCode() {
            var hashCode = 1641483799;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Center.GetHashCode();
            hashCode = hashCode * -1521134295 + Radius.GetHashCode();
            return hashCode;
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
