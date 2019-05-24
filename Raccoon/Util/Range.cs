﻿using System.Text.RegularExpressions;

namespace Raccoon.Util {
    public struct Range {
        #region Public Members

        public static readonly Range Empty = new Range(0, 0);

        #endregion Public Members

        #region Private Members

        private static readonly Regex StringFormatRegex = new Regex(@"(\-?\d+(?:\.?\d+)?)\s*\-\s*(\-?\d+(?:\.?\d+)?)");

        public float Min, Max;

        #endregion Private Members

        #region Constructors

        public Range(float min, float max) {
            if (min > max) {
                throw new System.ArgumentException("Invalid interval, 'max' must be greater than 'min'");
            }

            Min = min;
            Max = max;
        }

        public Range(Range range) {
            Min = range.Min;
            Max = range.Max;
        }

        #endregion Constructors

        #region Public Properties

        public float Length { get { return Max - Min; } }

        #endregion Public Properties

        #region Public Methods

        public static Range Union(Range rangeA, Range rangeB) {
            return new Range(Math.Min(rangeA.Min, rangeB.Min), Math.Max(rangeA.Max, rangeB.Max));
        }

        public static Range Parse(string value) {
            MatchCollection matches = StringFormatRegex.Matches(value);

            if (matches.Count == 0 || !matches[0].Success) {
                throw new System.FormatException($"String '{value}' doesn't not typify a Range.");
            }

            return new Range(
                float.Parse(matches[0].Groups[1].Value),
                float.Parse(matches[0].Groups[2].Value)
            );
        }

        public static bool TryParse(string value, out Range result) {
            MatchCollection matches = StringFormatRegex.Matches(value);

            if (matches.Count == 0 || !matches[0].Success) {
                result = Empty;
                return false;
            }

            result = new Range(
                float.Parse(matches[0].Groups[1].Value),
                float.Parse(matches[0].Groups[2].Value)
            );

            return true;
        }

        public bool Overlaps(Range range) {
            return !(Min > range.Max || range.Min > Max);
        }

        public bool Overlaps(Range range, out float amount) {
            if (Overlaps(range)) {
                amount = Math.Min(Max, range.Max) - Math.Max(Min, range.Min);
                return true;
            }

            amount = 0;
            return false;
        }

        public bool Overlaps(Range range, out Range overlappedRange) {
            if (Overlaps(range)) {
                overlappedRange = new Range(Math.Max(Min, range.Min), Math.Min(Max, range.Max));
                return true;
            }

            overlappedRange = new Range();
            return false;
        }

        public float Clamp(float value) {
            return Math.Clamp(value, Min, Max);
        }

        public float Distance(Range range) {
            if (Overlaps(range)) {
                return 0;
            }

            return range.Min > Max ? range.Min - Max : Min - range.Max;
        }

        public bool Contains(float value) {
            return value >= Min && value <= Max;
        }

        public bool ContainsExclusive(float value) {
            return value > Min && value < Max;
        }

        public override string ToString() {
            return $"[Min: {Min}, Max: {Max}, Length: {Length}]";
        }

        #endregion Public Methods

        #region Operators

        public static Range operator -(Range r) {
            return new Range(-r.Min, -r.Max);
        }

        public static Range operator +(Range l, Range r) {
            return new Range(l.Min + r.Min, l.Max + r.Max);
        }

        public static Range operator +(Range l, float r) {
            return new Range(l.Min + r, l.Max + r);
        }

        public static Range operator +(Range l, int r) {
            return new Range(l.Min + r, l.Max + r);
        }

        public static Range operator -(Range l, Range r) {
            return new Range(l.Min - r.Min, l.Max - r.Max);
        }

        public static Range operator -(Range l, float r) {
            return new Range(l.Min - r, l.Max - r);
        }

        public static Range operator -(Range l, int r) {
            return new Range(l.Min - r, l.Max - r);
        }

        #endregion Operators
    }
}
