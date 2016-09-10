namespace Raccoon {
    public static class Math {
        public static readonly float PI = (float) System.Math.PI;
        public static readonly float RadToDeg = (float) (180.0 / System.Math.PI);
        public static readonly float DegToRad = (float) (System.Math.PI / 180.0);

        public static float ToRadians(float deg) {
            return deg * DegToRad;
        }

        public static float ToDegrees(float rad) {
            return rad * RadToDeg;
        }

        public static float Angle(float x1, float y1, float x2, float y2) {
            return (float) System.Math.Atan2(y1 - y2, x2 - x1) * RadToDeg;
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

        public static float Abs(float v) {
            return System.Math.Abs(v);
        }

        public static float Ceil(float v) {
            return (float) System.Math.Ceiling(v);
        }

        public static float Floor(float v) {
            return (float) System.Math.Floor(v);
        }

    }
}
