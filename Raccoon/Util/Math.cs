using System.Collections.Generic;

namespace Raccoon.Util {
    public static class Math {
        public const float PI = Microsoft.Xna.Framework.MathHelper.Pi;
        public const float HalfPI = PI / 2;
        public const float ThirdPI = PI / 3;
        public const float FourthPI = PI / 4;
        public static readonly float RadToDeg = 180.0f / PI;
        public static readonly float DegToRad = PI / 180.0f;

        public static float ToRadians(float deg) {
            return deg * DegToRad;
        }

        public static float ToDegrees(float rad) {
            return rad * RadToDeg;
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

        public static float Angle(float x1, float y1, float x2, float y2) {
            return (float) System.Math.Atan2(y1 - y2, x2 - x1) * RadToDeg;
        }

        public static float Angle(Vector2 from, Vector2 to) {
            return Angle(from.X, from.Y, to.X, to.Y);
        }

        public static float Angle(float x, float y) {
            return Angle(0, 0, x, y);
        }

        public static float Angle(Vector2 to) {
            return Angle(to.X, to.Y);
        }

        public static float WrapAngle(float angle) {
            return Microsoft.Xna.Framework.MathHelper.WrapAngle(angle);
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

        public static float Lerp(float start, float end, float t) {
            return Microsoft.Xna.Framework.MathHelper.Lerp(start, end, t);
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
        public static Vector2 Abs(Vector2 vec) {
            return new Vector2(System.Math.Abs(vec.X), System.Math.Abs(vec.Y));
        }

        public static float Min(float n1, float n2) {
            return Microsoft.Xna.Framework.MathHelper.Min(n1, n2);
        }

        public static int Min(int n1, int n2) {
            return Microsoft.Xna.Framework.MathHelper.Min(n1, n2);
        }

        public static float Max(float n1, float n2) {
            return Microsoft.Xna.Framework.MathHelper.Max(n1, n2);
        }

        public static int Max(int n1, int n2) {
            return Microsoft.Xna.Framework.MathHelper.Max(n1, n2);
        }

        public static float Distance(float from, float to) {
            return System.Math.Abs(to - from);
        }

        public static float Distance(Vector2 from, Vector2 to) {
            return (float) System.Math.Sqrt(DistanceSquared(from, to));
        }

        public static float Distance(Vector2 fromLineA, Vector2 fromLineB, Vector2 toPoint) {
            return (float) System.Math.Sqrt(DistanceSquared(fromLineA, fromLineB, toPoint));
        }

        public static float DistanceSquared(Vector2 from, Vector2 to) {
            return (to - from).LengthSquared();
        }

        public static float DistanceSquared(Vector2 fromLineStart, Vector2 fromLineEnd, Vector2 toPoint) {
            // implemented using http://stackoverflow.com/a/1501725
            float lengthSquared = DistanceSquared(fromLineStart, fromLineEnd);
            if (lengthSquared == 0) {
                return DistanceSquared(fromLineStart, toPoint);
            }

            float t = Clamp(Vector2.Dot(toPoint - fromLineStart, fromLineEnd - fromLineStart) / lengthSquared, 0, 1);
            Vector2 proj = fromLineStart + t * (fromLineEnd - fromLineStart);
            return DistanceSquared(toPoint, proj);
        }

        public static Vector2 RotateAround(Vector2 point, float degrees) {
            float cos = Cos(degrees), sin = Sin(degrees);
            return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
        }

        public static Vector2 RotateAround(Vector2 point, Vector2 origin, float degrees) {
            float cos = Cos(degrees), sin = Sin(degrees);
            point -= origin;
            return origin + new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
        }

        public static float[] Projection(Vector2 axis, IEnumerable<Vector2> points) {
            IEnumerator<Vector2> enumerator = points.GetEnumerator();
            enumerator.MoveNext();
            float min = axis.Dot(enumerator.Current);
            float max = min;
            foreach (Vector2 point in points) {
                float p = axis.Dot(point);
                if (p < min) {
                    min = p;
                } else if (p > max) {
                    max = p;
                }
            }

            return new float[] { min, max };
        }

        public static float[] Projection(Vector2 axis, params Vector2[] points) {
            return Projection(axis, points as IEnumerable<Vector2>);
        }
    }
}
