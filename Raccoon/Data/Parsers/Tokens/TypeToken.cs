using System.Collections.Generic;

namespace Raccoon.Data.Parsers {
    public class TypeToken : Token {
        #region Private Members

        private static readonly char[] CustomTrimChars = new char[] { ' ' };

        private static Dictionary<string, TypeKind> TypeKindsNames
            = new Dictionary<string, TypeKind>();

        private static Dictionary<System.Type, TypeKind> TypeKindsByValueType
            = new Dictionary<System.Type, TypeKind>();

        #endregion Private Members

        #region Constructors

        static TypeToken() {
            foreach (TypeKind typeKind in System.Enum.GetValues(typeof(TypeKind))) {
                TypeDescriptorAttribute descriptor
                    = typeKind.GetAttribute<TypeDescriptorAttribute>();

                if (descriptor == null) {
                    continue;
                }

                foreach (string name in descriptor.Names) {
                    TypeKindsNames.Add(name, typeKind);
                }

                TypeKindsByValueType.Add(descriptor.ValueType, typeKind);
            }
        }

        public TypeToken() : base(TokenKind.Type) {
        }

        public TypeToken(string custom) : base(TokenKind.Type) {
            Set(custom);
        }

        public TypeToken(TypeKind kind) : base(TokenKind.Type) {
            try {
                Set(kind);
            } catch (System.ArgumentException) {
                throw new System.ArgumentException(
                    "Use 'TypeToken(string)' to describe a custom type.",
                    nameof(kind)
                );
            }
        }

        public TypeToken(System.Type valueType) : base(TokenKind.Type) {
            if (valueType == null) {
                throw new System.ArgumentNullException(nameof(valueType));
            }

            if (!TypeKindsByValueType.TryGetValue(valueType, out TypeKind kind)) {
                throw new System.ArgumentException($"Not found a {nameof(TypeKind)} which matches provided value type '{valueType.Name}'");
            }

            Set(kind);
        }

        #endregion Constructors

        #region Public Properties

        public TypeKind Type { get; private set; }
        public string Custom { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Set(TypeKind kind) {
            if (kind == TypeKind.Custom) {
                throw new System.ArgumentException(
                    "Use 'Set(string)' to describe a custom type.",
                    nameof(kind)
                );
            }

            Type = kind;
            Custom = null;
        }

        public void Set(string custom) {
            custom = custom.Trim(CustomTrimChars);

            if (TypeKindsNames.TryGetValue(custom.ToLower(), out TypeKind typeKind)) {
                Set(typeKind);
                return;
            }

            Type = TypeKind.Custom;
            Custom = custom;
        }

        public ValueToken CreateValueToken(object value) {
            switch (Type) {
                case TypeKind.Custom:
                    return Converter.Custom(value);

                case TypeKind.Int32:
                    return Converter.Int32(value);

                case TypeKind.Boolean:
                    return Converter.Boolean(value);

                case TypeKind.String:
                    return Converter.String(value);

                default:
                    throw new System.NotImplementedException(
                        $"Type kind '{Type}' isn't handled."
                    );
            }
        }

        public override string ToString() {
            if (Type == TypeKind.Custom) {
                return $"Type Custom({Custom ?? "?"})";
            }

            return $"Type {Type}";
        }

        #endregion Public Methods

        #region Converter Class

        private static class Converter {
            public static ValueToken Custom(object value) {
                return new AnyValueToken(value);
            }

            public static ValueToken Int32(object value) {
                int result;

                if (value == null) {
                    result = default(int);
                } else if (value is System.Int32 i) {
                    result = i;
                } else if (value is string str) {
                    if (!System.Int32.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(System.Int32)}."
                        );
                    }
                } else {
                    result = System.Convert.ToInt32(value);
                }

                return new DefinedValueToken<System.Int32>(result);
            }

            public static ValueToken Boolean(object value) {
                bool result;

                if (value == null) {
                    result = default(bool);
                } else if (value is System.Boolean b) {
                    result = b;
                } else if (value is string str) {
                    if (!System.Boolean.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(System.Boolean)}."
                        );
                    }
                } else {
                    result = System.Convert.ToBoolean(value);
                }

                return new DefinedValueToken<System.Boolean>(result);
            }

            public static ValueToken String(object value) {
                string result;

                if (value == null) {
                    result = default(string);
                } else if (value is string str) {
                    result = str;
                } else {
                    result = System.Convert.ToString(value);
                }

                return new DefinedValueToken<System.String>(result);
            }
        }

        #endregion Converter Class
    }
}
