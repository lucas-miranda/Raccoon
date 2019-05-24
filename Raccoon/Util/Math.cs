namespace Raccoon.Util {
    public static class Math {
        public const float Epsilon = 0.0001f;
        public const double PI = Microsoft.Xna.Framework.MathHelper.Pi;
        public const double DoublePI = 2.0 * PI;
        public const double TriplePI = 3.0 * PI;
        public const double HalfPI = PI / 2.0;
        public const double ThirdPI = PI / 3.0;
        public const double FourthPI = PI / 4.0;
        public static readonly double RadToDeg = 180.0 / PI;
        public static readonly double DegToRad = PI / 180.0;

        #region Numeric Stuff

        public static float Abs(float n) {
            return System.Math.Abs(n);
        }

        public static double Abs(double n) {
            return System.Math.Abs(n);
        }

        public static int Abs(int n) {
            return System.Math.Abs(n);
        }

        public static Vector2 Abs(Vector2 vec) {
            return new Vector2(Abs(vec.X), Abs(vec.Y));
        }

        public static float Approach(float start, float end, float amount) {
            return start < end ? Min(start + amount, end) : Max(start - amount, end);
        }

        public static int Approach(int start, int end, int amount) {
            return start < end ? Min(start + amount, end) : Max(start - amount, end);
        }

        public static Vector2 Approach(Vector2 start, Vector2 end, Vector2 amount) {
            return new Vector2(Approach(start.X, end.X, amount.X), Approach(start.Y, end.Y, amount.Y));
        }

        public static float Ceiling(float n) {
            return (float) System.Math.Ceiling(n);
        }

        public static double Ceiling(double n) {
            return System.Math.Ceiling(n);
        }

        public static Vector2 Ceiling(Vector2 value) {
            return new Vector2(Ceiling(value.X), Ceiling(value.Y));
        }

        public static float SignedCeiling(float n) {
            return Sign(n) < 0f ? Floor(n) : Ceiling(n);
        }

        public static double SignedCeiling(double n) {
            return Sign(n) < 0.0 ? Floor(n) : Ceiling(n);
        }

        public static Vector2 SignedCeiling(Vector2 value) {
            return new Vector2(SignedCeiling(value.X), SignedCeiling(value.Y));
        }

        public static float SignedFloor(float n) {
            return Sign(n) < 0f ? Ceiling(n) : Floor(n);
        }

        public static double SignedFloor(double n) {
            return Sign(n) < 0.0 ? Ceiling(n) : Floor(n);
        }

        public static Vector2 SignedFloor(Vector2 value) {
            return new Vector2(SignedFloor(value.X), SignedFloor(value.Y));
        }

        public static float Clamp(float value, float min, float max) {
            return Microsoft.Xna.Framework.MathHelper.Clamp(value, min, max);
        }

        public static int Clamp(int value, int min, int max) {
			value = (value > max) ? max : value;
			value = (value < min) ? min : value;
			return value;
        }

        public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max) {
            return new Vector2(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y));
        }

        public static Vector2 Clamp(Vector2 value, Rectangle rect) {
            return Clamp(value, rect.Position, rect.BottomRight - Vector2.One);
        }

        public static Rectangle Clamp(Rectangle inner, Rectangle outer) {
            return new Rectangle(Clamp(inner.TopLeft, outer.TopLeft, outer.BottomRight), Clamp(inner.BottomRight, outer.TopLeft, outer.BottomRight));
        }

        public static Size Clamp(Size value, Size min, Size max) {
            return new Size(Clamp(value.Width, min.Width, max.Width), Clamp(value.Height, min.Height, max.Height));
        }

        public static Vector2 CircleClamp(Vector2 value, float radius = 1f) {
            return PolarToCartesian(new Vector2(Clamp(value.Length(), -radius, radius), Angle(value)));
        }

        public static Vector2 CircleClamp(Vector2 value, Vector2 circleCenter, float radius = 1f) {
            value -= circleCenter;
            return circleCenter + PolarToCartesian(new Vector2(Clamp(value.Length(), -radius, radius), Angle(value)));
        }

        public static float DispersionNormalized(float value, float center) {
            return (value - center) / center;
        }

        public static Vector2 DispersionNormalized(Vector2 value, Vector2 center) {
            return (value - center) / center;
        }

        public static float Floor(float n) {
            return (float) System.Math.Floor(n);
        }

        public static double Floor(double n) {
            return System.Math.Floor(n);
        }

        public static Vector2 Floor(Vector2 value) {
            return new Vector2(Floor(value.X), Floor(value.Y));
        }

        public static float Lerp(float start, float end, float t) {
            return Microsoft.Xna.Framework.MathHelper.Lerp(start, end, t);
        }

        /*
        public static float LerpPrecise(float start, float end, float t) {
            return Microsoft.Xna.Framework.MathHelper.LerpPrecise(start, end, t);
        }
        */

        public static bool IsPowerOfTwo(int n) {
            return (n & (n - 1)) == 0;
        }

        public static bool IsPowerOfTwo(uint n) {
            return (n & (n - 1U)) == 0U;
        }

        public static bool IsPowerOfTwo(long n) {
            return (n & (n - 1L)) == 0L;
        }

        public static bool IsPowerOfTwo(ulong n) {
            return (n & (n - 1UL)) == 0UL;
        }

        public static int CeilingPowerOfTwo(int n) {
            // reference: https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;

            return n;
        }

        public static float Min(float n1, float n2) {
            return Microsoft.Xna.Framework.MathHelper.Min(n1, n2);
        }

        public static int Min(int n1, int n2) {
			return n1 < n2 ? n1 : n2;
        }

        public static int Max(int n1, int n2) {
			return n1 > n2 ? n1 : n2;
        }

        public static Vector2 Max(Vector2 v1, Vector2 v2) {
            return new Vector2(Max(v1.X, v2.X), Max(v1.Y, v2.Y));
        }

        public static Size Max(Size s1, Size s2) {
            return new Size(Max(s1.Width, s2.Width), Max(s1.Height, s2.Height));
        }

        public static Vector2 Min(Vector2 v1, Vector2 v2) {
            return new Vector2(Min(v1.X, v2.X), Min(v1.Y, v2.Y));
        }

        public static Size Min(Size s1, Size s2) {
            return new Size(Min(s1.Width, s2.Width), Min(s1.Height, s2.Height));
        }

        public static float Max(float n1, float n2) {
            return Microsoft.Xna.Framework.MathHelper.Max(n1, n2);
        }

        public static float NormalizeInRange(float value, float min, float max) {
            float average = min + (max - min) / 2f;
            return (value - average) / average;
        }

        public static Vector2 NormalizeInRange(Vector2 value, Vector2 min, Vector2 max) {
            Vector2 center = min + (max - min) / 2f;
            return (value - center) / center;
        }

        public static float Round(float n, System.MidpointRounding midpointRounding = System.MidpointRounding.ToEven) {
            return (float) System.Math.Round(n, midpointRounding);
        }

        public static double Round(double n, System.MidpointRounding midpointRounding = System.MidpointRounding.ToEven) {
            return System.Math.Round(n, midpointRounding);
        }

        public static Vector2 Round(Vector2 value, System.MidpointRounding midpointRounding = System.MidpointRounding.ToEven) {
            return new Vector2(Round(value.X, midpointRounding), Round(value.Y, midpointRounding));
        }

        public static int Sign(float n) {
            return System.Math.Sign(n);
        }

        public static int Sign(double n) {
            return System.Math.Sign(n);
        }

        public static int Sign(int n) {
            return System.Math.Sign(n);
        }

        public static float Truncate(float n) {
            return (float) System.Math.Truncate(n);
        }

        public static double Truncate(double n) {
            return System.Math.Truncate(n);
        }

        public static float Map(float value, float min, float max, float targetMin, float targetMax) {
            return targetMin + (value / (max - min)) * (targetMax - targetMin);
        }

        public static float Map(float value, Range range, Range targetRange) {
            return Map(value, range.Min, range.Max, targetRange.Min, targetRange.Max);
        }

        #region Comparison

        public static bool EqualsEstimate(float a, float b, float tolerance = Epsilon) {
            return Abs(a - b) < tolerance;
        }

        public static bool EqualsEstimate(double a, double b, double tolerance = Epsilon) {
            return Abs(a - b) < tolerance;
        }

        #endregion Comparison

        #endregion Numeric Stuff

        #region Geometry Stuff

        public static float ToRadians(float deg) {
            return (float) (deg * DegToRad);
        }

        public static float ToDegrees(float rad) {
            return (float) (rad * RadToDeg);
        }

        public static Vector2 CartesianToPolar(Vector2 cartesianPoint) {
            return new Vector2(cartesianPoint.Length(), Angle(cartesianPoint));
        }

        public static Vector2 CartesianToPolar(float x, float y) {
            return CartesianToPolar(new Vector2(x, y));
        }

        public static Vector2 PolarToCartesian(float radial, float angular) {
            return new Vector2(radial * Cos(angular), radial * Sin(angular));
        }

        public static Vector2 PolarToCartesian(Vector2 polarPoint) {
            return PolarToCartesian(polarPoint.X, polarPoint.Y);
        }

        public static float Sin(float deg) {
            return (float) System.Math.Sin(deg * DegToRad);
        }

        public static float Cos(float deg) {
            return (float) System.Math.Cos(deg * DegToRad);
        }

        public static float Tan(float deg) {
            return (float) System.Math.Tan(deg * DegToRad);
        }

        public static float Angle(float x, float y) {
            return (float) (System.Math.Atan2(y, x) * RadToDeg);
        }

        public static float Angle(Vector2 point) {
            return Angle(point.X, point.Y);
        }

        public static float Angle(float x1, float y1, float x2, float y2) {
            return (float) (System.Math.Atan2(y2 - y1, x2 - x1) * RadToDeg);
        }

        public static float Angle(Vector2 from, Vector2 to) {
            return Angle(from.X, from.Y, to.X, to.Y);
        }

        public static float Angle(Vector2 a, Vector2 b, Vector2 c) {
            double aX = a.X, aY = a.Y,
                   bX = b.X, bY = b.Y,
                   cX = c.X, cY = c.Y;

            double ba = (bX - aX) * (bX - aX) + (bY - aY) * (bY - aY),
                   bc = (bX - cX) * (bX - cX) + (bY - cY) * (bY - cY),
                   ca = (cX - aX) * (cX - aX) + (cY - aY) * (cY - aY);

            return (float) (System.Math.Acos((bc + ba - ca) / System.Math.Sqrt(4 * bc * ba)) * RadToDeg);
        }

        public static float AngleArc(Vector2 pointA, Vector2 pointB) {
            return Angle(pointA, Vector2.Zero, pointB);
        }

        public static float AngleArc(Vector2 origin, Vector2 pointA, Vector2 pointB) {
            return Angle(pointA, origin, pointB);
        }

        public static float WrapAngle(float angle) {
            angle = (float) (Microsoft.Xna.Framework.MathHelper.WrapAngle(ToRadians(angle)) * RadToDeg);
            return angle >= 0 ? angle : 360 + angle;
        }

        /// <summary>
        /// Subdivides into subsections and snaps angle to closest one.
        /// </summary>
        /// <param name="angle">Angle degrees.</param>
        /// <param name="angleSubdivision">Subdivision degrees.</param>
        /// <returns>Angle degrees snapped on a subdivision.</returns>
        public static float SnapAngle(float angle, float angleSubdivision) {
            angle = WrapAngle(angle);
            int sections = (int) (angle / angleSubdivision);
            return angleSubdivision * (sections + Round((angle % angleSubdivision) / angleSubdivision));
        }

        public static float Distance(float from, float to) {
            return System.Math.Abs(to - from);
        }

        public static float Distance(float x1, float y1, float x2, float y2) {
            return (float) System.Math.Sqrt(DistanceSquared(x1, y1, x2, y2));
        }

        public static double Distance(double x1, double y1, double x2, double y2) {
            return System.Math.Sqrt(DistanceSquared(x1, y1, x2, y2));
        }

        public static int Distance(int x1, int y1, int x2, int y2) {
            return (int) System.Math.Sqrt(DistanceSquared(x1, y1, x2, y2));
        }

        public static float Distance(Vector2 from, Vector2 to) {
            return (float) System.Math.Sqrt(DistanceSquared(from, to));
        }

        public static float Distance(Line line, Vector2 point) {
            return (float) System.Math.Sqrt(DistanceSquared(line, point));
        }

        public static float DistanceSquared(float x1, float y1, float x2, float y2) {
            return x1 * x2 + y1 * y2;
        }

        public static double DistanceSquared(double x1, double y1, double x2, double y2) {
            return x1 * x2 + y1 * y2;
        }

        public static int DistanceSquared(int x1, int y1, int x2, int y2) {
            return x1 * x2 + y1 * y2;
        }

        public static float DistanceSquared(Vector2 from, Vector2 to) {
            Vector2 diff = to - from;
            return Vector2.Dot(diff, diff);
        }

        public static float DistanceSquared(Line line, Vector2 point) {
            // implemented using http://stackoverflow.com/a/1501725
            float lengthSquared = line.LengthSquared;
            if (lengthSquared == 0) {
                return DistanceSquared(line.PointA, point);
            }

            float t = Clamp(Vector2.Dot(point - line.PointA, line.ToVector2()) / lengthSquared, 0, 1);
            Vector2 proj = line.GetPointNormalized(t);
            return DistanceSquared(point, proj);
        }

        public static Vector2 Rotate(Vector2 point, float degrees) {
            float cos = Cos(degrees), sin = Sin(degrees);
            return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
        }

        public static Vector2 RotateAround(Vector2 point, Vector2 origin, float degrees) {
            return origin + Rotate(point - origin, degrees);
        }

        public static float CatmullRom(float n1, float n2, float n3, float n4, float amount) {
            return Microsoft.Xna.Framework.MathHelper.CatmullRom(n1, n2, n3, n4, amount);
        }

        public static float Component(Vector2 v, Vector2 direction) {
            double alpha = System.Math.Atan2(direction.Y, direction.X);
            double theta = System.Math.Atan2(v.Y, v.X);
            return (float) (v.Length() * System.Math.Cos(theta - alpha));
        }

        public static Vector2 ComponentVector(Vector2 v, Vector2 direction) {
            return Component(v, direction) * direction.Normalized();
        }

        public static bool IsLeft(Vector2 a, Vector2 b, Vector2 c) {
            return Triangle.SignedArea2(a, b, c) > 0f;
        }

        public static bool IsLeftOn(Vector2 a, Vector2 b, Vector2 c) {
            return Triangle.SignedArea2(a, b, c) >= 0f;
        }

        public static bool IsRight(Vector2 a, Vector2 b, Vector2 c) {
            return Triangle.SignedArea2(a, b, c) < 0f;
        }

        public static bool IsRightOn(Vector2 a, Vector2 b, Vector2 c) {
            return Triangle.SignedArea2(a, b, c) <= 0f;
        }

        public static bool IsCollinear(Vector2 a, Vector2 b, Vector2 c, float tolerance = Epsilon) {
            return EqualsEstimate(Triangle.SignedArea2(a, b, c), 0f, tolerance);
        }

        public static Vector2 BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
            float iv_t = 1f - t;
            return iv_t * iv_t * iv_t * p0 + 3f * iv_t * iv_t * t * p1 + 3f * iv_t * t * t * p2 + t * t * t * p3;
        }

        public static Vector2 BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, float t) {
            return (1f - t) * (1f - t) * p0 + 2f * (1f - t) * t * p1 + t * t * p2;
        }

        #endregion Trygonometric Stuff
    }
}
