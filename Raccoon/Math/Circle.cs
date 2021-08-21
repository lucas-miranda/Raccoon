using Raccoon.Util;

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

        /// <summary>
        /// Checks Circle-Point intersection.
        /// </summary>
        /// <param name="v">Point to detect intersection.</param>
        /// <returns>True if intersection has been found, otherwise false.</returns>
        public bool Contains(Vector2 v) {
            return (v - Center).LengthSquared() <= Radius * Radius;
        }

        /// <summary>
        /// Checks Circle-Circle intersection.
        /// </summary>
        /// <param name="c">Circle to detect intersection.</param>
        /// <returns>True if intersection has been found, otherwise false.</returns>
        public bool Intersects(Circle c) {
            Vector2 centerDist = c.Center - Center;
            float radiusDiff = Radius - c.Radius;
            return centerDist.X * centerDist.X + centerDist.Y * centerDist.Y <= System.Math.Abs(radiusDiff * radiusDiff);
        }

        /// <summary>
        /// Checks Circle-Line intersection.
        /// </summary>
        /// <param name="line">Line to detect intersection.</param>
        /// <returns>True if intersection has been found, otherwise false.</returns>
        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Finds Circle-Line intersection points.
        /// See <see cref="Circle.Intersects(Line)"/> if simply the intersection is needed.
        /// </summary>
        /// <param name="line">Line to detect intersection.</param>
        /// <returns>Zero points if no intersection is detected; One point if just a point intersects; Two points if Line goes through Circle.</returns>
        public Vector2[] IntersectionPoints(Line line) {
            Vector2 m = line.PointA - Center;
            Vector2 d = (line.PointB - Center) - m;
            float a = Vector2.Dot(d, d),
                  b = 2f * Vector2.Dot(m, d),
                  c = Vector2.Dot(m, m) - Radius * Radius;

            float determinant = b * b - 4f * a *  c;
            if (determinant < 0.0f) {
                return new Vector2[0];
            }

            double sqrtDeterminant = System.Math.Sqrt(determinant);

            // root A
            double t1 = (-b - sqrtDeterminant) / (2.0 * a);

            // root B
            double t2 = (-b + sqrtDeterminant) / (2.0 * a);

            Vector2 p1 = Vector2.Lerp(line.PointA, line.PointB, (float) t1), // intersection point 1
                    p2 = Vector2.Lerp(line.PointA, line.PointB, (float) t2); // intersection point 2
            bool t1Insersect = 0.0 <= t1 && t1 <= 1.0,
                 t2Intersect = 0.0 <= t2 && t2 <= 1.0;
            if (t1Insersect && !t2Intersect) {
                return new Vector2[] { p1 };
            } else if (!t1Insersect && t2Intersect) {
                return new Vector2[] { p2 };
            } else if (!t1Insersect && !t2Intersect) {
                return new Vector2[0];
            }

            return new Vector2[] { p1, p2 };
        }

        public Vector2[] IntersectionPoints(Circle c) {
            float centerDist = (c.Center - Center).Length();
            if (Math.EqualsEstimate(centerDist, 0f) && Center == c.Center) {
                throw new System.InvalidOperationException("Coincident circles, can't calculate a discrete number of intersection points.");
            }

            if (centerDist > Radius + c.Radius || centerDist < Math.Abs(Radius - c.Radius)) {
                return new Vector2[0];
            }

            float a = (Radius * Radius - c.Radius * c.Radius + centerDist * centerDist) / (2.0f * centerDist), // radius length to touch intersection points segment base
                  halfSegmentBaseChordLength = Math.Sqrt(Radius * Radius - a * a);

            Vector2 segmentBaseCenter = Center + a * (c.Center - Center) / centerDist;

            Vector2 firstPoint = new Vector2(
                segmentBaseCenter.X + halfSegmentBaseChordLength * (c.Center.Y - Center.Y) / centerDist,
                segmentBaseCenter.Y - halfSegmentBaseChordLength * (c.Center.X - Center.X) / centerDist
            );

            Vector2 secondPoint = new Vector2(
                segmentBaseCenter.X - halfSegmentBaseChordLength * (c.Center.Y - Center.Y) / centerDist,
                segmentBaseCenter.Y + halfSegmentBaseChordLength * (c.Center.X - Center.X) / centerDist
            );

            if (firstPoint == secondPoint) {
                return new Vector2[] { firstPoint };
            }

            return new Vector2[] {
                firstPoint,
                secondPoint
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
