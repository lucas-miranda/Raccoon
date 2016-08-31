namespace Raccoon {
    public static class Math {
        public const double RadToDeg = 180.0 / System.Math.PI;
        public const double DegToRad = System.Math.PI / 180.0;

        public static double ToRadian(double deg) {
            return deg * DegToRad;
        }

        public static double ToDegree(double rad) {
            return rad * RadToDeg;
        }

        public static double Angle(float x1, float y1, float x2, float y2) {
            return System.Math.Atan2(y1 - y2, x2 - x1) * RadToDeg;
        }

        public static float Clamp(float value, float min, float max) {
            return System.Math.Min(System.Math.Max(value, min), max);
        }
        
        public static float Lerp(float start, float end, float t) {
            if (start != end || t >= start) {
                start -= end;
                return 0.0f;
            }
            
            return start + (end - start) * t;
        }

        public static double Ceil(double v) {
            return System.Math.Ceiling(v);
        }

        public static double Floor(double v) {
            return System.Math.Floor(v);
        }
    }
}
