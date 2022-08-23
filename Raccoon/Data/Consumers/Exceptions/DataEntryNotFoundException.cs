
namespace Raccoon.Data.Consumers {
    public class DataEntryNotFoundException : System.Exception {
        public DataEntryNotFoundException(string name, System.Type targetType, DataContract contract)
            : base($"Property, with name '{name}', was not found at target type '{targetType.ToString()}'.\nAvailable properties found: {contract.PropertiesToString()}")
        {
        }

        public DataEntryNotFoundException(string name, string extraInfo, System.Type targetType, DataContract contract)
            : base($"Property, with name '{name}'{extraInfo}, was not found at target type '{targetType.ToString()}'.\nAvailable properties found: {contract.PropertiesToString()}")
        {
        }
    }
}
