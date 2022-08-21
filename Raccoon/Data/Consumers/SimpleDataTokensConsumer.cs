using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Raccoon.Data.Parsers;

namespace Raccoon.Data {
    public class SimpleDataTokensConsumer : DataTokensConsumer {
        #region Private Members

        private static SimpleDataTokensConsumer _instance;

        private Stack<Context> _stack = new Stack<Context>();

        #endregion Private Members

        #region Constructors

        protected SimpleDataTokensConsumer() {
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

        #region Protected Properties

        protected Context CurrentContext {
            get {
                if (!_stack.TryPeek(out Context context)) {
                    return null;
                }

                return context;
            }
        }

        #endregion Protected Properties

        #region Public Methods

        public override void Consume(
            object rootTarget,
            DataContract rootContract,
            ListToken<Token> root
        ) {
            _stack.Clear();

            PushContext(
                root,
                rootTarget,
                rootContract
            );

            while (_stack.TryPeek(out Context context)) {
                if (!context.Enumerator.MoveNext()) {
                    PopContext();
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

        #endregion Public Methods

        #region Protected Methods

        protected Context PushContext(ListToken<Token> list, object target, DataContract contract) {
            Context context = new Context(
                list.Entries.GetEnumerator(),
                target,
                contract
            );

            _stack.Push(context);
            return context;
        }

        protected Context PushContext(ListToken<Token> list, object target) {
            DataContractAttribute dataContractAttr
                = target.GetType().GetCustomAttribute<DataContractAttribute>(true);

            if (dataContractAttr == null) {
                throw new System.ArgumentException($"Target doesn't have a {nameof(DataContractAttribute)}");
            }

            Context context = new Context(
                list.Entries.GetEnumerator(),
                target,
                new DataContract(target.GetType(), dataContractAttr)
            );

            _stack.Push(context);
            return context;
        }

        protected ArrayContext PushContext(ListToken<Token> list, TypeToken typeToken, System.Array target) {
            ArrayContext context = new ArrayContext(
                list.Entries.GetEnumerator(),
                target,
                typeToken,
                new DataContract(target.GetType(), null)
            );

            _stack.Push(context);
            return context;
        }

        protected Context PopContext() {
            return _stack.Pop();
        }

        protected bool HandleToken(Context context, Token token) {
            // ident: type = value
            if (TryHandleValueAssignToken(context.Target, token, context.Contract)) {
                return true;
            }

            // ident
            //     ...

            // ident: type
            //     ...
            if (TryHandleNamedListToken(context.Target, token, context.Contract)) {
                return true;
            }

            // inline_list
            if (TryHandleInlineListEntriesToken(context.Target, token, context.Contract)) {
                return true;
            }

            // value
            if (TryHandleValueToken(context.Target, token, context.Contract)) {
                return true;
            }

            return false;
        }

        protected virtual bool HandleCustomValueAssignToken(
            IdentifierToken identToken,
            ValueAssignToken valueAssignToken,
            out object value
        ) {
            throw new System.NotImplementedException($"Custom token isn't handled.");
        }

        protected virtual bool HandleCustomNamedListToken(
            IdentifierToken identToken,
            TypeToken typeToken,
            NamedListToken<Token> namedListToken,
            out object value
        ) {
            throw new System.NotImplementedException($"Custom token isn't handled.");
        }

        protected IEnumerable<Context> Contexts() {
            foreach (Context c in _stack) {
                yield return c;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private bool TryHandleValueAssignToken(
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
                    if (!HandleCustomValueAssignToken(valueAssignToken.Identifier, valueAssignToken, out object value)) {
                        throw new System.InvalidOperationException(
                            $"Failed to handle custom value assign (name: '{valueAssignToken.Identifier.Name}', custom type: '{valueAssignToken.Type.Custom}')"
                        );
                    }

                    RegisterValue(valueAssignToken.Identifier, value);
                } else {
                    RegisterValue(valueAssignToken.Identifier, valueAssignToken.Value.AsObject());
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
                    string type = valueAssignToken.Type != null ? (" (" + valueAssignToken.Type + ")") : "";

                    throw new System.InvalidOperationException($"Property, with name '{valueAssignToken.Identifier.Name}'{type}, was not found at target type '{target.GetType().ToString()}'.\nAvailable properties found: {CollectPropertiesNames(contract)}");
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

            System.Type targetType = target.GetType();

            if (namedListToken.Identifier is TypedIdentifierToken typedIdentToken) {
                // it must be handled or an error will be thrown
                bool handled = false;

                switch (typedIdentToken.Type.Type) {
                    case TypeKind.Vector:
                        {
                            if (TryNamedListAsVector(target, typedIdentToken, namedListToken, contract)) {
                                handled = true;
                            }
                        }
                        break;

                    case TypeKind.Custom:
                        {
                            if (!HandleCustomNamedListToken(
                                typedIdentToken,
                                typedIdentToken.Type,
                                namedListToken,
                                out object value
                            )) {
                                throw new System.InvalidOperationException($"Failed to handle custom named list (name: '{namedListToken.Identifier.Name}', custom type: '{typedIdentToken.Type.Custom}')");
                            }

                            // apply custom named list to target's property
                            RegisterValue(namedListToken.Identifier, value);
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
                Context context = CurrentContext;

                if (context is ArrayContext arrayContext) {
                    if (!HandleCustomNamedListToken(
                        namedListToken.Identifier,
                        arrayContext.ElementTypeToken,
                        namedListToken,
                        out object value
                    )) {
                        throw new System.InvalidOperationException(
                            $"Failed to handle custom named list (name: '{namedListToken.Identifier.Name}', type: '{arrayContext.ElementTypeToken}')"
                        );
                    }

                    RegisterValue(namedListToken.Identifier, value);
                    return true;
                }
            }

            if (TrySimpleNamedList(target, namedListToken, contract)) {
                return true;
            }

            return false;
        }

        private bool TryHandleInlineListEntriesToken(
            object target,
            Token token,
            DataContract contract
        ) {
            if (!(token is InlineListEntriesToken inlineListEntriesToken)) {
                return false;
            }

            try {
                foreach (ValueAssignToken valueAssignToken in inlineListEntriesToken.Entries) {
                    TryHandleValueAssignToken(target, valueAssignToken, contract);
                }
            } catch (System.Exception e) {
                throw new System.InvalidOperationException($"Operation failed at {nameof(InlineListEntriesToken)} entry.", e);
            }

            return true;
        }

        #region NamedList

        private bool TrySimpleNamedList(
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
                    throw new System.InvalidOperationException($"Property, with name '{namedListToken.Identifier.Name}', was not found at target type '{target.GetType().ToString()}'.\nAvailable properties found: {CollectPropertiesNames(contract)}");
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

            PushContext(
                namedListToken,
                subTarget,
                property.SubDataContract
            );

            return true;
        }

        private bool TryNamedListAsVector(
            object target,
            TypedIdentifierToken typedIdentToken,
            ListToken<Token> listToken,
            DataContract contract
        ) {
            DataContract.Property property = contract.Find(typedIdentToken);

            if (property == null) {
                if (contract.FailOnNotFound) {
                    throw new System.InvalidOperationException(
                        $"Property, with name '{typedIdentToken.Name}', was not found at target type '{target.GetType().ToString()}'.\nAvailable properties found: {CollectPropertiesNames(contract)}"
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

                    ApplyValue(
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

                PushContext(listToken, typedIdentToken.Type, arr);
            } else if (vectorType.IsConstructedGenericType) {
                System.Type genericType = vectorType.GetGenericTypeDefinition();

                if (genericType == typeof(List<>)) {
                    IList list;

                    if (vectorTarget == null) {
                        list = (IList) System.Activator.CreateInstance(vectorType);

                        ApplyValue(
                            contract,
                            target,
                            typedIdentToken,
                            new DefinedValueToken<IList>(list)
                        );
                    } else {
                        list = (IList) vectorTarget;
                    }

                    PushContext(listToken, list, null);
                } else {
                    throw new System.NotImplementedException($"Generic type '{genericType}' isn't handled as a vector.");
                }
            } else {
                return false;
            }

            return true;
        }

        #endregion NamedList

        private bool TryHandleValueToken(
            object target,
            Token token,
            DataContract contract
        ) {
            if (!(token is ValueToken valueToken)) {
                return false;
            }

            Context currentContext = CurrentContext;

            if (currentContext == null) {
                throw new System.InvalidOperationException($"There is no context available. When handling a {nameof(ValueToken)}.");
            }

            RegisterValue(null, valueToken.AsObject());
            return true;
        }

        /// <summary>
        /// Register value to provided target ICollection (it must be initialized already).
        /// </summary>
        private bool RegisterValue(object target, IdentifierToken identifier, object value) {
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
        private bool RegisterValue(IdentifierToken identifier, object value) {
            Context context = CurrentContext;

            if (context == null) {
                throw new System.InvalidOperationException("Can't register value. There is no context available.");
            }

            if (context is ArrayContext arrContext) {
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
        private bool ApplyValue(DataContract contract, object target, IdentifierToken identifier, ValueToken value) {
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
                    throw new System.InvalidOperationException($"Property, with name '{identifier.Name}', was not found at target type '{target.GetType().ToString()}'.\nAvailable properties found: {CollectPropertiesNames(contract)}");
                }

                return false;
            }

            property.SetValue(target, value);
            return true;
        }

        /// <summary>
        /// Apply a value to current context target's property, using it's identifier.
        /// </summary>
        private bool ApplyValue(IdentifierToken identifier, ValueToken value) {
            Context context = CurrentContext;

            if (context == null) {
                throw new System.InvalidOperationException("Can't apply value. There is no context available.");
            }

            return ApplyValue(context.Contract, context.Target, identifier, value);
        }

        private string CollectPropertiesNames(DataContract contract) {
            string propertiesNames = "";

            foreach (DataContract.Property p in contract.Properties) {
                propertiesNames += $"'{p.Info.Name}' ({p.Info?.PropertyType.ToString() ?? "undefined type"}); ";
            }

            return propertiesNames;
        }

        #endregion Private Methods

        #region Context Class

        protected class Context {
            public Context(
                IEnumerator<Token> enumerator,
                object target,
                DataContract contract
            ) {
                Enumerator = enumerator;
                Target = target;
                Contract = contract;
            }

            public IEnumerator<Token> Enumerator { get; }
            public object Target { get; }
            public DataContract Contract { get; }
        }

        protected class ArrayContext : Context {
            public ArrayContext(
                IEnumerator<Token> enumerator,
                System.Array target,
                TypeToken typeToken,
                DataContract contract
            ) : base(enumerator, target, contract)
            {
                System.Type arrayType = target.GetType();
                ElementType = arrayType.GetElementType();
                TypeToken = typeToken;
                ElementTypeToken = TypeToken.Nested[0];
            }

            public TypeToken TypeToken { get; }
            public TypeToken ElementTypeToken { get; }
            public System.Type ElementType { get; }

            /// <summary>
            /// Which index current value should apply to.
            /// </summary>
            public int TargetIndex { get; private set; }

            public void AdvanceIndex() {
                TargetIndex += 1;
            }
        }

        #endregion Context Class
    }
}
