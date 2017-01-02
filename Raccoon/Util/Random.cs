using System.Collections.Generic;

using Raccoon.Graphics;

namespace Raccoon.Util {
    public class Random {
        private static Random _instance = new Random();

        private System.Random _rand;

        private Random() {
            _rand = new System.Random();
        }

        private Random(int seed) {
            _rand = new System.Random(seed);
        }

        public static void Bytes(byte[] buffer) {
            _instance._rand.NextBytes(buffer);
        }

        public static int Integer() {
            return _instance._rand.Next();
        }

        public static int Integer(int min, int max) {
            return _instance._rand.Next(min, max + 1);
        }

        public static float Single() {
            return (float) Double();
        }

        public static float Single(float min, float max) {
            return (float) Double(min, max);
        }

        public static double Double() {
            return _instance._rand.NextDouble();
        }

        public static double Double(double min, double max) {
            return min + Double() * (max - min);
        }

        public static Color Color() {
            return new Color(Integer(0, 255), Integer(0, 255), Integer(0, 255));
        }

        public static Vector2 Vector2() {
            return new Vector2((float) (Double() * 2 - 1), (float) (Double() * 2 - 1));
        }

        public static Vector2 Vector2(Rectangle area) {
            return new Vector2(Integer((int) area.Left, (int) area.Right), Integer((int) area.Top, (int) area.Bottom));
        }

        public static Direction Direction() {
            return (Direction) (1 << Integer(0, 3));
        }

        public static T Choose<T>(ICollection<T> list) where T : class {
            int i = Integer(0, Math.Max(0, list.Count - 1));
            foreach (T item in list) {
                if (i == 0) {
                    return item;
                }

                i--;
            }

            return default(T);
        }

        public static void SetSeed(int seed) {
            _instance._rand = new System.Random(seed);
        }
    }
}
