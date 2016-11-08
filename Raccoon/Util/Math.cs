namespace Raccoon {
    public static class Math {
        public const float PI = Microsoft.Xna.Framework.MathHelper.Pi;
        public static readonly float RadToDeg = 180.0f / PI;
        public static readonly float DegToRad = PI / 180.0f;

        public static float ToRadians(float deg) {
            return deg * DegToRad;
        }

        public static float ToDegrees(float rad) {
            return rad * RadToDeg;
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

        public static float Abs(float n) {
            return System.Math.Abs(n);
        }

        public static int Abs(int n) {
            return System.Math.Abs(n);
        }

        public static float Ceil(float n) {
            return (float) System.Math.Ceiling(n);
        }

        public static float Floor(float n) {
            return (float) System.Math.Floor(n);
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
    }
}
