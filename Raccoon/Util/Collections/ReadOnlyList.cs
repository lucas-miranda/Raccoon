using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util.Collections {
    public class ReadOnlyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable,
                                   IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T> {
        #region Private Members

        private readonly IList<T> _list;

        #endregion Private Members

        #region Constructors

        public ReadOnlyList(IList<T> list) {
            _list = list;
        }

        #endregion Constructors

        #region Public Properties

        public bool IsReadOnly { get { return true; } }
        public bool IsSynchronized { get { return false; } }
        public bool IsFixedSize { get { return true; } }
        public object SyncRoot { get { return this; } }

        public int Count {
            get {
                return _list.Count;
            }
        }

        public T this[int index] {
            get {
                return _list[index];
            }

            set {
                throw new System.NotSupportedException();
            }
        }

        object IList.this[int index] {
            get {
                return _list[index];
            }

            set {
                throw new System.NotSupportedException();
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Add(T value) {
            throw new System.NotSupportedException();
        }

        public int Add(object value) {
            throw new System.NotSupportedException();
        }

        public void Insert(int index, T value) {
            throw new System.NotSupportedException();
        }

        public void Insert(int index, object value) {
            throw new System.NotSupportedException();
        }

        public bool Remove(T value) {
            throw new System.NotSupportedException();
        }

        public void Remove(object value) {
            throw new System.NotSupportedException();
        }

        public void RemoveAt(int index) {
            throw new System.NotSupportedException();
        }

        public bool Contains(T value) {
            return _list.Contains(value);
        }

        public bool Contains(object value) {
            return _list.Contains((T) value);
        }

        public int IndexOf(T value) {
            return _list.IndexOf(value);
        }

        public int IndexOf(object value) {
            return _list.IndexOf((T) value);
        }

        public void Clear() {
            throw new System.NotSupportedException();
        }

        public void CopyTo(T[] array, int index) {
            _list.CopyTo(array, index);
        }

        public void CopyTo(System.Array array, int index) {
            int i = 0;
            foreach (T item in _list) {
                array.SetValue(item, index + i);
                i += 1;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion Public Methods
    }
}
