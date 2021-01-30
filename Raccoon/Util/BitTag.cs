using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util {
    public struct BitTag : System.IEquatable<BitTag>, IEnumerable<BitTag>, IEnumerable, System.IConvertible {
        #region Public Members

        public static readonly BitTag None = new BitTag(0UL),
                                      All = new BitTag(ulong.MaxValue);

        public ulong LiteralValue;

        #endregion Public Members

        #region Constructors

        public BitTag(System.IConvertible flags, System.Type enumType) {
            LiteralValue = (ulong) flags.ToInt64(System.Globalization.CultureInfo.InvariantCulture);

            if (enumType == null || !enumType.IsEnum) {
                EnumType = null;
            } else {
                EnumType = enumType;
            }
        }

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
        public string BinaryRepresentation { 
            get {
                return System.Convert.ToString((long) LiteralValue, 2);
            }
        }

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

        public T ToEnum<T>() {
            System.Type t = typeof(T);
            if (!t.IsEnum) {
                throw new System.ArgumentException($"Expecting a valid {nameof(System.Enum)}, but found '{t.Name}'.");
            }

            return (T) System.Enum.ToObject(typeof(T), LiteralValue);
        }

        public System.Enum ToEnum() {
            if (EnumType == null) {
                throw new System.InvalidOperationException("Can't cast BitTag to enum, registered EnumType is null.");
            }

            return (System.Enum) System.Enum.ToObject(EnumType, LiteralValue);
        }

        public override bool Equals(object obj) {
            return obj is BitTag otherBitTag && Equals(otherBitTag);
        }

        public bool Equals(BitTag other) {
            return EnumType == other.EnumType && LiteralValue == other.LiteralValue;
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

            return BinaryRepresentation;
        }

        #region System.IConvertible Implementation

        public System.TypeCode GetTypeCode() {
            return System.TypeCode.Object;
        }

        public bool ToBoolean(System.IFormatProvider provider) {
            throw new System.InvalidCastException();
        }

        public byte ToByte(System.IFormatProvider provider) {
            return System.Convert.ToByte(LiteralValue, provider);
        }

        public char ToChar(System.IFormatProvider provider) {
            return System.Convert.ToChar(LiteralValue, provider);
        }

        public System.DateTime ToDateTime(System.IFormatProvider provider) {
            throw new System.InvalidCastException();
        }

        public decimal ToDecimal(System.IFormatProvider provider) {
            return System.Convert.ToDecimal(LiteralValue, provider);
        }

        public double ToDouble(System.IFormatProvider provider) {
            return System.Convert.ToDouble(LiteralValue, provider);
        }

        public short ToInt16(System.IFormatProvider provider) {
            return System.Convert.ToInt16(LiteralValue, provider);
        }

        public int ToInt32(System.IFormatProvider provider) {
            return System.Convert.ToInt32(LiteralValue, provider);
        }

        public long ToInt64(System.IFormatProvider provider) {
            return System.Convert.ToInt64(LiteralValue, provider);
        }

        public sbyte ToSByte(System.IFormatProvider provider) {
            return System.Convert.ToSByte(LiteralValue, provider);
        }

        public float ToSingle(System.IFormatProvider provider) {
            return System.Convert.ToSingle(LiteralValue, provider);
        }

        public string ToString(System.IFormatProvider provider) {
            return System.Convert.ToString(LiteralValue, provider);
        }

        public object ToType(System.Type conversionType, System.IFormatProvider provider) {
            if (!conversionType.IsEnum) {
                throw new System.InvalidCastException($"Can't convert to '{conversionType}', it must be an enum type.");
            }

            return System.Enum.ToObject(conversionType, LiteralValue);
        }

        public ushort ToUInt16(System.IFormatProvider provider) {
            return System.Convert.ToUInt16(LiteralValue, provider);
        }

        public uint ToUInt32(System.IFormatProvider provider) {
            return System.Convert.ToUInt32(LiteralValue, provider);
        }

        public ulong ToUInt64(System.IFormatProvider provider) {
            return System.Convert.ToUInt64(LiteralValue, provider);
        }

        #endregion System.IConvertible Implementation

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

        /*
        public static implicit operator System.Enum(BitTag bitTag) {
            return (System.Enum) bitTag.ToEnum();
        }
        */

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
            return new BitTag(l.LiteralValue & r.LiteralValue, l.EnumType ?? r.EnumType);
        }

        public static BitTag operator |(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue | r.LiteralValue, l.EnumType ?? r.EnumType);
        }

        public static BitTag operator +(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue | r.LiteralValue, l.EnumType ?? r.EnumType);
        }

        public static BitTag operator -(BitTag l, BitTag r) {
            return new BitTag(l.LiteralValue & ~r.LiteralValue, l.EnumType ?? r.EnumType);
        }

        #endregion Operators
    }
}
