using System.Collections.Generic;
using System.Collections;

using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public class NamedListConsumerHandler : ConsumerHandler {
        #region Private Members

        private static NamedListConsumerHandler _instance;

        #endregion Private Members

        #region Constructors

        private NamedListConsumerHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public static NamedListConsumerHandler Instance {
            get {
                if (_instance == null) {
                    _instance = new NamedListConsumerHandler();
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
            if (!(token is NamedListToken<Token> namedListToken)) {
                return false;
            }

            System.Type targetType = target.GetType();

            if (namedListToken.Identifier is TypedIdentifierToken typedIdentToken) {
                // it must be handled or an error will be thrown
                bool handled = false;

                switch (typedIdentToken.Type.Type) {
                    case TypeKind.Vector:
                        {
                            if (TryNamedListAsVector(consumer, target, typedIdentToken, namedListToken, contract)) {
                                handled = true;
                            }
                        }
                        break;

                    case TypeKind.Custom:
                        {
                            if (!consumer.HandleCustomNamedListToken(
                                typedIdentToken,
                                typedIdentToken.Type,
                                namedListToken,
                                out object value
                            )) {
                                throw new System.InvalidOperationException($"Failed to handle custom named list (name: '{namedListToken.Identifier.Name}', custom type: '{typedIdentToken.Type.Custom}')");
                            }

                            // apply custom named list to target's property
                            consumer.RegisterValue(namedListToken.Identifier, value);
                            handled = true;
                        }
                        break;

                    default:
                        break;
                }

                if (!handled) {
                    throw new System.NotImplementedException(
                        $"{nameof(TypedIdentifierToken)}'s type '{typedIdentToken.Type.Type}' isn't handled, at {nameof(NamedListToken<Token>)} with identifier name '{typedIdentToken.Name}'."
                    );
                }

                return true;
            }

            if (target is ICollection) {
                // handle as an entry at target collection
                // identifier can be ignored at this case
                ConsumerContext.Entry context = consumer.Context.Current;

                if (context is ConsumerContext.ArrayEntry arrayContext) {
                    if (!consumer.HandleCustomNamedListToken(
                        namedListToken.Identifier,
                        arrayContext.ElementTypeToken,
                        namedListToken,
                        out object value
                    )) {
                        throw new System.InvalidOperationException(
                            $"Failed to handle custom named list (name: '{namedListToken.Identifier.Name}', type: '{arrayContext.ElementTypeToken}')"
                        );
                    }

                    consumer.RegisterValue(namedListToken.Identifier, value);
                    return true;
                }
            }

            if (TrySimpleNamedList(consumer, target, namedListToken, contract)) {
                return true;
            }

            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private bool TrySimpleNamedList(
            DataTokensConsumer consumer,
            object target,
            NamedListToken<Token> namedListToken,
            DataContract contract
        ) {
            if (contract == null) {
                throw new System.ArgumentException($"Expecting a {nameof(DataContract)} at property with identifier '{namedListToken.Identifier}'. (target type: {(target?.GetType().ToString() ?? "undefined type")})");
            }

            DataContract.Property property = contract.Find(namedListToken.Identifier);

            if (property == null) {
                if (contract.FailOnNotFound) {
                    throw new DataEntryNotFoundException(
                        namedListToken.Identifier.Name,
                        target.GetType(),
                        contract
                    );
                }

                return false;
            }

            if (property.SubDataContract == null && !(typeof(ICollection).IsAssignableFrom(property.Info.PropertyType))) {
                throw new System.InvalidOperationException(
                    $"Trying to consume token '{namedListToken.GetType().ToString()}' to property '{property.Info.Name}' (at {property.Info.DeclaringType.Name}), but, it's type '{property.Info.PropertyType.Name}', isn't marked with {nameof(DataContractAttribute)}."
                );
            }

            object subTarget = property.Info.GetValue(target);

            if (subTarget == null) {
                throw new System.InvalidOperationException($"Property '{property.Info.Name}' (at {property.Info.DeclaringType.Name}) isn't initialized.");
            }

            consumer.Context.Push(
                namedListToken,
                subTarget,
                property.SubDataContract
            );

            property.MarkAsReceivedValue();
            return true;
        }

        private bool TryNamedListAsVector(
            DataTokensConsumer consumer,
            object target,
            TypedIdentifierToken typedIdentToken,
            ListToken<Token> listToken,
            DataContract contract
        ) {
            DataContract.Property property = contract.Find(typedIdentToken);

            if (property == null) {
                if (contract.FailOnNotFound) {
                    throw new DataEntryNotFoundException(
                        typedIdentToken.Name,
                        target.GetType(),
                        contract
                    );
                }

                return false;
            }

            System.Type vectorType = property.Info.PropertyType;
            object vectorTarget = property.Info.GetValue(target);

            if (typeof(System.Array).IsAssignableFrom(vectorType)) {
                System.Array arr;

                if (vectorTarget == null) {
                    // create a System.Array
                    arr = System.Array.CreateInstance(
                        vectorType.GetElementType(),
                        listToken.Entries.Count
                    );

                    consumer.ApplyValue(
                        contract,
                        target,
                        typedIdentToken,
                        new DefinedValueToken<System.Array>(arr)
                    );
                } else {
                    // ensure System.Array has enough space
                    arr = (System.Array) vectorTarget;

                    if (arr.Length < listToken.Entries.Count) {
                        throw new System.InvalidOperationException(
                            $"{TypeKind.Vector} value already is initialized with a {nameof(System.Array)}, but it's length isn't enough, there is {listToken.Entries.Count} entries to be registered, but length is {arr.Length}."
                        );
                    }
                }

                consumer.Context.PushArray(listToken, typedIdentToken.Type, arr);
            } else if (vectorType.IsConstructedGenericType) {
                System.Type genericType = vectorType.GetGenericTypeDefinition();

                if (genericType == typeof(List<>)) {
                    IList list;

                    if (vectorTarget == null) {
                        list = (IList) System.Activator.CreateInstance(vectorType);

                        consumer.ApplyValue(
                            contract,
                            target,
                            typedIdentToken,
                            new DefinedValueToken<IList>(list)
                        );
                    } else {
                        list = (IList) vectorTarget;
                    }

                    consumer.Context.Push(listToken, list, null);
                } else {
                    throw new System.NotImplementedException($"Generic type '{genericType}' isn't handled as a vector.");
                }
            } else {
                return false;
            }

            return true;
        }

        #endregion Private Methods
    }
}
