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

        public override object AsObject() {
            return Value;
        }

        public override void SetPropertyValue(object target, PropertyInfo info) {
            if (target == null) {
                throw new System.NotSupportedException($"Target null isn't supported.");
            }

            if (info == null) {
                throw new System.ArgumentNullException(nameof(info));
            }

            System.Type propertyType = GetValueType(info.PropertyType);

            if (!typeof(T).IsAssignableFrom(propertyType)) {
                throw new System.ArgumentException(
                    $"Property '{info.Name}' (from type {target.GetType().ToString()}) has type '{info.PropertyType.ToString()}', but a type '{typeof(T).ToString()}' is expected.",
                    nameof(info)
                );
            }

            if (!propertyType.IsAssignableFrom(Value.GetType())) {
                throw new System.ArgumentException(
                    $"Property '{info.Name}' (from type {target.GetType().ToString()}) has type '{info.PropertyType.ToString()}'. It can't receive a value with type '{Value.GetType().ToString()}'.",
                    nameof(info)
                );
            }

            info.SetValue(target, Value);
        }

        public override string ToString() {
            return $"DefinedValue {Value} ({Value?.GetType().ToString() ?? "?"})";
        }
    }
}
