using System.Collections.Generic;
using Raccoon.Data.Parsers;

namespace Raccoon.Data {
    /// <summary>
    /// Produce tokens from a given object, using it's DataContract.
    /// Used when preparing an object to export as a data file.
    /// </summary>
    public class SimpleDataTokensProducer : DataTokensProducer {
        #region Private Members

        private static SimpleDataTokensProducer _instance;

        private Stack<Context> _stack = new Stack<Context>();

        #endregion Private Members

        #region Constructors

        private SimpleDataTokensProducer() {
        }

        #endregion Constructors

        #region Public Properties

        public static SimpleDataTokensProducer Instance {
            get {
                if (_instance == null) {
                    _instance = new SimpleDataTokensProducer();
                }

                return _instance;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public ListToken<Token> Produce(object rootTarget, DataContract rootContract) {
            ListToken<Token> root = new ListToken<Token>();

            _stack.Clear();
            _stack.Push(new Context(rootTarget, rootContract, root));

            while (_stack.TryPeek(out Context context)) {
                if (!context.Enumerator.MoveNext()) {
                    _stack.Pop();
                    continue;
                }

                DataContract.Property property = context.Enumerator.Current;

                if (property.SubDataContractDescriptor != null) {
                    // ident
                    //     ...

                    NamedListToken<Token> listTokens = new NamedListToken<Token>(
                        new IdentifierToken(property.DisplayName)
                    );

                    context.OutputListToken.Entries.Add(listTokens);

                    _stack.Push(new Context(
                        property.Info.GetValue(context.Target),
                        property.SubDataContract,
                        listTokens
                    ));
                } else {
                    // ident: type = value
                    TypeToken typeToken = new TypeToken(property.Info.PropertyType);

                    ValueAssignToken valueAssignToken = new ValueAssignToken(
                        new IdentifierToken(property.DisplayName),
                        typeToken,
                        typeToken.CreateValueToken(property.Info.GetValue(context.Target))
                    );

                    context.OutputListToken.Entries.Add(valueAssignToken);
                }
            }

            return root;
        }

        #endregion Public Methods

        #region Context Class

        private class Context {
            public Context(
                object target,
                DataContract contract,
                ListToken<Token> outputListToken
            ) {
                Target = target;
                Contract = contract;
                Enumerator = contract.Properties.GetEnumerator();
                OutputListToken = outputListToken;
            }

            public IEnumerator<DataContract.Property> Enumerator { get; }
            public object Target { get; }
            public DataContract Contract { get; }
            public ListToken<Token> OutputListToken { get; }
        }

        #endregion Context Class
    }
}
