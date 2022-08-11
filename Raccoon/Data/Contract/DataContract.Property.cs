using System.Reflection;
using Raccoon.Data.Parsers;

namespace Raccoon.Data {
    public partial class DataContract {
        public class Property {
            public Property(
                PropertyInfo info,
                DataFieldAttribute attribute,
                FontCase defaultFieldNameCase
            ) {
                if (info == null) {
                    throw new System.ArgumentNullException(nameof(info));
                }

                if (attribute == null) {
                    throw new System.ArgumentNullException(nameof(attribute));
                }

                Info = info;
                Attribute = attribute;

                SubDataContractDescriptor
                    = info.PropertyType.GetCustomAttribute<DataContractAttribute>(true);

                if (SubDataContractDescriptor != null) {
                    SubDataContract = new DataContract(Info.PropertyType, SubDataContractDescriptor);
                } else {
                    SubDataContract = null;
                }

                FontCase fontCase;
                if (Attribute.Case == FontCase.None) {
                    fontCase = defaultFieldNameCase;
                } else {
                    fontCase = Attribute.Case;
                }

                DisplayName = fontCase.Apply(Info.Name, true);
                IdentifyName = DisplayName.RemoveAll(' ');
            }

            public string DisplayName { get; }
            public string IdentifyName { get; }
            public PropertyInfo Info { get; }
            public DataFieldAttribute Attribute { get; }
            public DataContractAttribute SubDataContractDescriptor { get; }
            public DataContract SubDataContract { get; }

            public bool HasName(string name) {
                return IdentifyName.Equals(name, System.StringComparison.InvariantCultureIgnoreCase);
            }

            public void SetValue(object target, ValueToken token) {
                token.SetPropertyValue(target, Info);
            }
        }
    }
}
