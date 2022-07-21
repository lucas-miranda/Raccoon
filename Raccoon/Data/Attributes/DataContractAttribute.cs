
namespace Raccoon.Data {
    [System.AttributeUsage(
        System.AttributeTargets.Class | System.AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = true
    )]
    public class DataContractAttribute : System.Attribute {
        public DataContractAttribute() {
        }

        /// <summary>
        /// Field name is converted to this font case when looking for a field name.
        /// When defined it becames the default setting, but can be overrided at specific field attribute.
        /// </summary>
        public FontCase DefaultFieldNameCase { get; set; } = FontCase.LowerCase;

        /// <summary>
        /// Contract will fail when a requested field, when consuming tokens, isn't found.
        /// </summary>
        public bool FailOnNotFound { get; set; } = true;
    }
}
