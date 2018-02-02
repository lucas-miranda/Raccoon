using System.Collections.Generic;
using NUnit.Framework;
using Raccoon;

namespace Raccoon.Tests.Extensions {
    class EnumTest {
        [System.Flags]
        public enum TestFlag {
            None = 0,
            A = 1 << 0,
            B = 1 << 1,
            C = 1 << 2,
            D = 1 << 3,
        }

        private static object[] _testData_GetFlagValues = {
            new object[] {
                TestFlag.None,
                new System.Enum[] { TestFlag.None }
            },

            new object[] {
                TestFlag.C,
                new System.Enum[] { TestFlag.C }
            },

            new object[] {
                TestFlag.A | TestFlag.C,
                new System.Enum[] { TestFlag.A, TestFlag.C }
            },

            new object[] {
                TestFlag.A | TestFlag.C | TestFlag.D,
                new System.Enum[] { TestFlag.A, TestFlag.C, TestFlag.D }
            },

            new object[] {
                TestFlag.A | TestFlag.C | TestFlag.B | TestFlag.D,
                new System.Enum[] { TestFlag.A, TestFlag.B, TestFlag.C, TestFlag.D }
            }
        };

        [Test, TestCaseSource("_testData_GetFlagValues")]
        public void GetFlagValues(TestFlag flags, System.Enum[] expectedValues) {
            List<System.Enum> flagValues = flags.GetFlagValues();
            for (int i = 0; i < expectedValues.Length; i++) {
                Assert.IsTrue(flagValues[i].Equals(expectedValues[i]), $"Expecting value '{expectedValues[i]}', not found on list: {string.Join(", ", flagValues)}");
            }
        }
    }
}
