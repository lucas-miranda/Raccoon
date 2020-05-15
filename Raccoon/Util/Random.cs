using System.Collections.Generic;

using Raccoon.Graphics;

namespace Raccoon.Util {
    /// <summary>
    /// Provides a set of Random generation utility methods.
    /// </summary>
    public static class Random {
        private static int _seedValue;

        private static System.Random _rand = new System.Random();

        static Random() {
            Seed = (int) System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// A number used to calculate values in the pseudo-random sequence.
        /// </summary>
        public static int Seed { get { return _seedValue; } set { _seedValue = value; _rand = new System.Random(_seedValue); } }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <returns>True or False.</returns>
        public static bool Boolean() {
            return Integer(1, 100) <= 50;
        }

        /// <summary>
        /// Fills a array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to receive random numbers.</param>
        public static void Bytes(byte[] buffer) {
            _rand.NextBytes(buffer);
        }

        /// <summary>
        /// Returns a random non-negative integer number.
        /// </summary>
        /// <returns>Number in range [0, int.MaxValue].</returns>
        public static int Integer() {
            return _rand.Next();
        }

        /// <summary>
        /// Returns a random integer in range [min, max].
        /// </summary>
        /// <param name="min">The inclusive lower bound value.</param>
        /// <param name="max">The inclusive upper bound value, must be greater than min value.</param>
        /// <returns>Number in range [min, max].</returns>
        public static int Integer(int min, int max) {
            return _rand.Next(min, max + 1);
        }

        /// <summary>
        /// Returns a random single precision floating-point number in range [0.0, 1.0[.
        /// </summary>
        /// <returns>Number in range [0.0, 1.0[.</returns>
        public static float Single() {
            return (float) Double();
        }

        /// <summary>
        /// Returns a random single precision floating-point number in range [min, max[.
        /// </summary>
        /// <param name="min">The inclusive lower bound value.</param>
        /// <param name="max">The inclusive upper bound value, must be greater than min value.</param>
        /// <returns>Number in range [min, max[.</returns>
        public static float Single(float min, float max) {
            return (float) Double(min, max);
        }

        /// <summary>
        /// Returns a random double precision floating-point number in range [0.0, 1.0[.
        /// </summary>
        /// <returns>Number in range [0.0, 1.0[.</returns>
        public static double Double() {
            return _rand.NextDouble();
        }

        /// <summary>
        /// Returns a random double precision floating-point number in range [min, max[.
        /// </summary>
        /// <param name="min">The inclusive lower bound value.</param>
        /// <param name="max">The inclusive upper bound value, must be greater than min value.</param>
        /// <returns>Number in range [0.0, 1.0[.</returns>
        public static double Double(double min, double max) {
            return min + Double() * (max - min);
        }

        /// <summary>
        /// Returns a random Color.
        /// </summary>
        /// <returns>A random Color value.</returns>
        public static Color Color() {
            return new Color((byte) Integer(0, 255), (byte) Integer(0, 255), (byte) Integer(0, 255));
        }

        /// <summary>
        /// Returns a random normalized Vector2.
        /// </summary>
        /// <returns>A random Vector2 in range.</returns>
        public static Vector2 Vector2() {
            return Math.PolarToCartesian(1f, Integer(0, 359));
        }

        /// <summary>
        /// Returns a random Vector2 in a determined area.
        /// </summary>
        /// <param name="area">A Rectangle area.</param>
        /// <returns>A random Vector2 in range (x: [left, right], y: [top, bottom]).</returns>
        public static Vector2 Vector2(Rectangle area) {
            return new Vector2(Integer((int) area.Left, (int) area.Right), Integer((int) area.Top, (int) area.Bottom));
        }

        /// <summary>
        /// Returns a random Vector2 in a determined area.
        /// </summary>
        /// <param name="area">A area's Size.</param>
        /// <returns>A random Vector2 in range (x: [0, 0], y: [width, height]).</returns>
        public static Vector2 Vector2(Size size) {
            return Vector2(new Rectangle(size));
        }

        public static Vector2 Vector2(Vector2 min, Vector2 max) {
            return new Vector2(Single(min.X, max.X), Single(min.Y, max.Y));
        }

        /// <summary>
        /// Returns a random single Direction.
        /// </summary>
        /// <returns>A random single Direction.</returns>
        public static Direction Direction() {
            return (Direction) (1 << Integer(0, 3));
        }

        /// <summary>
        /// Tests a random integer percent value.
        /// </summary>
        /// <param name="chance">Percent value in range [1, 100].</param>
        /// <returns>True if random value is less than or equals chance, False otherwise.</returns>
        public static bool PercentInteger(int chance) {
            return Integer(1, 100) <= chance;
        }

        /// <summary>
        /// Tests a random single precision percent value.
        /// </summary>
        /// <param name="chance">Percent value in range [0f, 100f[.</param>
        /// <returns>True if random value is less than chance, False otherwise.</returns>
        public static bool PercentSingle(float chance) {
            return Single(0f, 100f) <= chance;
        }

        /// <summary>
        /// Choose a random point inside a circle.
        /// </summary>
        /// <param name="radius">Circle radius.</param>
        /// <param name="minRandomRadius">Random radius will be in range [minRandomRadius, radius].</param>
        /// <returns>A random point in circle.</returns>
        public static Vector2 PositionInCircle(float radius, float minRandomRadius = 0f) {
            return Math.PolarToCartesian(Single(minRandomRadius, radius), Integer(0, 359));
        }

        /// <summary>
        /// Choose a random point inside a circle arc
        /// </summary>
        /// <param name="circleRadius">Circle radius.</param>
        /// <param name="arcStartAngle">Angle (in degrees) when arc starts.</param>
        /// <param name="arcAngleLength">Lenght (in degrees) of arc.</param>
        /// <param name="minRandomRadius">Random radius will be in range [minRandomRadius, radius].</param>
        /// <returns>A random point in circle arc.</returns>
        public static Vector2 PositionInArc(float circleRadius, float arcStartAngle, float arcAngleLength, float minRandomRadius = 0f) {
            return Math.PolarToCartesian(Single(minRandomRadius, circleRadius), Single(arcStartAngle, arcStartAngle + arcAngleLength));
        }

        /// <summary>
        /// Choose a random value contained in a list.
        /// </summary>
        /// <typeparam name="T">Any class.</typeparam>
        /// <param name="list">A list containing values.</param>
        /// <returns>A random value in list or default T value, if list is empty.</returns>
        public static T Choose<T>(IList<T> list) {
            return list[Integer(0, list.Count - 1)];
        }

        /// <summary>
        /// Choose a random value contained in a collection.
        /// </summary>
        /// <typeparam name="T">Any class.</typeparam>
        /// <param name="collection">A collection containing values.</param>
        /// <returns>A random value in collection or default T value, if collection is empty.</returns>
        public static T Choose<T>(ICollection<T> collection) {
            IEnumerator<T> enumerator = collection.GetEnumerator();
            int i = Integer(0, collection.Count - 1);

            while (enumerator.MoveNext()) {
                if (i == 0) {
                    return enumerator.Current;
                }

                i--;
            }

            return default;
        }

        /// <summary>
        /// Choose a random value, using chances values as input array and returning a id of the choosed one.
        /// </summary>
        /// <param name="chanceValues">Integer chances.</param>
        /// <returns>Id of the chance value choosed.</returns>
        public static int ChooseWeighted(IList<int> chanceValues) {
            if (chanceValues.Count == 0) {
                throw new System.ArgumentException("Can't choose without a defined chance value.");
            }

            int total = 0;

            foreach (int value in chanceValues) {
                total += value;
            }

            if (total <= 0) {
                throw new System.ArgumentException("Chances values total sum must be greater than zero.");
            }

            int targetChance = Integer(1, total);
            for (int i = 0; i < chanceValues.Count; i++) {
                int chance = chanceValues[i];
                if (chance == 0) {
                    continue;
                }

                targetChance -= chance;
                if (targetChance <= 0) {
                    return i;
                }
            }

            return -1;
        }

        public static T Retrieve<T>(IList<T> list) {
            if (list.Count <= 0) {
                throw new System.ArgumentException($"Can't retrieve an element from a empty IList<{typeof(T)}>");
            }

            int index = Integer(0, list.Count - 1);
            T value = list[index];
            list.RemoveAt(index);
            return value;
        }
    }
}
