namespace Raccoon.Data.Parsers {
    public enum TypeKind {
        Custom = 0,

        [TypeDescriptor(typeof(System.Int32), "int32", "i32", "int", "i")]
        Int32,

        [TypeDescriptor(typeof(System.UInt32), "uint32", "u32", "uint", "u", "milli")]
        UInt32,

        [TypeDescriptor(typeof(System.Boolean), "bool")]
        Boolean,

        [TypeDescriptor(typeof(System.String), "string", "str")]
        String,

        [TypeDescriptor(typeof(Vector2), "vector2", "vec2", "position", "pos")]
        Vector2,

        [TypeDescriptor(typeof(Size), "size")]
        Size,

        [TypeDescriptor(typeof(System.Array), "vector", "vec")]
        Vector,
    }

    public static class TypeKindExtensions {
        public static TypeDescriptorAttribute Descriptor(this TypeKind kind) {
            return kind.GetAttribute<TypeDescriptorAttribute>();
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class TypeDescriptorAttribute : System.Attribute {
        public TypeDescriptorAttribute(System.Type valueType, params string[] names) {
            if (valueType == null) {
                throw new System.ArgumentNullException(nameof(valueType));
            }

            if (names == null || names.Length == 0) {
                throw new System.ArgumentException("It must contains at least 1 defined name.");
            }

            Names = names;
            ValueType = valueType;
        }

        public string[] Names { get; }
        public System.Type ValueType { get; }
    }
}
