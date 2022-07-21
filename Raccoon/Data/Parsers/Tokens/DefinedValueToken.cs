using System.Reflection;

namespace Raccoon.Data.Parsers {
    public class DefinedValueToken<T> : ValueToken {
        public DefinedValueToken() : base() {
            Value = default(T);
        }

        public DefinedValueToken(T value) : base() {
            Value = value;
        }

        public T Value { get; set; }

        public override string AsString() {
            return Value?.ToString() ?? string.Empty;
        }

        public override void SetPropertyValue(object target, PropertyInfo info) {
            if (target == null) {
                throw new System.NotSupportedException($"Target null isn't supported.");
            }

            if (info == null) {
                throw new System.ArgumentNullException(nameof(info));
            }

            if (!info.PropertyType.IsAssignableFrom(typeof(T))) {
                throw new System.ArgumentException(
                    $"Property '{info.Name}' (from type {target.GetType().Name}) has type '{info.PropertyType.Name}', but '{Value.GetType().Name}' is provided.",
                    nameof(info)
                );
            }

            info.SetValue(target, Value);
        }

        public override string ToString() {
            return $"DefinedValue {Value} ({Value?.GetType().Name ?? "?"})";
        }
    }
}
