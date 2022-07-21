using System.Collections.Generic;
using System.Reflection;

using Raccoon.Data.Parsers;
using Raccoon.Util.Collections;

namespace Raccoon.Data {
    public partial class DataContract {
        #region Private Members

        private static readonly char[] CharsToRemoveFromName = new char[] { ' ' };

        private List<Property> _properties = new List<Property>();

        #endregion Private Members

        #region Constructors

        public DataContract(System.Type targetType, DataContractAttribute attribute) {
            if (targetType == null) {
                throw new System.ArgumentNullException(nameof(targetType));
            }

            if (attribute != null) {
                DefaultFieldNameCase = attribute.DefaultFieldNameCase;
                FailOnNotFound = attribute.FailOnNotFound;
            }

            Properties = new ReadOnlyList<Property>(_properties);

            // acquire properties
            foreach (PropertyInfo propertyInfo in targetType.GetProperties()) {
                DataFieldAttribute dataFieldAttr
                    = propertyInfo.GetCustomAttribute<DataFieldAttribute>(true);

                if (dataFieldAttr == null) {
                    continue;
                }

                RegisterProperty(propertyInfo, dataFieldAttr);
            }
        }

        #endregion Constructors

        #region Public Properties

        public FontCase DefaultFieldNameCase { get; private set; } = FontCase.LowerCase;
        public bool FailOnNotFound { get; private set; } = true;
        public ReadOnlyList<Property> Properties { get; }

        #endregion Public Properties

        #region Public Methods

        public void RegisterProperty(PropertyInfo info, DataFieldAttribute attribute) {
            _properties.Add(new Property(info, attribute, DefaultFieldNameCase));
        }

        public Property Find(IdentifierToken identifier, TypeToken type) {
            string name = identifier.Name.RemoveAll(CharsToRemoveFromName[0]);

            if (type == null) {
                foreach (Property property in _properties) {
                    if (property.HasName(name)) {
                        return property;
                    }
                }
            } else {
                TypeDescriptorAttribute typeDescriptor = type.Type.Descriptor();

                foreach (Property property in _properties) {
                    if (property.HasName(name)
                     && typeDescriptor.ValueType.IsAssignableFrom(property.Info.PropertyType)
                    ) {
                        return property;
                    }
                }
            }

            return null;
        }

        public Property Find(IdentifierToken identifier) {
            return Find(identifier, null);
        }

        #endregion Public Methods
    }
}
