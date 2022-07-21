using System.Collections.Generic;
using Raccoon.Data.Parsers;

namespace Raccoon.Data {
    public class SimpleDataTokensConsumer : DataTokensConsumer {
        #region Private Members

        private static SimpleDataTokensConsumer _instance;

        private Stack<Context> _stack = new Stack<Context>();

        #endregion Private Members

        #region Constructors

        private SimpleDataTokensConsumer() {
        }

        #endregion Constructors

        #region Public Properties

        public static SimpleDataTokensConsumer Instance {
            get {
                if (_instance == null) {
                    _instance = new SimpleDataTokensConsumer();
                }

                return _instance;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Consume(
            object rootTarget,
            DataContract rootContract,
            ListToken<Token> root
        ) {
            _stack.Clear();
            _stack.Push(new Context(
                root.Entries.GetEnumerator(),
                rootTarget,
                rootContract
            ));

            while (_stack.TryPeek(out Context context)) {
                if (!context.Enumerator.MoveNext()) {
                    _stack.Pop();
                    continue;
                }

                Token token = context.Enumerator.Current;

                // ident: type = value
                if (TryHandleValueAssignToken(context.Target, token, context.Contract)) {
                    continue;
                }

                // ident
                //     ...
                if (TryHandleNamedListToken(context.Target, token, context.Contract)) {
                    continue;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private bool TryHandleValueAssignToken(
            object target,
            Token token,
            DataContract contract
        ) {
            if (!(token is ValueAssignToken valueAssignToken)) {
                return false;
            }

            DataContract.Property property
                = contract.Find(
                    valueAssignToken.Identifier,
                    valueAssignToken.Type
                );

            if (property == null) {
                if (contract.FailOnNotFound) {
                    string type = valueAssignToken.Type != null ? (" (" + valueAssignToken.Type + ")") : "";
                    throw new System.InvalidOperationException($"Property, with name '{valueAssignToken.Identifier.Name}'{type}, was not found at target type '{target.GetType().Name}'.");
                }

                return false;
            }

            property.SetValue(target, valueAssignToken.Value);
            return true;
        }

        private bool TryHandleNamedListToken(
            object target,
            Token token,
            DataContract contract
        ) {
            if (!(token is NamedListToken<Token> namedListToken)) {
                return false;
            }

            DataContract.Property property = contract.Find(namedListToken.Identifier);

            if (property == null) {
                if (contract.FailOnNotFound) {
                    throw new System.InvalidOperationException($"Property, with name '{namedListToken.Identifier.Name}', was not found at target type '{target.GetType().Name}'.");
                }

                return false;
            }

            if (property.SubDataContract == null) {
                throw new System.InvalidOperationException($"Trying to consume token '{namedListToken.GetType().Name}' to property '{property.Info.Name}' (at {property.Info.DeclaringType.Name}), but, it's type '{property.Info.PropertyType.Name}', isn't marked with {nameof(DataContractAttribute)}.");
            }

            object subTarget = property.Info.GetValue(target);

            if (subTarget == null) {
                throw new System.InvalidOperationException($"Property '{property.Info.Name}' (at {property.Info.DeclaringType.Name}) isn't initialized.");
            }

            _stack.Push(new Context(
                namedListToken.Entries.GetEnumerator(),
                subTarget,
                property.SubDataContract
            ));

            return true;
        }

        #endregion Private Methods

        #region Context Class

        private class Context {
            public Context(IEnumerator<Token> enumerator, object target, DataContract contract) {
                Enumerator = enumerator;
                Target = target;
                Contract = contract;
            }

            public IEnumerator<Token> Enumerator { get; }
            public object Target { get; }
            public DataContract Contract { get; }
        }

        #endregion Context Class
    }
}
