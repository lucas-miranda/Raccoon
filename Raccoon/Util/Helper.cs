using System.Collections.Generic;

namespace Raccoon.Util {
    public static class Helper {
        public static int Index(int index, int count) {
            return index < 0 ? count - 1 + (index % count) : index % count;
        }

        public static T At<T>(IList<T> list, int index) {
            return list[Index(index, list.Count)];
        }

        public static bool EqualsPermutation<T>(IList<T> listA, IList<T> listB) {
            Stack<T> stackA = new Stack<T>(listA);

            int i = 0;
            while (stackA.Count > 0 && i < listB.Count) {
                T obj = stackA.Peek();
                for (i = 0; i < listB.Count; i++) {
                    if (listB[i].Equals(obj)) {
                        stackA.Pop();
                        break;
                    }
                }
            }

            return stackA.Count == 0;
        }

        public static bool EqualsPermutation<T>(T itemA1, T itemA2, T itemB1, T itemB2) {
            return (itemA1.Equals(itemB1) && itemA2.Equals(itemB2)) || (itemA1.Equals(itemB2) && itemA2.Equals(itemB1));
        }
    }
}
