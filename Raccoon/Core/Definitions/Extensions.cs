using System;
using System.Collections.Generic;

namespace Raccoon {
    public static class Extensions {
        #region Enums

        public static int GetUnderlyingNumericSize(this Enum e) {
            return System.Runtime.InteropServices.Marshal.SizeOf(Enum.GetUnderlyingType(e.GetType()));
        }

        public static List<Enum> GetFlagValues(this Enum enumFlags) {
            List<Enum> separatedFlagValues = new List<Enum>();
            long enumFlagsAsNumber = Convert.ToInt64(enumFlags);
            int bits = 8 * enumFlags.GetUnderlyingNumericSize();
            for (int i = 0; i < bits; i++) {
                long bitValue = 1L << i;
                if ((enumFlagsAsNumber & bitValue) == 0L) {
                    continue;
                }

                separatedFlagValues.Add((Enum) Enum.ToObject(enumFlags.GetType(), bitValue));
            }

            if (separatedFlagValues.Count == 0) {
                separatedFlagValues.Add((Enum) Enum.ToObject(enumFlags.GetType(), 0L));
            }

            return separatedFlagValues;
        }

        #endregion Enums

        #region String

        public static int Count(this string str, string value) {
            int count = 0,
                i = str.IndexOf(value);
            
            while (i != -1) {
                count++;
                i = str.IndexOf(value, i + value.Length);
            }

            return count;
        }

        #endregion String

        #region Linked List

        public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> current) {
            return current.Next ?? current.List.First;
        }

        public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> current) {
            return current.Previous ?? current.List.Last;
        }

        #endregion Linked List

        #region IList

        public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
            T aux = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = aux;
        }

        #endregion IList
    }
}
