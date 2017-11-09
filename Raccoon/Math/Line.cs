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

        public bool Intersects(Line line) {
            return IntersectionPoint(line) != null;
        }

        public Vector2? IntersectionPoint(Line line) {
            // reference:  https://gamedev.stackexchange.com/a/12246
            float a1 = SignedTriangleArea(PointA, PointB, line.PointB),
                  a2 = SignedTriangleArea(PointA, PointB, line.PointA);

            if (a1 * a2 < .0f) {
                float a3 = SignedTriangleArea(line.PointA, line.PointB, PointA);
                float a4 = a3 + a2 - a1;

                if (a3 * a4 < .0f) {
                    float t = a3 / (a3 - a4);
                    return GetPointNormalized(t);
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

            return null;

            float SignedTriangleArea(Vector2 a, Vector2 b, Vector2 c) {
                return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
            }
        }

        public Range Projection(ICollection<Vector2> points) {
            return ToVector2().Projection(points);
        }

        public Range Projection(params Vector2[] points) {
            return Projection(points as ICollection<Vector2>);
        }

        public Vector2 ToVector2() {
            return PointB - PointA;
        }

        #endregion Public Methods
    }
}
