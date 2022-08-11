using System.Reflection;

namespace Raccoon.Data.Parsers {
    public class AnyValueToken : ValueToken {
        public AnyValueToken() : base() {
        }

        public AnyValueToken(object value) : base() {
            Value = value;
        }

        public object Value { get; set; }

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

            if (Value == null) {
                if (!info.PropertyType.IsClass) {
                    /*
                    throw new System.ArgumentException(
                        $"Can't apply null value to type '{info.PropertyType}', which is a struct.",
                        nameof(info)
                    );
                    */
                    info.SetValue(target, System.Activator.CreateInstance(info.PropertyType));
                } else {
                    info.SetValue(target, Value);
                }
            } else if (info.PropertyType.IsAssignableFrom(Value.GetType())) {
                info.SetValue(target, Value);
            }

            // try to infer type using PropertyInfo
            TypeToken inferedType = new TypeToken(GetValueType(info.PropertyType));
            ValueToken inferedTypeValueToken = inferedType.CreateValueToken(Value);

            if (inferedTypeValueToken is AnyValueToken) {
                throw new System.ArgumentException(
                    $"Can't infer type from value type '{info.PropertyType}'.",
                    nameof(info)
                );
            }

            inferedTypeValueToken.SetPropertyValue(target, info);
        }

        public override string ToString() {
            return $"AnyValue {Value ?? "_"} ({Value?.GetType().ToString() ?? "?"})";
        }
    }
}
