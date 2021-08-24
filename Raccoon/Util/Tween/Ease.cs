namespace Raccoon.Util.Tween {
    public static class Ease {
        private const double T1 = 1 / 2.75,
                             B1 = 1.5 / 2.75,
                             T2 = 2 / 2.75,
                             B2 = 2.25 / 2.75,
                             T3 = 2.5 / 2.75,
                             B3 = 2.625 / 2.75;

        public static float Linear(float t) {
            return t;
        }

        public static float QuadIn(float t) {
            return t * t;
        }

        public static float QuadOut(float t) {
            return t * (2 - t);
        }

        public static float QuadInOut(float t) {
            return t < .5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }

        public static float CubicIn(float t) {
            return t * t * t;
        }

        public static float CubicOut(float t) {
            return (--t) * t * t + 1;
        }

        public static float CubicInOut(float t) {
            return t < .5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }

        public static float QuartIn(float t) {
            return t * t * t * t;
        }

        public static float QuartOut(float t) {
            return 1 - (--t) * t * t * t;
        }

        public static float QuartInOut(float t) {
            return t < .5 ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
        }

        public static float QuintIn(float t) {
            return t * t * t * t * t;
        }

        public static float QuintOut(float t) {
            return 1 + (--t) * t * t * t * t;
        }

        public static float QuintInOut(float t) {
            return t < .5 ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
        }

        public static float SineIn(float t) {
            return (float) (1 - System.Math.Cos(t * Math.HalfPI));
        }

        public static float SineOut(float t) {
            return (float) System.Math.Sin(t * Math.HalfPI);
        }

        public static float SineInOut(float t) {
            return (float) (t == 1 ? 1 : -.5 * System.Math.Cos(t * Math.PI) + .5);
        }

        public static float ExpoIn(float t) {
            return (float) System.Math.Pow(2, 10 * (t - 1));
        }

        public static float ExpoOut(float t) {
            return (float) (t == 1 ? 1 : 1 - System.Math.Pow(2, -10 * t));
        }

        public static float ExpoInOut(float t) {
            if (t == 1) return 1;
            return (float) (t < .5 ? System.Math.Pow(2, 10 * (t * 2 - 1)) / 2 : (2 - System.Math.Pow(2, -10 * (t * 2 - 1))) / 2);
        }

        public static float CircIn(float t) {
            return (float) (1 - System.Math.Sqrt(1 - t * t));
        }

        public static float CircOut(float t) {
            return (float) System.Math.Sqrt(1 - (t - 1) * (t - 1));
        }

        public static float CircInOut(float t) {
            return (float) (t < .5 ? (1 - System.Math.Sqrt(1 - t * t * 4)) / 2 : (System.Math.Sqrt(1 - (t * 2 - 2) * (t * 2 - 2)) + 1) / 2);
        }

        public static float BackIn(float t) {
            return (float) (t * t * (2.70158 * t - 1.70158));
        }

        public static float BackOut(float t) {
            return (float) ((t - 1) * (t - 1) * (2.70158 * (t - 1) + 1.70158) + 1);
        }

        public static float BackInOut(float t) {
            return (float) (t < .5 ? (4 * t * t * ((2.5949095 + 1) * 2 * t - 2.5949095)) / 2 : ((t * 2 - 2) * (t * 2 - 2) * ((2.5949095 + 1) * (t * 2 - 2) + 2.5949095) + 2) / 2);
        }

        public static float ElasticIn(float t) {
            return (float) -(System.Math.Pow(2, 10 * (t - 1)) * System.Math.Sin(((t - 1) - .075) * (2 * Math.PI) / .3));
        }

        public static float ElasticOut(float t) {
            return (float) (System.Math.Pow(2, -10 * t) * System.Math.Sin((t - .075) * (2 * Math.PI) / .3) + 1);
        }

        public static float ElasticInOut(float t) {
            return (float) (t < .5 ? -.5 * System.Math.Pow(2, 10 * (t * 2 - 1)) * System.Math.Sin(((t * 2 - 1) - .1125) * (2 * Math.PI) / .45) : System.Math.Pow(2, -10 * (t * 2 - 1)) * System.Math.Sin(((t * 2 - 1) - .1125) * (2 * Math.PI) / .45) * .5 + 1);
        }

        public static float BounceIn(float t) {
            return 1 - BounceOut(1 - t);
        }

        public static float BounceOut(float t) {
            if (t < T1) {
                return (float) (7.5625 * t * t);
            } else if (t < T2) {
                return (float) (7.5625 * (t - B1) * (t - B1) + .75);
            } else if (t < T3) {
                return (float) (7.5625 * (t - B2) * (t - B2) + .9375);
            } else {
                return (float) (7.5625 * (t - B3) * (t - B3) + .984375);
            }
        }

        public static float BounceInOut(float t) {
            return (float) (t < .5 ? BounceIn(t * 2) * .5 : BounceOut(t * 2 - 1) * .5 + .5);
        }
    }
}
