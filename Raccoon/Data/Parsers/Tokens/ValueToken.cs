using System.Reflection;

namespace Raccoon.Data.Parsers {
    public abstract class ValueToken : Token {
        public ValueToken() : base(TokenKind.Value) {
        }

        public abstract string AsString();
        public abstract object AsObject();
        public abstract void SetPropertyValue(object target, PropertyInfo info);

        /// <summary>
        /// Get meaningful value type from a type.
        /// </summary>
        protected System.Type GetValueType(System.Type valueType) {
            if (valueType.IsGenericType
             && valueType.GetGenericTypeDefinition() == typeof(System.Nullable<>)
            ) {
                // property type is System.Nullable<_>
                System.Type[] genericArgs = valueType.GenericTypeArguments;

                if (genericArgs.Length == 1) {
                    // use first type at System.Nullable<_>
                    return genericArgs[0];
                }
            }

            return valueType;
        }
    }
}
