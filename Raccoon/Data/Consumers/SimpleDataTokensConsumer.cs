using System.Collections.Generic;
using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    /// <summary>
    /// Reads tokens from a Token list and applies to a given object, using it's DataContract.
    /// Used when loading data from a parsed data file.
    /// </summary>
    public class SimpleDataTokensConsumer : DataTokensConsumer {
        #region Protected Methods

        protected delegate HandleResult ValueTypeHandler(ValueAssignToken valueAssignToken);

        protected delegate HandleResult StructureTypeHandler(
            IdentifierToken identToken,
            TypeToken typeToken,
            ListToken<Token> listToken
        );

        /// <summary>
        /// How should value be threated when a handling
        /// </summary>
        protected enum HandleStatus {
            /// <summary>
            /// Handling operation has failed.
            /// It gives consumer the option to retry with a different approach.
            /// </summary>
            Fail = 0,

            /// <summary>
            /// Handling operation completed.
            /// Token was handled properly and should be considered a success.
            /// </summary>
            Success,
        }

        #endregion Protected Methods

        #region Private Members

        private static SimpleDataTokensConsumer _instance;

        private Dictionary<string, ValueTypeHandler> _customValueTypeHandlers
            = new Dictionary<string, ValueTypeHandler>();

        private Dictionary<string, StructureTypeHandler> _customStructureTypeHandlers
            = new Dictionary<string, StructureTypeHandler>();

        #endregion Private Members

        #region Constructors

        protected SimpleDataTokensConsumer() {
            SetupHandlers();
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

        public override void Consume(
            object rootTarget,
            DataContract rootContract,
            ListToken<Token> root
        ) {
            Context.Clear();
            Context.Push(
                root,
                rootTarget,
                rootContract
            );

            while (Context.Peek(out ConsumerContext.Entry context)) {
                if (!context.Enumerator.MoveNext()) {
                    Context.Pop();
                    continue;
                }

                if (!HandleToken(context, context.Enumerator.Current)) {
                    Token current = context.Enumerator.Current,
                          next;

                    if (context.Enumerator.MoveNext()) {
                        next = context.Enumerator.Current;
                    } else {
                        next = null;
                    }

                    throw new System.InvalidOperationException($"Token ({current}) wasn't handled. (next: '{(next?.ToString() ?? "undefined")}')");
                }
            }
        }

        public override bool HandleCustomValueAssignToken(
            ValueAssignToken valueAssignToken,
            out object value
        ) {
            // try to use a registered custom handler
            if (_customValueTypeHandlers.TryGetValue(
                valueAssignToken.Type.Custom,
                out ValueTypeHandler handler
            )) {
                HandleResult result = handler.Invoke(valueAssignToken);

                if (result.IsSuccess) {
                    value = result.Value;
                    return true;
                }
            }

            return base.HandleCustomValueAssignToken(valueAssignToken, out value);
        }

        public override bool HandleCustomNamedListToken(
            IdentifierToken identToken,
            TypeToken typeToken,
            ListToken<Token> listToken,
            out object value
        ) {
            // try to use a registered custom handler
            if (_customStructureTypeHandlers.TryGetValue(
                typeToken.Custom,
                out StructureTypeHandler handler
            )) {
                HandleResult result = handler.Invoke(identToken, typeToken, listToken);

                if (result.IsSuccess) {
                    value = result.Value;
                    return true;
                }
            }

            return base.HandleCustomNamedListToken(
                identToken,
                typeToken,
                listToken,
                out value
            );
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Handle a given Token using pre-defined handlers.
        /// Every Token from token list is handled this way.
        /// </summary>
        protected bool HandleToken(ConsumerContext.Entry context, Token token) {
            // ident: type = value
            if (ValueAssignConsumerHandler.Instance.TryHandle(
                this,
                context.Target,
                token,
                context.Contract
            )) {
                return true;
            }

            // ident
            //     ...
            // ident: type
            //     ...
            if (NamedListConsumerHandler.Instance.TryHandle(
                this,
                context.Target,
                token,
                context.Contract
            )) {
                return true;
            }

            // inline_list
            if (InlineListEntriesConsumerHandler.Instance.TryHandle(
                this,
                context.Target,
                token,
                context.Contract
            )) {
                return true;
            }

            // value
            if (ValueConsumerHandler.Instance.TryHandle(
                this,
                context.Target,
                token,
                context.Contract
            )) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Register custom handlers at start.
        /// </summary>
        protected virtual void SetupHandlers() {
        }

        /// <summary>
        /// Register a handler to be used when a custom value type is found.
        /// It's verified in a value assign expression, such as `identifier: custom_type = value`
        /// </summary>
        protected void RegisterCustomValueTypeHandler(
            string typeName,
            ValueTypeHandler handler
        ) {
            if (string.IsNullOrWhiteSpace(typeName)) {
                throw new System.ArgumentException($"Invalid typename: '{(typeName ?? "")}'");
            }

            if (handler == null) {
                throw new System.ArgumentNullException(nameof(handler));
            }

            _customValueTypeHandlers.Add(typeName, handler);
        }

        /// <summary>
        /// Register a handler to be used when a custom structure type is found.
        /// A structure is a data type which can hold many expressions.
        /// </summary>
        protected void RegisterCustomStructureTypeHandler(
            string typeName,
            StructureTypeHandler handler
        ) {
            if (string.IsNullOrWhiteSpace(typeName)) {
                throw new System.ArgumentException($"Invalid typename: '{(typeName ?? "")}'");
            }

            if (handler == null) {
                throw new System.ArgumentNullException(nameof(handler));
            }

            _customStructureTypeHandlers.Add(typeName, handler);
        }

        #endregion Protected Methods

        #region HandleResult Struct

        /// <summary>
        /// Describes a handle operation, with it's status and value.
        /// </summary>
        protected struct HandleResult {
            public object Value;
            public HandleStatus Status;

            public HandleResult(object value, HandleStatus status) {
                Value = value;
                Status = status;
            }

            public bool IsFail { get { return Status == HandleStatus.Fail; } }
            public bool IsSuccess { get { return Status == HandleStatus.Success; } }

            public static HandleResult Fail() {
                return new HandleResult(null, HandleStatus.Fail);
            }

            public static HandleResult Success(object value) {
                return new HandleResult(value, HandleStatus.Success);
            }
        }

        #endregion HandleResult Struct
    }
}
