using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Raccoon.Graphics;

namespace Raccoon.Util {
    /// <summary>
    /// Provides a set of Random generation utility methods.
    /// </summary>
    public static class Random {
        #region Private Members

        private static System.Type _baseRandomType = typeof(System.Random);
        private static int _seed;

        #endregion Private Members

        #region Constructors

        static Random() {
            _seed = (int) System.DateTime.Now.Ticks;
            BaseRandom = new System.Random(_seed);
        }

        #endregion Constructors

        #region Public Properties

        public static System.Random BaseRandom { get; private set; }

        public static System.Type BaseRandomType {
            get {
                return _baseRandomType;
            }

            set {
                if (value == _baseRandomType) {
                    return;
                }

                if (value == null) {
                    throw new System.ArgumentNullException(nameof(value));
                }

                if (!typeof(System.Random).IsAssignableFrom(value)) {
                    throw new System.ArgumentException($"Type '{value}' don't derive from {nameof(System.Random)}.");
                }

                _baseRandomType = value;
                BaseRandom = (System.Random) System.Activator.CreateInstance(_baseRandomType, _seed);
            }
        }

        /// <summary>
        /// A number used to calculate values in the pseudo-random sequence.
        /// </summary>
        public static int Seed {
            get {
                return _seed;
            }

            set {
                _seed = value;
                BaseRandom = new System.Random(_seed);
            }
        }

        #endregion Public Properties

        #region Public Methods

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
            BaseRandom.NextBytes(buffer);
        }

        /// <summary>
        /// Returns a random number sign, which can be plus (+1) or minus (-1).
        /// </summary>
        /// <returns>1 or -1.</returns>
        public static int Sign() {
            return Boolean() ? 1 : -1;
        }

        /// <summary>
        /// Returns a random non-negative integer number.
        /// </summary>
        /// <returns>Number in range [0, int.MaxValue].</returns>
        public static int Integer() {
            return BaseRandom.Next();
        }

        /// <summary>
        /// Returns a random integer in range [min, max].
        /// </summary>
        /// <param name="min">The inclusive lower bound value.</param>
        /// <param name="max">The inclusive upper bound value, must be greater than min value.</param>
        /// <returns>Number in range [min, max].</returns>
        public static int Integer(int min, int max) {
            if (max < min) {
                throw new System.ArgumentException("Max should be greater or equals min.");
            } else if (min == max) {
                return max;
            }

            return BaseRandom.Next(min, max + 1);
        }

        /// <summary>
        /// Returns a random integer in range.
        /// </summary>
        /// <param name="min">Values range.</param>
        /// <returns>Number in range.</returns>
        public static int Integer(Range range) {
            return BaseRandom.Next((int) range.Min, ((int) range.Max) + 1);
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
            if (max < min) {
                throw new System.ArgumentException("Max should be greater or equals min.");
            }

            return (float) Double(min, max);
        }

        /// <summary>
        /// Returns a random single precision floating-point number in range.
        /// </summary>
        /// <param name="min">Values range.</param>
        /// <returns>Number in range.</returns>
        public static float Single(Range range) {
            return (float) Double(range.Min, range.Max);
        }

        /// <summary>
        /// Returns a random double precision floating-point number in range [0.0, 1.0[.
        /// </summary>
        /// <returns>Number in range [0.0, 1.0[.</returns>
        public static double Double() {
            return BaseRandom.NextDouble();
        }

        /// <summary>
        /// Returns a random double precision floating-point number in range [min, max[.
        /// </summary>
        /// <param name="min">The inclusive lower bound value.</param>
        /// <param name="max">The inclusive upper bound value, must be greater than min value.</param>
        /// <returns>Number in range [0.0, 1.0[.</returns>
        public static double Double(double min, double max) {
            if (max < min) {
                throw new System.ArgumentException("Max should be greater or equals min.");
            }

            return min + Double() * (max - min);
        }

        /// <summary>
        /// Returns a random double precision floating-point number in range.
        /// </summary>
        /// <param name="min">Values range.</param>
        /// <returns>Number in range.</returns>
        public static double Double(Range range) {
            return (double) range.Min + Double() * (double) (range.Max - range.Min);
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
            if (max.X < min.X) {
                throw new System.ArgumentException("Max.X should be greater or equals min.X.");
            }

            if (max.Y < min.Y) {
                throw new System.ArgumentException("Max.Y should be greater or equals min.Y.");
            }

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
            if (chance <= 0) {
                return false;
            } else if (chance >= 100) {
                return true;
            }

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
            if (list.Count == 0) {
                throw new System.ArgumentException($"Can't choose an element from an empty IList<{typeof(T)}>");
            } else if (list.Count == 1) {
                return list[0];
            }

            return list[Integer(0, list.Count - 1)];
        }

        /// <summary>
        /// Choose a random value contained in a collection.
        /// </summary>
        /// <typeparam name="T">Any class.</typeparam>
        /// <param name="collection">A collection containing values.</param>
        /// <returns>A random value in collection or default T value, if collection is empty.</returns>
        public static T Choose<T>(ICollection<T> collection) {
            if (collection.Count == 0) {
                throw new System.ArgumentException($"Can't choose an element from a empty ICollection<{typeof(T)}>");
            }

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

        public static KeyValuePair<K, V> Choose<K, V>(IDictionary<K, V> dictionary) {
            if (dictionary.Count <= 0) {
                throw new System.ArgumentException($"Can't retrieve an element from a empty IDictionary<{typeof(K)}, {typeof(V)}>");
            }

            int index = Integer(0, dictionary.Count - 1);
            K key = default;
            V value = default;

            foreach (KeyValuePair<K, V> entry in dictionary) {
                index -= 1;

                if (index < 0) {
                    key = entry.Key;
                    value = entry.Value;
                    break;
                }
            }

            return new KeyValuePair<K, V>(key, value);
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

        public static T ChooseWeighted<T>(IList<T> items, IList<int> chanceValues) {
            if (items.Count != chanceValues.Count) {
                throw new System.ArgumentException($"{nameof(items)} count ({items.Count}) and {nameof(chanceValues)} count ({chanceValues.Count}) should be equals.");
            }

            int index = ChooseWeighted(chanceValues);
            if (index < 0) {
                throw new System.InvalidOperationException($"Something unexpected happened. Choosed index is invalid.");
            }

            return items[index];
        }

        public static int ChooseWeighted<T>(IList<T> items) where T : ITuple {
            if (items.Count == 0) {
                throw new System.ArgumentException("Items list is empty.");
            }

            if (items[0].Length < 2) {
                throw new System.ArgumentException("At least two components are required. First component should always be the chance amount.");
            }

            if (items[0][0].GetType() != typeof(int)) {
                throw new System.ArgumentException($"First component should be an integer chance amount. But '{items[0][0].GetType()}' was provided.");
            }

            int total = 0;

            foreach (ITuple value in items) {
                total += (int) value[0];
            }

            if (total <= 0) {
                throw new System.ArgumentException("Chances values total sum must be greater than zero.");
            }

            int targetChance = Integer(1, total);
            for (int i = 0; i < items.Count; i++) {
                int chance = (int) items[i][0];
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

        public static T Retrieve<T>(ICollection<T> collection) {
            if (collection.Count <= 0) {
                throw new System.ArgumentException($"Can't retrieve an element from a empty ICollection<{typeof(T)}>");
            }

            int index = Integer(0, collection.Count - 1);
            T value = default(T);

            foreach (T item in collection) {
                index -= 1;

                if (index >= 0) {
                    continue;
                }

                value = item;
            }

            collection.Remove(value);
            return value;
        }

        public static KeyValuePair<K, V> Retrieve<K, V>(IDictionary<K, V> dictionary) {
            KeyValuePair<K, V> entry = Choose<K, V>(dictionary);
            dictionary.Remove(entry.Key);
            return entry;
        }

        #endregion Public Methods
    }
}
