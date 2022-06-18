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

        public bool IntersectionPoints(Line line, out Vector2[] intersectionPoint) {
            // ref: https://cp-algorithms.com/geometry/segments-intersection.html
            //      https://github.com/e-maxx-eng/e-maxx-eng/blob/e1c68001df7a081e7f70cfc3afdd0b9d4ea75809/src/geometry/segments-intersection.md

            if (!Intersect1D(PointA.X, PointB.X, line.PointA.X, line.PointB.X) || !Intersect1D(PointA.Y, PointB.Y, line.PointA.Y, line.PointB.Y)) {
                intersectionPoint = null;
                return false;
            }

            float d_a = PointA.Y - PointB.Y,
                  d_b = PointB.X - PointA.X,
                  d_c = -d_a * PointA.X - d_b * PointA.Y,

                  d_A = line.PointA.Y - line.PointB.Y,
                  d_B = line.PointB.X - line.PointA.X,
                  d_C = -d_A * line.PointA.X - d_B * line.PointA.Y;

            float d_z = (float) Math.Sqrt(d_a * d_a + d_b * d_b);
            if (Math.Abs(d_z) > Math.Epsilon) {
                d_a /= d_z;
                d_b /= d_z;
                d_c /= d_z;
            }

            float d_Z = (float) Math.Sqrt(d_A * d_A + d_B * d_B);
            if (Math.Abs(d_Z) > Math.Epsilon) {
                d_A /= d_Z;
                d_B /= d_Z;
                d_C /= d_Z;
            }

            float zn = d_a * d_B - d_b * d_A;

            if (Math.Abs(zn) < Math.Epsilon) {
                if (Math.Abs(d_a * line.PointA.X + d_b * line.PointA.Y + d_c) > Math.Epsilon
                 || Math.Abs(d_A * PointA.X + d_B * PointA.Y + d_C) > Math.Epsilon
                ) {
                    intersectionPoint = null;
                    return false;
                }

                Vector2 p_a = PointA,
                        p_b = PointB,
                        p_c = line.PointA,
                        p_d = line.PointB,
                        p_t;

                if (PointIsSmaller(p_b, p_a)) {
                    p_t = p_a;
                    p_a = p_b;
                    p_b = p_t;
                }

                if (PointIsSmaller(p_d, p_c)) {
                    p_t = p_d;
                    p_d = p_c;
                    p_c = p_t;
                }

                intersectionPoint = new Vector2[] {
                    MaxPoint(p_a, p_c),
                    MinPoint(p_b, p_d)
                };

                return true;
            }

            intersectionPoint = new Vector2[] {
                new Vector2(
                    -(d_c * d_B - d_b * d_C) / zn,
                    -(d_a * d_C - d_c * d_A) / zn
                )
            };

            return Between(PointA.X, PointB.X, intersectionPoint[0].X)
                && Between(PointA.Y, PointB.Y, intersectionPoint[0].Y)
                && Between(line.PointA.X, line.PointB.X, intersectionPoint[0].X)
                && Between(line.PointA.Y, line.PointB.Y, intersectionPoint[0].Y);
        }

        public bool IntersectionPoint(Line line, out Vector2 intersectionPoint) {
            if (IntersectionPoints(line, out Vector2[] intersectionPoints)) {
                intersectionPoint = intersectionPoints[0];
                return true;
            }

            intersectionPoint = Vector2.Zero;
            return false;
        }

        public bool Intersects(Line line) {
            return IntersectionPoint(line, out _);
        }

        public bool IntersectionPoint(Rectangle rectangle, out Vector2 intersectionPoint) {
            foreach (Line edge in rectangle.Edges()) {
                if (IntersectionPoint(edge, out intersectionPoint)) {
                    return true;
                }
            }

            intersectionPoint = default;
            return false;
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

        public override string ToString() {
            return $"[A: {PointA}, B: {PointB}]";
        }

        #endregion Public Methods

        #region Private Methods

        private float SignedTriangleArea(Vector2 a, Vector2 b, Vector2 c) {
            return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
        }

        private bool Intersect1D(float a, float b, float c, float d) {
            float t;
            if (a > b) {
                t = a;
                a = b;
                b = t;
            }

            if (c > d) {
                t = c;
                c = d;
                d = t;
            }

            return Math.Max(a, c) <= Math.Min(b, d) + Math.Epsilon;
        }

        private Vector2 MaxPoint(Vector2 a, Vector2 b) {
            return PointIsSmaller(a, b) ? b : a;
        }

        private Vector2 MinPoint(Vector2 a, Vector2 b) {
            return PointIsSmaller(a, b) ? a : b;
        }

        private bool PointIsSmaller(Vector2 a, Vector2 b) {
            return a.X < b.X - Math.Epsilon
                || (Math.Abs(a.X - b.X) < Math.Epsilon && a.Y < b.Y - Math.Epsilon);
        }

        private bool Between(float l, float r, float x) {
            return Math.Min(l, r) <= x + Math.Epsilon && x <= Math.Max(l, r) + Math.Epsilon;
        }

        #endregion Private Methods
    }
}
