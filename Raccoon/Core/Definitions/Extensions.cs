using System;
using System.Collections.Generic;

namespace Raccoon {
    public static class Extensions {
        #region Enums

        public static List<Enum> GetFlagValues(this Enum enumFlags) {
            List<Enum> separatedFlagValues = new List<Enum>();
            int enumFlagsAsNumber = Convert.ToInt32(enumFlags), enumSize = System.Runtime.InteropServices.Marshal.SizeOf(Enum.GetUnderlyingType(enumFlags.GetType()));
            for (int i = 0; i < 8 * enumSize; i++) {
                int bitValue = 1 << i;
                if ((enumFlagsAsNumber & bitValue) == 0) {
                    continue;
                }

                separatedFlagValues.Add((Enum) Enum.ToObject(enumFlags.GetType(), bitValue));
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
    }
}
