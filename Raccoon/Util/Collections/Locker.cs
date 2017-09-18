using System;
using System.Collections.Generic;

namespace Raccoon.Util.Collections {
    public class Locker<T> {
        public Action<T> OnAdded = delegate { }, OnRemoved = delegate { };

        private List<T> _toAdd = new List<T>(), _toRemove = new List<T>(), _items = new List<T>();
        private IComparer<T> _sortComparer;

        public Locker() { }

        public Locker(IComparer<T> comparer) {
            _sortComparer = comparer;
        }

        public bool IsLocked { get; private set; }
        public int Count { get { return _items.Count + _toAdd.Count - _toRemove.Count; } }

        public T this[int i] {
            get {
                return _items[i];
            }

            set {
                if (IsLocked) {
                    if (i < _items.Count) {
                        Remove(_items[i]);
                    }

                    Add(value);
                    OnAdded(value);
                    return;
                }

                _items[i] = value;
            }
        }

        public void Upkeep() {
            if (_toAdd.Count > 0) {
                foreach (T item in _toAdd) {
                    _items.Add(item);
                    OnAdded(item);
                }

                _toAdd.Clear();
            }

            if (_toRemove.Count > 0) {
                foreach (T item in _toRemove) {
                    if (_items.Remove(item)) {
                        OnRemoved(item);
                    }
                }

                _toRemove.Clear();
            }

            if (_sortComparer != null) {
                _items.Sort(_sortComparer);
            }
        }

        public void Lock() {
            IsLocked = true;
        }

        public void Unlock() {
            IsLocked = false;
        }

        public void Add(T item) {
            if (IsLocked) {
                _toAdd.Add(item);
                return;
            }

            _items.Add(item);
            OnAdded(item);
            if (_sortComparer != null) {
                _items.Sort(_sortComparer);
            }
        }

        public void AddRange(IEnumerable<T> items) {
            if (IsLocked) {
                _toAdd.AddRange(items);
                return;
            }

            foreach (T item in items) {
                _items.Add(item);
                OnAdded(item);
            }

            if (_sortComparer != null) {
                _items.Sort(_sortComparer);
            }
        }

        public bool Remove(T item) {
            if (IsLocked) {
                if (_toAdd.Contains(item)) {
                    _toAdd.Remove(item);
                    return true;
                }

                if (_items.Contains(item)) {
                    _toRemove.Add(item);
                    return true;
                }

                return false;
            }

            if (!_items.Contains(item)) {
                return false;
            }

            _items.Remove(item);
            OnRemoved(item);
            return true;
        }

        public void RemoveRange(IEnumerable<T> items) {
            if (IsLocked) {
                foreach (T item in items) {
                    if (_toAdd.Contains(item)) {
                        _toAdd.Remove(item);
                    } else if (_items.Contains(item)) {
                        _toRemove.Add(item);
                    }
                }

                return;
            }

            foreach (T item in items) {
                if (_items.Remove(item)) {
                    OnRemoved(item);
                }
            }
        }

        public int RemoveWhere(Predicate<T> match) {
            int count = 0;
            foreach (T item in _items) {
                if (match(item)) {
                    _toRemove.Add(item);
                    count++;
                }
            }
            

            if (IsLocked) {
                foreach (T item in _toAdd) {
                    if (match(item)) {
                        _toRemove.Add(item);
                        count++;
                    }
                }

                return count;
            }

            foreach (T item in _toRemove) {
                if (_items.Remove(item)) {
                    OnRemoved(item);
                }
            }

            _toRemove.Clear();
            return count;
        }

        public void Clear() {
            _toRemove.Clear();
            if (IsLocked) {
                _toRemove.AddRange(_items);
                _toRemove.AddRange(_toAdd);
                return;
            }

            foreach (T item in _items) {
                OnRemoved(item);
            }

            _items.Clear();
        }

        public bool Contains(T item) {
            return !_toRemove.Contains(item) && (_items.Contains(item) || _toAdd.Contains(item));
        }

        public T Find(Predicate<T> match) {
            foreach (T item in _items) {
                if (match(item)) {
                    return item;
                }
            }

            foreach (T item in _toAdd) {
                if (match(item)) {
                    return item;
                }
            }

            return default(T);
        }

        public IEnumerator<T> GetEnumerator() {
            Lock();

            using (IEnumerator<T> enumerator = _items.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    yield return enumerator.Current;
                }
            }

            Unlock();
        }

        public override string ToString() {
            return $"Count: {Count} Real: {_items.Count} [A: {_toAdd.Count} R: {_toRemove.Count}]";
        }
    }
}
