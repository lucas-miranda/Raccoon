using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util.Collections {
    public class ReadOnlyCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable, 
                                         ICollection, IReadOnlyCollection<T> {
        #region Private Members

        private readonly ICollection<T> _collection;

        #endregion Private Members

        #region Constructors

        public ReadOnlyCollection(ICollection<T> collection) {
            _collection = collection;
        }

        #endregion Constructors

        #region Public Properties

        public bool IsReadOnly { get { return true; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return this; } }

        public int Count {
            get {
                return _collection.Count;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Add(T value) {
            throw new System.NotSupportedException();
        }

        public bool Remove(T value) {
            return _collection.Remove(value);
        }

        public bool Contains(T value) {
            return _collection.Contains(value);
        }

        public void Clear() {
            throw new System.NotSupportedException();
        }

        public void CopyTo(T[] array, int index) {
            _collection.CopyTo(array, index);
        }

        public void CopyTo(System.Array array, int index) {
            int i = 0;
            foreach (T item in _collection) {
                array.SetValue(item, index + i);
                i += 1;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _collection.GetEnumerator();
        }

        #endregion Public Methods
    }
}
