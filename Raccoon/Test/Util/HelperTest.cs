using NUnit.Framework;
using Raccoon.Util;

namespace Raccoon.Tests.Util {
    class HelperTest {
        [TestCase()]
        public void EqualsPermutation_ListWithList() {
            Vector2[] verticesA = {
                new Vector2(2, 1),
                new Vector2(5, 4),
                new Vector2(-10, 6),
                new Vector2(40, 24),
                new Vector2(-12, -30)
            };

            Vector2[] verticesA_swapedPlaces = {
                new Vector2(2, 1),
                new Vector2(40, 24),
                new Vector2(5, 4),
                new Vector2(-12, -30),
                new Vector2(-10, 6)
            };

            Vector2[] verticesB = {
                new Vector2(9, 10),
                new Vector2(32, -30),
                new Vector2(3, 12),
                new Vector2(4, 9),
                new Vector2(1, -5)
            };

            Assert.IsTrue(Helper.EqualsPermutation(verticesA, verticesA));
            Assert.IsTrue(Helper.EqualsPermutation(verticesA, verticesA_swapedPlaces));
            Assert.IsFalse(Helper.EqualsPermutation(verticesA, verticesB));
        }

        [TestCase()]
        public void EqualsPermutation_2x2() {
            int[] numbersA = { 40, 48 };
            int[] numbersB = { -32, -18 };

            Assert.IsTrue(Helper.EqualsPermutation(numbersA[0], numbersA[1], numbersA[0], numbersA[1]));
            Assert.IsTrue(Helper.EqualsPermutation(numbersA[0], numbersA[1], numbersA[1], numbersA[0]));
            Assert.IsFalse(Helper.EqualsPermutation(numbersA[0], numbersA[1], numbersB[0], numbersB[1]));
        }

        /*
        [TestCase()]
        public void EqualsPermutation_3x3() {
            int[] numbersA = { -34, 3, 39 };
            int[] numbersB = { -35, -28, 40 };

            Assert.IsTrue(Helper.EqualsPermutation(numbersA[0], numbersA[1], numbersA[2], numbersA[0], numbersA[1], numbersA[2]));
            Assert.IsTrue(Helper.EqualsPermutation(numbersA[0], numbersA[1], numbersA[2], numbersA[1], numbersA[2], numbersA[0]));
            Assert.IsTrue(Helper.EqualsPermutation(numbersA[0], numbersA[1], numbersA[2], numbersB[0], numbersB[1], numbersB[2]));
        }*/
    }
}
