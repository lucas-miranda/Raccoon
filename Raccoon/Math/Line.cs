using System;

namespace Raccoon {
    public struct Line {
        public Line(Vector2 pointA, Vector2 pointB) {
            PointA = pointA;
            PointB = pointB;
            Slope = pointB.X - pointA.X != 0 ? (pointB.Y - pointA.Y) / (pointB.X - pointA.X) : float.PositiveInfinity;
            YIntercept = PointB.X - PointA.X != 0 ? (PointA.Y * PointB.X - PointA.X * PointB.Y) / (PointB.X - PointA.X) : float.PositiveInfinity;
            A = PointA.Y - PointB.Y;
            B = PointB.X - PointA.X;
            C = (PointA.X - PointB.X) * PointA.Y + (PointB.Y - PointA.Y) * PointB.Y;
        }

        public Line(float a, float b, float c) {
            A = a;
            B = b;
            C = c;
            Slope = B == 0 ? float.PositiveInfinity : - A / B;
            YIntercept = B == 0 ? float.PositiveInfinity : - C / B;
            float XIntercept = - C / A;
            PointA = new Vector2(XIntercept, B == 0 ? 0 : YIntercept);
            PointB = new Vector2(float.IsInfinity(Slope) ? XIntercept : XIntercept + 1, B == 0 ? 1 : Slope + YIntercept);
        }

        public Line(float slope, float yIntercept) {
            if (float.IsInfinity(slope)) throw new ArgumentException("Slope value can't be infinity", "slope");

            Slope = slope;
            YIntercept = yIntercept;
            PointA = new Vector2(0, YIntercept);
            PointB = new Vector2(1, Slope + YIntercept);
            A = PointA.Y - PointB.Y;
            B = PointB.X - PointA.X;
            C = (PointA.X - PointB.X) * PointA.Y + (PointB.Y - PointA.Y) * PointB.Y;
        }

        public Vector2 PointA { get; }
        public Vector2 PointB { get; }
        public float Length { get { return Util.Math.Distance(PointA, PointB); } }
        public float LengthSquared { get { return Util.Math.DistanceSquared(PointA, PointB); } }
        public float Slope { get; }
        public float YIntercept { get; }
        public float M { get { return Slope; } }
        public float A { get; }
        public float B { get; }
        public float C { get; }

        public Vector2 GetPoint(float x) {
            return new Vector2(x, B == 0 ? float.PositiveInfinity : Slope * x + YIntercept);
        }
    }
}
