namespace Raccoon.Components.StateMachine {
    public class Trigger {
        public Trigger(object value, Comparison comparisonType) {
            Value = value;
            CreateComparison(comparisonType);
        }

        public object Value { get; private set; }
        public System.Func<System.IComparable, bool> Comparison { get; private set; }

        private void CreateComparison(Comparison comparisonType) {
            switch (comparisonType) {
                case Raccoon.Comparison.Equals:
                    Comparison = (System.IComparable otherValue) => otherValue.CompareTo(Value) == 0;
                    break;

                case Raccoon.Comparison.Different:
                    Comparison = (System.IComparable otherValue) => otherValue.CompareTo(Value) != 0;
                    break;

                case Raccoon.Comparison.Greater:
                    Comparison = (System.IComparable otherValue) => otherValue.CompareTo(Value) > 0;
                    break;

                case Raccoon.Comparison.Less:
                    Comparison = (System.IComparable otherValue) => otherValue.CompareTo(Value) < 0;
                    break;

                case Raccoon.Comparison.GreaterOrEquals:
                    Comparison = (System.IComparable otherValue) => otherValue.CompareTo(Value) >= 0;
                    break;

                case Raccoon.Comparison.LessOrEquals:
                    Comparison = (System.IComparable otherValue) => otherValue.CompareTo(Value) <= 0;
                    break;
            }
        }
    }
}
