
namespace Raccoon.Graphics {
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class ShaderParameterAttribute : System.Attribute {
        public ShaderParameterAttribute() {
            CustomName = null;
        }

        public ShaderParameterAttribute(string customName) {
            CustomName = customName;
        }

        public string CustomName { get; }
    }
}
