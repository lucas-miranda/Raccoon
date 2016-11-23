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

        public static float WrapAngle(float angle) {
            return Microsoft.Xna.Framework.MathHelper.WrapAngle(angle);
        }

        public static float Clamp(float value, float min, float max) {
            return Microsoft.Xna.Framework.MathHelper.Clamp(value, min, max);
        }

        public static int Clamp(int value, int min, int max) {
            return Microsoft.Xna.Framework.MathHelper.Clamp(value, min, max);
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
    }
}
