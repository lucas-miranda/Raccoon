using System;

namespace Raccoon.Util {
    public class Range {
        #region Private Members

        private float _min, _max;

        #endregion Private Members

        #region Constructors

        public Range() {
            _min = _max = 0;
        }

        public Range(float min, float max) {
            if (min > max) throw new ArgumentException("Invalid interval, 'max' must be greater than 'min'");
            _min = min;
            Max = max;
        }

        public Range(Range range) {
            _min = range.Min;
            Max = range.Max;
        }

        #endregion Constructors

        #region Public Properties

        public float Length { get { return Max - Min; } }

        public float Min {
            get {
                return _min;
            }

            set {
                if (value > _max) throw new ArgumentException("Value must be less than 'Max'");

                _min = value;
            }
        }

        public float Max {
            get {
                return _max;
            }

            set {
                if (value < _min) throw new ArgumentException("Value must be greater than 'Min'");

                _max = value;
            }
        }

        #endregion Public Properties

        #region Public Static Methods

        public static Range Union(Range rangeA, Range rangeB) {
            return new Range(Math.Min(rangeA.Min, rangeB.Min), Math.Max(rangeA.Max, rangeB.Max));
        }

        #endregion Public Static Methods

        #region Public Methods

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

        public void Union(Range range) {
            Min = Math.Min(Min, range.Min);
            Max = Math.Max(Max, range.Max);
        }

        public override string ToString() {
            return $"[Min: {Min}, Max: {Max}, Length: {Length}]";
        }

        #endregion Public Methods
    }
}
