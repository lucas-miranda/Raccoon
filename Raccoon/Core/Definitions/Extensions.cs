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

    public static bool Has(this System.Enum e, System.Enum tag) {
        ulong t = System.Convert.ToUInt64(tag);
        return (System.Convert.ToUInt64(e) & t) == t;
    }

    public static bool HasAny(this System.Enum e, System.Enum tag) {
        return (System.Convert.ToUInt64(e) & System.Convert.ToUInt64(tag)) != 0UL;
    }

    #endregion Enums

    #region String

    private static readonly char[] WordSeparator = new char[] { ' ' };
    private static readonly System.Text.StringBuilder Builder = new System.Text.StringBuilder();

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


    public static string ToCamelCase(this string str, bool ignoreSingleLetters = false) {
        Builder.Clear();

        int startIndex = 0,
            endIndex;

        bool isWordSeparator = false,
             isFirstWord = true;

        while (startIndex < str.Length) {
            for (endIndex = startIndex; endIndex < str.Length; endIndex++) {
                char c = str[endIndex];

                if (System.Array.IndexOf(WordSeparator, c) >= 0) {
                    isWordSeparator = true;
                    break;
                } else if (endIndex > startIndex && char.IsUpper(c)) {
                    break;
                }
            }

            if (endIndex > startIndex) {
                // push word

                if (isFirstWord) {
                    Builder.Append(str.Substring(startIndex, (endIndex - startIndex)).ToLowerInvariant());
                    isFirstWord = false;
                } else if (endIndex - startIndex == 1) {
                    // handle single char

                    if (ignoreSingleLetters) {
                        Builder.Append(str[startIndex]);
                    } else {
                        Builder.Append(char.ToUpperInvariant(str[startIndex]));
                    }
                } else {
                    Builder.Append(char.ToUpperInvariant(str[startIndex]));
                    Builder.Append(str, startIndex + 1, endIndex - (startIndex + 1));
                }
            }

            if (isWordSeparator) {
                // push separator
                Builder.Append(str, endIndex, 1);
                isWordSeparator = false;
                startIndex = endIndex + 1;
            } else {
                startIndex = endIndex;
            }
        }

        string result = Builder.ToString();
        Builder.Clear();
        return result;
    }

    public static string ToCapitalized(this string str, bool ignoreSingleLetters = false) {
        Builder.Clear();

        int startIndex = 0,
            endIndex;

        while (startIndex < str.Length) {
            for (endIndex = startIndex; endIndex < str.Length; endIndex++) {
                if (System.Array.IndexOf(WordSeparator, str[endIndex]) >= 0) {
                    break;
                }
            }

            if (endIndex > startIndex) {
                // push word

                if (endIndex - startIndex == 1) {
                    if (ignoreSingleLetters) {
                        Builder.Append(str[startIndex]);
                    } else {
                        Builder.Append(char.ToUpperInvariant(str[startIndex]));
                    }
                } else {
                    Builder.Append(char.ToUpperInvariant(str[startIndex]));
                    Builder.Append(str, startIndex + 1, endIndex - (startIndex + 1));
                }
            }

            // push separator
            if (endIndex < str.Length) {
                Builder.Append(str, endIndex, 1);
            }

            startIndex = endIndex + 1;
        }

        string result = Builder.ToString();
        Builder.Clear();
        return result;
    }

    public static string SeparateCapitalized(this string str, char separationChar = ' ') {
        Builder.Clear();
        string separation = separationChar.ToString();
        int startIndex = 0;

        for (int i = 1; i < str.Length; i++) {
            if (char.IsUpper(str[i]) && str[i - 1] != separationChar) {
                Builder.Append(str, startIndex, i - startIndex);
                Builder.Append(separation);
                startIndex = i;
            }
        }

        if (startIndex < str.Length) {
            Builder.Append(str, startIndex, str.Length - startIndex);
        }

        string result = Builder.ToString();
        Builder.Clear();
        return result;
    }

    public static string RemoveAll(this string str, char removeChar) {
        Builder.Clear();

        int startIndex = 0;

        for (int i = 0; i < str.Length; i++) {
            char c = str[i];

            if (c == removeChar) {
                if (i > startIndex) {
                    Builder.Append(str, startIndex, i - startIndex);
                }

                startIndex = i + 1;
            }
        }

        if (startIndex < str.Length) {
            Builder.Append(str, startIndex, str.Length - startIndex);
        }

        string result = Builder.ToString();
        Builder.Clear();
        return result;
    }

    public static IEnumerable<string> SplitYield(this string str, char split) {
        int startIndex = 0;

        for (int i = 0; i < str.Length; i++) {
            char c = str[i];

            if (c == split) {
                if (i - startIndex > 0) {
                    yield return str.Substring(startIndex, i - startIndex);
                }

                startIndex = i + 1;
            }
        }

        if (startIndex < str.Length) {
            yield return str.Substring(startIndex, str.Length - startIndex);
        }
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

    public static T Retrieve<T>(this IList<T> list, int index) {
        T e = list[index];
        list.RemoveAt(index);
        return e;
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

    #region Stack

    public static bool TryPeek<T>(this Stack<T> stack, out T value) {
        if (stack.Count == 0) {
            value = default(T);
            return false;
        }

        value = stack.Peek();
        return true;
    }

    public static bool TryPop<T>(this Stack<T> stack, out T value) {
        if (stack.Count == 0) {
            value = default(T);
            return false;
        }

        value = stack.Pop();
        return true;
    }

    #endregion Stack

    #region Boolean

    public static string ToPrettyString(this bool value) {
        return value ? "yes" : "no";
    }

    #endregion Boolean
}
