using System.Collections;

using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public abstract class DataTokensConsumer {
        #region Constructors

        protected DataTokensConsumer() {
            Context = new ConsumerContext();
        }

        #endregion Constructors

        #region Public Properties

        public ConsumerContext Context { get; }

        #endregion Public Properties

        #region Public Methods

        public abstract void Consume(
            object rootTarget,
            DataContract rootContract,
            ListToken<Token> root
        );

        public virtual bool HandleCustomValueAssignToken(
            ValueAssignToken valueAssignToken,
            out object value
        ) {
            throw new System.NotImplementedException($"Custom type '{valueAssignToken.Type.Custom}' isn't handled. (at identifier: '{valueAssignToken.Identifier.Name}')");
        }

        public virtual bool HandleCustomNamedListToken(
            IdentifierToken identToken,
            TypeToken typeToken,
            ListToken<Token> listToken,
            out object value
        ) {
            throw new System.NotImplementedException(
                $"Custom type '{typeToken.Custom}' isn't handled. (at identifier: '{identToken.Name}')"
            );
        }

        /// <summary>
        /// Register value to provided target ICollection (it must be initialized already).
        /// </summary>
        public bool RegisterValue(object target, IdentifierToken identifier, object value) {
            if (target == null) {
                throw new System.ArgumentNullException(nameof(target));
            }

            if (!(target is ICollection)) {
                throw new System.ArgumentException(nameof(target), $"Can't register value. Target isn't a {nameof(ICollection)}.");
            }

            if (target is IDictionary targetDict) {
                if (identifier == null) {
                    throw new System.ArgumentNullException(nameof(identifier));
                }

                targetDict.Add(identifier.Name, value);
                return true;
            } else if (target is IList targetList) {
                targetList.Add(value);
                return true;
            } else {
                throw new System.NotImplementedException($"Target collection type '{target.GetType().ToString()}' isn't handled.");
            }
        }

        /// <summary>
        /// Register value to current context target ICollection (it must be initialized already).
        /// </summary>
        public bool RegisterValue(IdentifierToken identifier, object value) {
            ConsumerContext.Entry context = Context.Current;

            if (context == null) {
                throw new System.InvalidOperationException("Can't register value. There is no context available.");
            }

            if (context is ConsumerContext.ArrayEntry arrContext) {
                System.Array arr = (System.Array) arrContext.Target;

                if (arrContext.TargetIndex > arr.Length) {
                    throw new System.InvalidOperationException($"Can't register value. Target index ({arrContext.TargetIndex}) is out of {nameof(System.Array)} bounds [0, {arr.Length - 1}].");
                }

                try {
                    arr.SetValue(value, arrContext.TargetIndex);
                } catch (System.InvalidCastException e) {
                    throw new System.InvalidOperationException(
                        $"Type mismatch. Value '{value.ToString()}' ({value?.GetType().ToString() ?? "undefined type"}) can't be registered at {nameof(System.Array)} with type {arr.GetType().ToString()}.",
                        e
                    );
                }

                arrContext.AdvanceIndex();
                return true;
            }

            return RegisterValue(context.Target, identifier, value);
        }

        /// <summary>
        /// Apply a value to a target's property, using it's identifier.
        /// </summary>
        public bool ApplyValue(DataContract contract, object target, IdentifierToken identifier, ValueToken value) {
            if (contract == null) {
                throw new System.ArgumentNullException(nameof(contract));
            }

            if (target == null) {
                throw new System.ArgumentNullException(nameof(target));
            }

            if (identifier == null) {
                throw new System.ArgumentNullException(nameof(identifier));
            }

            DataContract.Property property = contract.Find(identifier);

            if (property == null) {
                if (contract.FailOnNotFound) {
                    throw new System.InvalidOperationException($"Property, with name '{identifier.Name}', was not found at target type '{target.GetType().ToString()}'.\nAvailable properties found: {contract.PropertiesToString()}");
                }

                return false;
            }

            property.SetValue(target, value);
            return true;
        }

        /// <summary>
        /// Apply a value to current context target's property, using it's identifier.
        /// </summary>
        public bool ApplyValue(IdentifierToken identifier, ValueToken value) {
            ConsumerContext.Entry context = Context.Current;

            if (context == null) {
                throw new System.InvalidOperationException("Can't apply value. There is no context available.");
            }

            return ApplyValue(context.Contract, context.Target, identifier, value);
        }

        #endregion Public Methods
    }
}
