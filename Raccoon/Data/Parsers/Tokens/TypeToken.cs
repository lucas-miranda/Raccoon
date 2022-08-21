using System.Collections.Generic;

namespace Raccoon.Data.Parsers {
    public class TypeToken : Token {
        #region Private Members

        private const char
                NestedTypeOpenDelimiter = '<',
                NestedTypeCloseDelimiter = '>',
                NestedTypeSeparator = ',';

        private static readonly char[]
                CustomTrimChars = new char[] { ' ' },
                NestedTypeDelimiters = new char[] {
                    NestedTypeOpenDelimiter,
                    NestedTypeCloseDelimiter
                };

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
                throw new System.ArgumentException($"Not found a {nameof(TypeKind)} which matches provided value type '{valueType.ToString()}'");
            }

            Set(kind);
        }

        #endregion Constructors

        #region Public Properties

        public TypeKind Type { get; private set; }
        public string Custom { get; private set; }
        public TypeToken[] Nested { get; private set; }
        public bool HasElementType { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Try to identify if it's a registered type and use as it.
        /// Otherwise, a TypeToken with a custom type is created.
        /// </summary>
        public static TypeToken CreateOrCustom(System.Type valueType) {
            if (valueType == null) {
                throw new System.ArgumentNullException(nameof(valueType));
            }

            if (TypeKindsByValueType.TryGetValue(valueType, out TypeKind kind)) {
                return new TypeToken(kind);
            }

            return new TypeToken(valueType.Name.ToString());
        }

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

            // try to identify provided "custom" value
            if (custom.IndexOfAny(NestedTypeDelimiters) >= 0) {
                // it should be handled as a generic type
                int openIndex = custom.IndexOf(NestedTypeOpenDelimiter),
                    closeIndex = custom.LastIndexOf(NestedTypeCloseDelimiter);

                if (openIndex < 0) {
                    throw new System.InvalidOperationException(
                        $"Missing an open delimiter '{NestedTypeOpenDelimiter}' at nested types, at type '{custom}'."
                    );
                } else if (closeIndex < 0) {
                    throw new System.InvalidOperationException(
                        $"Missing a close delimiter '{NestedTypeCloseDelimiter}' at nested types, at type '{custom}'."
                    );
                }

                // set current type as all the value before first open delimiter
                string type = custom.Substring(0, openIndex);
                Set(type);

                // extract nested types
                List<string> nestedTypes = new List<string>();
                int open = 0,
                    startIndex = openIndex + 1;

                for (int i = startIndex; i < closeIndex; i++) {
                    char c = custom[i];

                    if (c == NestedTypeOpenDelimiter) {
                        open += 1;
                    } else if (c == NestedTypeCloseDelimiter) {
                        open -= 1;
                    } else if (c == NestedTypeSeparator) {
                        // only register if there is no nested types opened
                        if (open == 0) {
                            nestedTypes.Add(custom.Substring(startIndex, i - startIndex));
                            startIndex = i + 1;
                        }
                    }
                }

                if (open > 0 || open < 0) {
                    throw new System.InvalidOperationException(
                        $"Malformed nested types.\nComplete type: {custom}"
                    );
                } else if (startIndex < closeIndex) {
                    nestedTypes.Add(custom.Substring(startIndex, closeIndex - startIndex));
                }

                if (Type != TypeKind.Custom) {
                    // identify expected generic values
                    TypeDescriptorAttribute descriptor = Type.Descriptor();
                    System.Type valueType = descriptor.ValueType;

                    int genericArgs;

                    if (valueType == typeof(System.Array)) {
                        genericArgs = 1;
                    } else {
                        if (!valueType.ContainsGenericParameters) {
                            throw new System.InvalidOperationException(
                                $"Type '{type}' ({valueType.Name}) don't have generic parameters. But {nestedTypes.Count} nested type{(nestedTypes.Count == 1 ? "" : "s")} was found."
                            );
                        }

                        genericArgs = valueType.GetGenericArguments().Length;
                    }

                    if (genericArgs != nestedTypes.Count) {
                        throw new System.InvalidOperationException(
                            $"Base type '{valueType.Name}' expects {genericArgs} nested type{(genericArgs == 1 ? "" : "s")}, but {nestedTypes.Count} type{(nestedTypes.Count == 1 ? "" : "s")} was provided."
                        );
                    }

                    // construct nested types array
                    Nested = new TypeToken[genericArgs];

                    for (int i = 0; i < Nested.Length; i++) {
                        Nested[i] = new TypeToken(nestedTypes[i]);
                    }

                    //

                    switch (Type) {
                        case TypeKind.Vector:
                            // Nested types represent element type
                            HasElementType = true;
                            break;

                        default:
                            break;
                    }

                    return;
                }

                // handling custom base type

                // construct nested types array
                Nested = new TypeToken[nestedTypes.Count];

                for (int i = 0; i < Nested.Length; i++) {
                    Nested[i] = new TypeToken(nestedTypes[i]);
                }

                return;
            } else if (TypeKindsNames.TryGetValue(custom.ToLower(), out TypeKind typeKind)) {
                // already exists a type with defined name, use it
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

                case TypeKind.UInt32:
                    return Converter.UInt32(value);

                case TypeKind.Single:
                    return Converter.Single(value);

                case TypeKind.Double:
                    return Converter.Double(value);

                case TypeKind.Boolean:
                    return Converter.Boolean(value);

                case TypeKind.String:
                    return Converter.String(value);

                case TypeKind.Vector2:
                    return Converter.Vector2(value);

                case TypeKind.Size:
                    return Converter.Size(value);

                // TODO handle types with Nested types

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
                System.Int32 result;

                if (value == null) {
                    result = default(System.Int32);
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

            public static ValueToken UInt32(object value) {
                System.UInt32 result;

                if (value == null) {
                    result = default(System.UInt32);
                } else if (value is System.UInt32 u) {
                    result = u;
                } else if (value is string str) {
                    if (!System.UInt32.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(System.UInt32)}."
                        );
                    }
                } else {
                    result = System.Convert.ToUInt32(value);
                }

                return new DefinedValueToken<System.UInt32>(result);
            }

            public static ValueToken Single(object value) {
                System.Single result;

                if (value == null) {
                    result = default(System.Single);
                } else if (value is System.Single u) {
                    result = u;
                } else if (value is string str) {
                    if (!System.Single.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(System.Single)}."
                        );
                    }
                } else {
                    result = System.Convert.ToSingle(value);
                }

                return new DefinedValueToken<System.Single>(result);
            }

            public static ValueToken Double(object value) {
                System.Double result;

                if (value == null) {
                    result = default(System.Double);
                } else if (value is System.Double u) {
                    result = u;
                } else if (value is string str) {
                    if (!System.Double.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(System.Double)}."
                        );
                    }
                } else {
                    result = System.Convert.ToSingle(value);
                }

                return new DefinedValueToken<System.Double>(result);
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

            public static ValueToken Vector2(object value) {
                Vector2 result;

                if (value == null) {
                    result = default(Vector2);
                } else if (value is Vector2 vec2) {
                    result = vec2;
                } else if (value is string str) {
                    if (!Raccoon.Vector2.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(Raccoon.Vector2)}."
                        );
                    }
                } else {
                    throw new System.InvalidOperationException(
                        $"Can't convert from {value.GetType().ToString()} ({value}) to {nameof(Raccoon.Vector2)}."
                    );
                }

                return new DefinedValueToken<Vector2>(result);
            }

            public static ValueToken Size(object value) {
                Size result;

                if (value == null) {
                    result = default(Size);
                } else if (value is Size size) {
                    result = size;
                } else if (value is string str) {
                    if (!Raccoon.Size.TryParse(str, out result)) {
                        throw new System.InvalidOperationException(
                            $"Failed to parse from '{str}' to {nameof(Raccoon.Size)}."
                        );
                    }
                } else {
                    throw new System.InvalidOperationException(
                        $"Can't convert from {value.GetType().ToString()} ({value}) to {nameof(Raccoon.Size)}."
                    );
                }

                return new DefinedValueToken<Size>(result);
            }
        }

        #endregion Converter Class
    }
}
