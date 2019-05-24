using System.Collections.Generic;
using System.Reflection;

using Raccoon.Util;

public static class Extensions {
    #region Enums

    public static int GetUnderlyingNumericSize(this System.Enum e) {
        return System.Runtime.InteropServices.Marshal.SizeOf(System.Enum.GetUnderlyingType(e.GetType()));
    }

    public static List<System.Enum> GetFlagValues(this System.Enum enumFlags) {
        List<System.Enum> flagValues = new List<System.Enum>();
        long flagsNumber = System.Convert.ToInt64(enumFlags);

        if (Math.IsPowerOfTwo(flagsNumber)) {
            flagValues.Add((System.Enum) System.Enum.ToObject(enumFlags.GetType(), flagsNumber));
            return flagValues;
        }

        int bits = (int) System.Math.Ceiling(System.Math.Log(flagsNumber, 2));
        for (int i = 0; i <= bits; i++) {
            long bitValue = 1L << i;
            if ((flagsNumber & bitValue) == 0L) {
                continue;
            }

            flagValues.Add((System.Enum) System.Enum.ToObject(enumFlags.GetType(), bitValue));
        }

        return flagValues;
    }

    public static T GetAttribute<T>(this System.Enum e) where T : System.Attribute {
        System.Type type = e.GetType();
        FieldInfo fieldInfo = type.GetField(System.Enum.GetName(type, e));
        return fieldInfo.GetCustomAttribute<T>();
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

    public static IEnumerable<T> IterateReverse<T>(this IList<T> list) {
        for (int i = list.Count - 1; i >= 0; i--) {
            yield return list[i];
        }
    }

    #endregion IList
}
