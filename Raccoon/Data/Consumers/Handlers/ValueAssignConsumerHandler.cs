using System.Collections;

using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public class ValueAssignConsumerHandler : ConsumerHandler {
        #region Private Members

        private static ValueAssignConsumerHandler _instance;

        #endregion Private Members

        #region Constructors

        private ValueAssignConsumerHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public static ValueAssignConsumerHandler Instance {
            get {
                if (_instance == null) {
                    _instance = new ValueAssignConsumerHandler();
                }

                return _instance;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override bool TryHandle(
            DataTokensConsumer consumer,
            object target,
            Token token,
            DataContract contract
        ) {
            if (!(token is ValueAssignToken valueAssignToken)) {
                return false;
            }

            if (contract == null) {
                if (!(target is ICollection)) {
                    throw new System.InvalidOperationException($"Target doesn't have a required {nameof(DataContract)}.");
                }

                if (valueAssignToken.Type.Type == TypeKind.Custom) {
                    if (!consumer.HandleCustomValueAssignToken(valueAssignToken, out object value)) {
                        throw new System.InvalidOperationException(
                            $"Failed to handle custom value assign (name: '{valueAssignToken.Identifier.Name}', custom type: '{valueAssignToken.Type.Custom}')"
                        );
                    }

                    consumer.RegisterValue(valueAssignToken.Identifier, value);
                } else {
                    consumer.RegisterValue(valueAssignToken.Identifier, valueAssignToken.Value.AsObject());
                }

                return true;
            }

            DataContract.Property property
                = contract.Find(
                    valueAssignToken.Identifier,
                    valueAssignToken.Type
                );

            if (property == null) {
                if (contract.FailOnNotFound) {
                    throw new DataEntryNotFoundException(
                        valueAssignToken.Identifier.Name,
                        valueAssignToken.Type != null ? (" (" + valueAssignToken.Type + ")") : "",
                        target.GetType(),
                        contract
                    );
                }

                return false;
            }

            property.SetValue(target, valueAssignToken.Value);
            return true;
        }

        #endregion Public Methods
    }
}
