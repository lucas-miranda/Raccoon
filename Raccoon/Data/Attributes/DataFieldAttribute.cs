
namespace Raccoon.Data {
    [System.AttributeUsage(
        System.AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = true
    )]
    public class DataFieldAttribute : System.Attribute {
        public DataFieldAttribute() {
        }

        /// <summary>
        /// Field name is converted to this font case when looking for a field name.
        /// </summary>
        public FontCase Case { get; set; }
    }
}
