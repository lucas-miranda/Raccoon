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

        public static Vector2 Abs(Vector2 vec) {
            return new Vector2(System.Math.Abs(vec.X), System.Math.Abs(vec.Y));
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

        public static Vector2 Ceiling(Vector2 value) {
            return new Vector2((float) System.Math.Ceiling(value.X), (float) System.Math.Ceiling(value.Y));
        }

        public static float Clamp(float value, float min, float max) {
            return Microsoft.Xna.Framework.MathHelper.Clamp(value, min, max);
        }

        public static int Clamp(int value, int min, int max) {
            return Microsoft.Xna.Framework.MathHelper.Clamp(value, min, max);
        }

        public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max) {
            return new Vector2(Clamp(value.X, min.X, max.X), Clamp(value.Y, min.Y, max.Y));
        }

        public static Vector2 Clamp(Vector2 value, Rectangle rect) {
            return Clamp(value, rect.Position, new Vector2(rect.Right, rect.Bottom));
        }

        public static Rectangle Clamp(Rectangle inner, Rectangle outer) {
            return new Rectangle(Clamp(inner.TopLeft, outer.TopLeft, outer.BottomRight), Clamp(inner.BottomRight, outer.TopLeft, outer.BottomRight));
        }

        public static Vector2 CircleClamp(Vector2 value, float radius = 1f) {
            return PolarToCartesian(new Vector2(Clamp(value.Length(), -radius, radius), Angle(value)));
        }

        public static float DispersionNormalized(float value, float center) {
            return (value - center) / center;
        }

        public static Vector2 DispersionNormalized(Vector2 value, Vector2 center) {
            return (value - center) / center;
        }

        public static Vector2 Floor(Vector2 value) {
            return new Vector2((float) System.Math.Floor(value.X), (float) System.Math.Floor(value.Y));
        }

        public static float Lerp(float start, float end, float t) {
            return Microsoft.Xna.Framework.MathHelper.Lerp(start, end, t);
        }

        public static float LerpPrecise(float start, float end, float t) {
            return Microsoft.Xna.Framework.MathHelper.LerpPrecise(start, end, t);
        }

        public static float Min(float n1, float n2) {
            return Microsoft.Xna.Framework.MathHelper.Min(n1, n2);
        }

        public static int Min(int n1, int n2) {
            return Microsoft.Xna.Framework.MathHelper.Min(n1, n2);
        }

        public static int Max(int n1, int n2) {
            return Microsoft.Xna.Framework.MathHelper.Max(n1, n2);
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
            float ba = (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y),
                  bc = (b.X - c.X) * (b.X - c.X) + (b.Y - c.Y) * (b.Y - c.Y),
                  ca = (c.X - a.X) * (c.X - a.X) + (c.Y - a.Y) * (c.Y - a.Y);

            return (float) (System.Math.Acos((bc + ba - ca) / System.Math.Sqrt(4 * bc * ba)) * RadToDeg);
        }

        public static float WrapAngle(float angle) {
            angle = (float) (Microsoft.Xna.Framework.MathHelper.WrapAngle(ToRadians(angle)) * RadToDeg);
            return angle >= 0 ? angle : 360 + angle;
        }

        public static float Distance(float from, float to) {
            return System.Math.Abs(to - from);
        }

        public static float Distance(Vector2 from, Vector2 to) {
            return (float) System.Math.Sqrt(DistanceSquared(from, to));
        }

        public static float Distance(Line line, Vector2 point) {
            return (float) System.Math.Sqrt(DistanceSquared(line, point));
        }

        public static float DistanceSquared(Vector2 from, Vector2 to) {
            return (to - from).LengthSquared();
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
            float cos = Cos(degrees), sin = Sin(degrees);
            point -= origin;
            return origin + new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
        }

        public static float CatmullRom(float n1, float n2, float n3, float n4, float amount) {
            return Microsoft.Xna.Framework.MathHelper.CatmullRom(n1, n2, n3, n4, amount);
        }

        #endregion Trygonometric Stuff
    }
}
