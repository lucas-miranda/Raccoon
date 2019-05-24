using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util {
    public struct BitTag : System.IEquatable<BitTag>, IEnumerable<BitTag>, IEnumerable {
        #region Public Members

        public static readonly BitTag None = new BitTag(0UL),
                                      All = new BitTag(ulong.MaxValue);

        public ulong LiteralValue;

        #endregion Public Members

        #region Constructors

        public BitTag(System.IConvertible flags) {
            LiteralValue = (ulong) flags.ToInt64(System.Globalization.CultureInfo.InvariantCulture);
            EnumType = null;
        }

        public BitTag(System.Enum flags) {
            LiteralValue = (ulong) System.Convert.ToInt64(flags);
            EnumType = flags.GetType();
        }

        public BitTag(ulong flags, System.Type enumType) : this() {
            if (enumType == null || !enumType.IsEnum) {
                LiteralValue = flags;
                EnumType = null;
                return;
            }

            LiteralValue = flags;
            EnumType = enumType;
        }

        public BitTag(ulong flags) {
            LiteralValue = flags;
            EnumType = null;
        }

        public BitTag(BitTag tag) {
            LiteralValue = tag.LiteralValue;
            EnumType = tag.EnumType;
        }

        #endregion Constructors

        #region Public Properties

        public System.Type EnumType { get; private set; }
        public bool IsSingleValue { get { return Math.IsPowerOfTwo(LiteralValue); } }

        #endregion Public Properties

        #region Public Methods

        public bool Has(BitTag tag) {
            return (LiteralValue & tag.LiteralValue) == tag.LiteralValue;
        }

        public bool Has(System.IConvertible tag) {
            ulong otherLiteralValue = tag.ToUInt64(System.Globalization.CultureInfo.InvariantCulture);
            return (LiteralValue & otherLiteralValue) == otherLiteralValue;
        }

        public bool Has(System.Enum tag) {
            return Has((System.IConvertible) tag);
        }

        public bool HasAny(BitTag tag) {
            return (LiteralValue & tag.LiteralValue) != 0UL;
        }

        public bool HasAny(System.IConvertible tag) {
            return (LiteralValue & tag.ToUInt64(System.Globalization.CultureInfo.InvariantCulture)) != 0UL;
        }

        public bool HasAny(System.Enum tag) {
            return HasAny((System.IConvertible) tag);
        }

        public T ToEnum<T>() where T : System.Enum {
            return (T) System.Enum.ToObject(typeof(T), LiteralValue);
        }

        public object ToEnum() {
            return System.Enum.ToObject(EnumType, LiteralValue);
        }

        public override bool Equals(object obj) {
            return obj is BitTag ? Equals((BitTag) obj) : base.Equals(obj);
        }

        public bool Equals(BitTag other) {
            return LiteralValue == other.LiteralValue;
        }

        public override int GetHashCode() {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + LiteralValue.GetHashCode();
            return hashCode;
        }

        public IEnumerator<BitTag> GetEnumerator() {
            if (LiteralValue == 0UL) {
                yield break;
            }

            if (IsSingleValue) {
                yield return new BitTag(LiteralValue) { EnumType = EnumType };
                yield break;
            }

            int bits = (int) System.Math.Ceiling(System.Math.Log(LiteralValue, 2));
            for (int i = 0; i <= bits; i++) {
                ulong flagValue = 1UL << i;
                if ((LiteralValue & flagValue) == 0UL) {
                    continue;
                }

                yield return new BitTag(flagValue) { EnumType = EnumType };
            }
        }

        public override string ToString() {
            if (EnumType != null) {
                return $"{System.Enum.ToObject(EnumType, LiteralValue)}";
            }

            return $"{System.Convert.ToString((long) LiteralValue, 2)}";
        }

        #endregion Public Methods

        #region Private Methods

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion Private Methods

        #region Implicit Conversions

        public static implicit operator BitTag(System.Enum flags) {
            return new BitTag(flags);
        }

        public static implicit operator BitTag(ulong flags) {
            return new BitTag(flags);
        }

        public static implicit operator BitTag(int flags) {
            return new BitTag(System.Convert.ToUInt64(flags));
        }

        public static implicit operator BitTag(uint flags) {
            return new BitTag(System.Convert.ToUInt64(flags));
        }

        public static implicit operator BitTag(short flags) {
            return new BitTag(System.Convert.ToUInt64(flags));
        }

        public static implicit operator BitTag(ushort flags) {
            return new BitTag(System.Convert.ToUInt64(flags));
        }

        public static implicit operator BitTag(byte flags) {
            return new BitTag(System.Convert.ToUInt64(flags));
        }

        #endregion Implicit Conversions

        #region Operators

        public static bool operator ==(BitTag l, BitTag r) {
            return l.LiteralValue == r.LiteralValue;
        }

        public static bool operator !=(BitTag l, BitTag r) {
            return l.LiteralValue != r.LiteralValue;
        }

        public static BitTag operator ~(BitTag tag) {
            return new BitTag(~tag.LiteralValue, tag.EnumType);
        }

        public static BitTag operator &(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue & r.LiteralValue, l.EnumType);
        }

        public static BitTag operator |(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue | r.LiteralValue, l.EnumType);
        }

        public static BitTag operator +(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue | r.LiteralValue, l.EnumType);
        }

        public static BitTag operator -(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue & ~r.LiteralValue, l.EnumType);
        }

        #endregion Operators
    }
}
