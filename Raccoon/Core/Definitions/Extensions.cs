using System.Text;
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
        if (value == null) {
            throw new System.ArgumentNullException(nameof(value));
        }

        int count = 0,
            i = str.IndexOf(value);

        while (i != -1) {
            count++;
            i = str.IndexOf(value, i + value.Length);
        }

        return count;
    }

    public static string Capitalize(this string str, bool capitalizeSingleLetters = false) {
        string[] splitted = str.Split(new char[] { ' ' });
        StringBuilder resultBuilder = new StringBuilder();

        for (int i = 0; i < splitted.Length; i++) {
            string part = splitted[i];

            if (part.Length == 0) {
                continue;
            } else if (part.Length == 1) {
                if (capitalizeSingleLetters) {
                    resultBuilder.Append(char.ToUpperInvariant(part[0]));
                } else {
                    resultBuilder.Append(part[0]);
                }

                continue;
            }

            resultBuilder.Append(char.ToUpperInvariant(part[0]));
            resultBuilder.Append(part.Substring(startIndex: 1));

            if (i < splitted.Length - 1) {
                resultBuilder.Append(' ');
            }
        }

        return resultBuilder.ToString();
    }

    public static string SeparateCapitalized(this string str, char separationChar = ' ') {
        string separation = separationChar.ToString();

        for (int i = 1; i < str.Length; i++) {
            if (char.IsUpper(str[i]) && str[i - 1] != separationChar) {
                str.Insert(i, separation);
            }
        }

        return str;
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

    #region List

    public static List<K> Map<T, K>(this List<T> list, System.Func<T, K> map) {
        List<K> result = new List<K>();
        foreach (T item in list) {
            result.Add(map(item));
        }

        return result;
    }

    public static K Reduce<T, K>(this List<T> list, System.Func<K, T, K> reduce) {
        K result = default;
        foreach (T item in list) {
            result = reduce(result, item);
        }

        return result;
    }

    public static List<T> Filter<T>(this List<T> list, System.Predicate<T> filter) {
        List<T> result = new List<T>();
        foreach (T item in list) {
            if (filter(item)) {
                result.Add(item);
            }
        }

        return result;
    }

    public static void Shuffle<T>(this List<T> list) {
        if (list.Count == 0) {
            return;
        }

        List<T> cloneList = new List<T>(list);
        list.Clear();

        while (cloneList.Count > 0) {
            list.Add(Random.Retrieve(cloneList));
        }
    }

    #endregion List

    #region Boolean

    public static string ToPrettyString(this bool value) {
        return value ? "yes" : "no";
    }

    #endregion Boolean
}
