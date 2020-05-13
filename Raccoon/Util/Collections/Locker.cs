﻿using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util.Collections {
    public class Locker<T> : ICollection<T> {
        #region Private Members

        private List<T> _toAdd = new List<T>(),
                        _toRemove = new List<T>(),
                        _items = new List<T>();

        private System.Comparison<T> _sortComparer;
        private int _locks = 0;

        #endregion Private Members

        #region Constructors

        public Locker() { 
            ToAdd = new ReadOnlyList<T>(_toAdd);
            ToRemove = new ReadOnlyList<T>(_toRemove);
            Items = new ReadOnlyList<T>(_items);
        }

        public Locker(System.Comparison<T> comparer) : this() {
            _sortComparer = comparer;
        }

        #endregion Constructors

        #region Public Properties

        public bool IsLocked { get { return _locks > 0; } }
        public int Count { get { return _items.Count + _toAdd.Count  - _toRemove.Count; } }
        public ReadOnlyList<T> ToAdd { get; private set; }
        public ReadOnlyList<T> ToRemove { get; private set; }
        public ReadOnlyList<T> Items { get; private set; }
        public bool IsReadOnly { get { return false; } }

        public T this[int i] {
            get {
                return _items[i];
            }

            set {
                if (IsLocked) {
                    Remove(_items[i]);
                    Add(value);
                    return;
                }

                _items[i] = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Lock() {
            _locks += 1;
        }

        public void Unlock() {
            if (_locks == 0) {
                return;
            }

            _locks -= 1;
            if (_locks > 0) {
                return;
            }

            Upkeep();
        }

        public void Add(T item) {
            if (IsLocked) {
                _toAdd.Add(item);
                return;
            }

            AddItem(item);
            Sort();
        }

        public void AddRange(IEnumerable<T> items) {
            if (IsLocked) {
                _toAdd.AddRange(items);
                return;
            }

            foreach (T item in items) {
                AddItem(item);
            }

            Sort();
        }

        public bool Remove(T item) {
            if (IsLocked) {
                if (_toRemove.Contains(item)) {
                    return false;
                }

                if (_toAdd.Remove(item) || _items.Contains(item)) {
                    _toRemove.Add(item);
                    return true;
                }

                return false;
            }

            return RemoveItem(item);
        }

        public List<T> RemoveRange(IEnumerable<T> items) {
            List<T> removed = new List<T>();

            if (IsLocked) {
                foreach (T item in items) {
                    if (!_toRemove.Contains(item) && (_toAdd.Contains(item) || _items.Contains(item))) {
                        _toRemove.Add(item);
                        removed.Add(item);
                    }
                }

                return removed;
            }

            foreach (T item in items) {
                RemoveItem(item);
            }

            return removed;
        }

        public List<T> RemoveWhere(System.Predicate<T> match) {
            List<T> removed = new List<T>();

            foreach (T item in _items) {
                if (match(item) && !_toRemove.Contains(item)) {
                    _toRemove.Add(item);
                    removed.Add(item);
                }
            }

            if (IsLocked) {
                foreach (T item in _toAdd) {
                    if (match(item) && !_toRemove.Contains(item)) {
                        _toRemove.Add(item);
                        removed.Add(item);
                    }
                }

                return removed;
            }

            foreach (T item in _toRemove) {
                RemoveItem(item);
            }

            return removed;
        }

        public void Clear() {
            if (IsLocked) {
                _toRemove.AddRange(_items);
                _toRemove.AddRange(_toAdd);
                return;
            }

            _toRemove.AddRange(_items);
            foreach (T item in _toRemove) {
                RemoveItem(item);
            }

            _toRemove.Clear();
        }

        public bool Contains(T item) {
            if (IsLocked) {
                return !_toRemove.Contains(item) && (_items.Contains(item) || _toAdd.Contains(item));
            }

            return _items.Contains(item);
        }

        public T Find(System.Predicate<T> match) {
            foreach (T item in _items) {
                if (match(item)) {
                    return item;
                }
            }

            if (IsLocked) {
                foreach (T item in _toAdd) {
                    if (match(item)) {
                        return item;
                    }
                }
            }

            return default(T);
        }

        public IEnumerator<T> GetEnumerator() {
            using (IEnumerator<T> enumerator = _items.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    yield return enumerator.Current;
                }
            }
        }

        public IEnumerable<T> ReverseIterator() {
            for (int i = _items.Count - 1; i >= 0; i--) {
                yield return _items[i];
            }
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < _items.Count; i++) {
                array[arrayIndex + i] = _items[i];
            }
        }

        public override string ToString() {
            return $"Count: {Count} [A: {_toAdd.Count} R: {_toRemove.Count}]";
        }

        #endregion Public Methods

        #region Private Methods

        private void Upkeep() {
            bool modified = false;

            if (_toAdd.Count > 0) {
                modified = true;
                foreach (T item in _toAdd) {
                    AddItem(item);
                }

                _toAdd.Clear();
            }

            if (_toRemove.Count > 0) {
                modified = true;
                foreach (T item in _toRemove) {
                    RemoveItem(item);
                }

                _toRemove.Clear();
            }

            if (modified) {
                Sort();
            }
        }

        private void AddItem(T item) {
            _items.Add(item);
        }

        private bool RemoveItem(T item) {
            return _items.Remove(item);
        }

        private void Sort() {
            if (_sortComparer == null) {
                return;
            }

            _items.Sort(_sortComparer);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion Private Methods
    }
}
