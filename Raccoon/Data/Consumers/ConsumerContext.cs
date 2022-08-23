using System.Collections.Generic;
using System.Reflection;

using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public class ConsumerContext {
        #region Private Members

        private Stack<Entry> _stack = new Stack<Entry>();

        #endregion Private Members

        #region Constructors

        public ConsumerContext() {
        }

        #endregion Constructors

        #region Public Properties

        public Entry Current {
            get {
                if (!_stack.TryPeek(out Entry context)) {
                    return null;
                }

                return context;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public Entry Push(ListToken<Token> list, object target, DataContract contract) {
            Entry context = new Entry(
                list.Entries.GetEnumerator(),
                target,
                contract
            );

            _stack.Push(context);
            return context;
        }

        public Entry Push(ListToken<Token> list, object target) {
            DataContractAttribute dataContractAttr
                = target.GetType().GetCustomAttribute<DataContractAttribute>(true);

            if (dataContractAttr == null) {
                throw new System.ArgumentException($"Target doesn't have a {nameof(DataContractAttribute)}");
            }

            Entry context = new Entry(
                list.Entries.GetEnumerator(),
                target,
                new DataContract(target.GetType(), dataContractAttr)
            );

            _stack.Push(context);
            return context;
        }

        public ArrayEntry PushArray(ListToken<Token> list, TypeToken typeToken, System.Array target) {
            ArrayEntry context = new ArrayEntry(
                list.Entries.GetEnumerator(),
                target,
                typeToken,
                new DataContract(target.GetType(), null)
            );

            _stack.Push(context);
            return context;
        }

        public bool Peek(out Entry context) {
            return _stack.TryPeek(out context);
        }

        public Entry Pop() {
            return _stack.Pop();
        }

        public IEnumerable<Entry> Entries() {
            foreach (Entry entry in _stack) {
                yield return entry;
            }
        }

        public void Clear() {
            _stack.Clear();
        }

        #endregion Public Methods

        #region Entry Class

        public class Entry {
            public Entry(
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

        #endregion Entry Class

        #region ArrayEntry

        public class ArrayEntry : Entry {
            public ArrayEntry(
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

        #endregion ArrayEntry
    }
}
