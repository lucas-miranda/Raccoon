
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

        /// <summary>
        /// Which operations this field is required to be handled.
        /// </summary>
        /// <remarks>
        /// What happens when following operations is required:
        ///     `DataOperation.Load`: Field must be loaded or an error is raised.
        ///     `DataOperation.Save`: Field will be written to the file.
        ///
        /// What happens when following operations *isn't* required:
        ///     `DataOperation.Load`: If, and only if, field is found, it'll be loaded, otherwise nothing happens.
        ///     `DataOperation.Save`: Field will not be written to the file.
        /// </remarks>
        public DataOperation Required { get; set; } = DataOperation.Save;
    }
}
