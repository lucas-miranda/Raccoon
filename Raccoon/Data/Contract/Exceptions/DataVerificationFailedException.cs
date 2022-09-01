
namespace Raccoon.Data {
    public class DataVerificationFailedException : System.Exception {
        public DataVerificationFailedException(string message) : base(message) {
        }

        public DataVerificationFailedException(string message, System.Exception innerException) : base(message, innerException) {
        }

        public static DataVerificationFailedException Property(DataContract.Property property) {
            return new DataVerificationFailedException(
                $"Property (Display Name: {property.DisplayName}, Identify Name: {property.IdentifyName}, Type: {property.Info.PropertyType.ToString()} Received Value? {property.ReceivedValue})"
            );
        }
    }
}
