using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon {
    public struct Line {
        #region Public Members

        public Vector2 PointA, PointB;

        #endregion Public Members

        #region Constructors

        public Line(Vector2 pointA, Vector2 pointB) {
            PointA = pointA;
            PointB = pointB;
            /*Slope = PointB.X - PointA.X != 0 ? (PointB.Y - PointA.Y) / (PointB.X - PointA.X) : float.PositiveInfinity;
            YIntercept = PointB.X - PointA.X != 0 ? (PointA.Y * PointB.X - PointA.X * PointB.Y) / (PointB.X - PointA.X) : float.PositiveInfinity;*/
            /*A = PointA.Y - PointB.Y;
            B = PointB.X - PointA.X;
            C = (PointA.X - PointB.X) * PointA.Y + (PointB.Y - PointA.Y) * PointB.Y;*/
        }

        /*public Line(float a, float b, float c) {
            A = a;
            B = b;
            C = c;
            Slope = B == 0 ? float.PositiveInfinity : - A / B;
            YIntercept = B == 0 ? float.PositiveInfinity : - C / B;
            float XIntercept = - C / A;
            PointA = new Vector2(XIntercept, B == 0 ? 0 : YIntercept);
            PointB = new Vector2(float.IsInfinity(Slope) ? XIntercept : XIntercept + 1, B == 0 ? 1 : Slope + YIntercept);
        }*/

        /*public Line(float slope, float yIntercept) {
            if (float.IsInfinity(slope)) throw new System.ArgumentException("Slope value can't be infinity", "slope");

            Slope = slope;
            YIntercept = yIntercept;
            PointA = new Vector2(0, YIntercept);
            PointB = new Vector2(1, Slope + YIntercept);
            A = PointA.Y - PointB.Y;
            B = PointB.X - PointA.X;
            C = (PointA.X - PointB.X) * PointA.Y + (PointB.Y - PointA.Y) * PointB.Y;
        }*/

        #endregion Constructors

        #region Public Properties

        public float Length { get { return Math.Distance(PointA, PointB); } }
        public float LengthSquared { get { return Math.DistanceSquared(PointA, PointB); } }
        public float Slope { get { return PointB.X - PointA.X != 0 ? (PointB.Y - PointA.Y) / (PointB.X - PointA.X) : float.PositiveInfinity; } }
        public float YIntercept { get { return PointB.X - PointA.X != 0 ? (PointA.Y * PointB.X - PointA.X * PointB.Y) / (PointB.X - PointA.X) : float.PositiveInfinity; } }
        public float Angle { get { return Math.WrapAngle(Math.Angle(PointA, PointB)); } }
        public Vector2 MidPoint { get { return GetPointNormalized(.5f); } }
        /*public float M { get { return Slope; } }
        public float A { get; }
        public float B { get; }
        public float C { get; }*/

        #endregion Public Properties

        #region Public Methods

        public Vector2 GetPoint(float x) {
            return new Vector2(x, PointB.X - PointA.X == 0 ? float.PositiveInfinity : Slope * x + YIntercept);
        }

        public Vector2 GetPointNormalized(float t) {
            return PointA + t * ToVector2();
        }

        public Vector2 GetProjectionPoint(Vector2 p) {
            Vector2 lineVec = ToVector2(),
                    normal = lineVec.Normalized();

            return PointA + normal * normal.Projection(p - PointA);
        }

        public bool IntersectionPoint(Line line, out Vector2 intersectionPoint) {
            // reference:  https://gamedev.stackexchange.com/a/12246
            float a1 = SignedTriangleArea(PointA, PointB, line.PointB),
                  a2 = SignedTriangleArea(PointA, PointB, line.PointA);

            if (a1 * a2 < .0f) {
                float a3 = SignedTriangleArea(line.PointA, line.PointB, PointA);
                float a4 = a3 + a2 - a1;

                if (a3 * a4 < .0f) {
                    float t = a3 / (a3 - a4);
                    intersectionPoint = GetPointNormalized(t);
                    return true;
                }
            }

            // test for collinear intersection range
            // reference: https://stackoverflow.com/a/565282
            // BUG: not working well
            /*Vector2 r = ToVector2(), s = line.ToVector2();
            if (Vector2.Cross(r, s) == 0 && Vector2.Cross(line.PointA - PointA, r) == 0) {
                float t0 = (line.PointA - PointA).Dot(r / (r.Dot(r)));
                float t1 = t0 + s.Dot(r / r.Dot(r));
                Range range = s.Dot(r) < 0 ? new Range(t1, t0) : new Range(t0, t1);
                range.Overlaps(new Range(0, 1), out float t);
                return PointA + t * r;
            }*/

            intersectionPoint = default(Vector2);
            return false;

            float SignedTriangleArea(Vector2 a, Vector2 b, Vector2 c) {
                return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
            }
        }

        public bool Intersects(Line line) {
            return IntersectionPoint(line, out _);
        }

        public bool IntersectionPoint(Rectangle rectangle, out Vector2 intersectionPoint) {
            // ref: https://gist.github.com/ChickenProp/3194723
            Vector2 v = ToVector2();

            float[] p = new float[] {
                        -v.X, v.X, -v.Y, v.Y
                    },
                    q = new float[] {
                        PointA.X - rectangle.Left,
                        rectangle.Right - PointA.X,
                        PointA.Y - rectangle.Top,
                        rectangle.Bottom - PointA.Y
                    };

            float u1 = float.NegativeInfinity,
                  u2 = float.PositiveInfinity;

            for (int i = 0; i < 4; i++) {
                if (p[i] == 0) {
                    if (q[i] < 0) {
                        intersectionPoint = default;
                        return false;
                    }
                } else {
                    float t = q[i] / p[i];
                    if (p[i] < 0 && u1 < t) {
                        u1 = t;
                    } else if (p[i] > 0 && u2 > t) {
                        u2 = t;
                    }
                }
            }

            if (u1 > u2 || u1 > 1 || u1 < 0) {
                intersectionPoint = default;
                return false;
            }

            intersectionPoint = new Vector2(PointA.X + u1 * v.X, PointA.Y + u1 * v.Y);
            return true;
        }

        public bool Intersects(Rectangle rectangle) {
            return IntersectionPoint(rectangle, out _);
        }

        public Vector2 ClosestPoint(Vector2 point) {
            Vector2 ab = ToVector2();
            float t = Math.Clamp(Vector2.Dot(point - PointA, ab) / Vector2.Dot(ab, ab), 0.0f, 1.0f);
            return GetPointNormalized(t);
        }

        public float Distance(Vector2 point) {
            return Math.Distance(ClosestPoint(point), point);
        }

        public float DistanceSquared(Vector2 point) {
            return Math.DistanceSquared(ClosestPoint(point), point);
        }

        public int Side(Vector2 point) {
            float cross = Vector2.Cross(PointB - PointA, point - PointA);
            if (cross == 0) {
                return 0;
            }

            return cross < 0 ? -1 : 1;
        }

        public Range Projection(ICollection<Vector2> points) {
            return ToVector2().Projection(points);
        }

        public Range Projection(params Vector2[] points) {
            return Projection(points as ICollection<Vector2>);
        }

        public float Projection(Vector2 point) {
            return ToVector2().Projection(point);
        }

        public Vector2 ToVector2() {
            return PointB - PointA;
        }

        #endregion Public Methods
    }
}
