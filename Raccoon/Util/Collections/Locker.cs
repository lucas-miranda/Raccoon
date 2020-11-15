using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util.Collections {
    public class Locker<T> : ICollection<T> {
        #region Private Members

        private List<T> _toAdd      = new List<T>(),
                        _toRemove   = new List<T>(),
                        _items      = new List<T>();

        private System.Comparison<T> _sortComparer;
        private int _locks = 0;

        #endregion Private Members

        #region Constructors

        public Locker() { 
            ToAdd = new ReadOnlyList<T>(_toAdd);
            ToRemove = new ReadOnlyList<T>(_toRemove);
            Items = new ReadOnlyList<T>(_items);
        }

        public Locker(System.Comparison<T> sorter) : this() {
            _sortComparer = sorter;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// If it's locked.
        /// </summary>
        public bool IsLocked { get { return _locks > 0; } }

        /// <summary>
        /// Items count.
        /// Items marked to be add or to be removed are counted as well.
        /// Count is as follow: items + items_to_add - items_to_remove
        /// </summary>
        public int Count { get { return _items.Count + _toAdd.Count  - _toRemove.Count; } }

        /// <summary>
        /// An read only wrapper to access items to be added.
        /// </summary>
        public ReadOnlyList<T> ToAdd { get; private set; }

        /// <summary>
        /// An read only wrapper to access items to be removed.
        /// </summary>
        public ReadOnlyList<T> ToRemove { get; private set; }

        /// <summary>
        /// An read only wrapper to access current inserted items.
        /// </summary>
        public ReadOnlyList<T> Items { get; private set; }

        /// <summary>
        /// Locker itself isn't read only.
        /// </summary>
        public bool IsReadOnly { get { return false; } }

#if DEBUG
        /// <summary>
        /// Helps to track unbalanced locks by throwing an exception when it reaches a maximum amount.
        ///
        /// Unbalanced locking happens when calling Lock(), before any given operation, and for a reason, it's respective Unlock() is never called.
        /// Causing an increasing amount of locks that are never unlocked and Upkeep() never happens.
        ///
        /// This verification only happens when DEBUG is active.
        /// </summary>
        public bool UnbalancedLockWarning { get; set; } = true;

        /// <summary>
        /// Max allowed locks before throwing an expection.
        /// Only happens if UnbalancedLockWarning is enabled.
        /// </summary>
        public int UnbalancedMaxLocks { get; set; } = 5;
#endif

        public T this[int i] {
            get {
                return _items[i];
            }

            set {
                if (IsLocked) {
                    throw new System.InvalidOperationException($"Set element by index isn't available when locked.");

                }

                _items[i] = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Locks it, making it effectively safe to modify while iterating through items, even at multiple nested iterations.
        /// Modifications will be recorded and applied when Upkeep() is called.
        /// </summary>
        public void Lock() {
            _locks += 1;
        }

        /// <summary>
        /// Unlocks a lock. Each Lock() call should has it's Unlock() call as well.
        /// When all locks are unlocked, it calls Upkeep() to apply modifications recorded previously.
        /// Does nothing if calling it when there is no locks to unlock.
        /// </summary>
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

        /// <summary>
        /// Adds an item to this collection.
        ///
        /// When unlocked:
        ///     Add and do sorting, if exists.
        ///
        /// When locked:
        ///     Register item to be added later on.
        /// </summary>
        /// <param name="item">An item to be added.</param>
        public void Add(T item) {
            if (IsLocked) {
                _toAdd.Add(item);
                return;
            }

            AddItem(item);
            Sort();
        }

        /// <summary>
        /// Adds a range of item to this collection.
        ///
        /// When unlocked:
        ///     Add every item and then do sorting, if exists.
        ///
        /// When locked:
        ///     Register all items to be added later on.
        /// </summary>
        /// <param name="items">Items to be added.</param>
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

        /// <summary>
        /// Removes a item from collection.
        /// </summary>
        /// <param name="item">An item to be removed.</param>
        /// <returns>True, if item was removed. False, if it's not.</returns>
        public bool Remove(T item) {
            if (IsLocked) {
                if (_toRemove.Contains(item)) {
                    return false;
                }

                bool wasRemoved = _toAdd.Remove(item);
                if (_items.Contains(item)) {
                    _toRemove.Add(item);
                    return true;
                }

                return wasRemoved;
            }

            return RemoveItem(item);
        }

        /// <summary>
        /// Removes multiple items.
        /// </summary>
        /// <param name="items">Items to be removed from Locker.</param>
        /// <returns>A list with every removed item.</returns>
        public List<T> RemoveRange(IEnumerable<T> items) {
            List<T> removed = new List<T>();

            if (IsLocked) {
                foreach (T item in items) {
                    if (_toRemove.Contains(item)) {
                        continue;
                    }

                    if (_toAdd.Remove(item)) {
                        removed.Add(item);
                    } else if (_items.Contains(item)) {
                        _toRemove.Add(item);
                        removed.Add(item);
                    }
                }

                return removed;
            }

            foreach (T item in items) {
                if (RemoveItem(item)) {
                    removed.Add(item);
                }
            }

            return removed;
        }

        /// <summary>
        /// Removes every item which meets a criteria.
        /// </summary>
        /// <param name="match">A predicate to evaluate if an item should be removed.</param>
        /// <returns>A list with every removed item.</returns>
        public List<T> RemoveWhere(System.Predicate<T> match) {
            if (match == null) {
                throw new System.ArgumentNullException(nameof(match));
            }

            List<T> removed = new List<T>();

            if (IsLocked) {
                List<T> remainingToRemove = new List<T>(_toRemove);

                for (int i = 0; i < _toAdd.Count; i++) {
                    T item = _toAdd[i];

                    if (!match(item) || remainingToRemove.Remove(item)) {
                        continue;
                    }

                    removed.Add(item);
                    _toAdd.RemoveAt(i);
                    i -= 1;
                }

                foreach (T item in _items) {
                    if (!match(item) || remainingToRemove.Remove(item)) {
                        continue;
                    }

                    _toRemove.Add(item);
                    removed.Add(item);
                }

                return removed;
            }

            for (int i = 0; i < _items.Count; i++) {
                T item = _items[i];
                if (!match(item)) {
                    continue;
                }

                removed.Add(item);
                _items.RemoveAt(i);
                i -= 1;
            }

            return removed;
        }

        /// <summary>
        /// Removes every item.
        /// </summary>
        public void Clear() {
            if (IsLocked) {
                _toAdd.Clear();
                _toRemove.AddRange(_items);
                return;
            }

            _toAdd.Clear();
            _items.Clear();
            _toRemove.Clear();
        }

        /// <summary>
        /// Checks if contains a given item.
        /// </summary>
        /// <param name="item">Item to check if Locker contains.</param>
        public bool Contains(T item) {
            if (IsLocked) {
                return !_toRemove.Contains(item) && (_items.Contains(item) || _toAdd.Contains(item));
            }

            return _items.Contains(item);
        }

        public T Find(System.Predicate<T> match) {
            if (IsLocked) {
                List<T> remainingToRemove = new List<T>(_toRemove);

                foreach (T item in _toAdd) {
                    if (match(item) && !remainingToRemove.Remove(item)) {
                        return item;
                    }
                }

                foreach (T item in _items) {
                    if (match(item) && !remainingToRemove.Remove(item)) {
                        return item;
                    }
                }

                return default(T);
            }

            foreach (T item in _items) {
                if (match(item)) {
                    return item;
                }
            }

            return default(T);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < _items.Count; i++) {
                array[arrayIndex + i] = _items[i];
            }
        }

        public IEnumerator<T> GetEnumerator() {
            using (IEnumerator<T> enumerator = _items.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    yield return enumerator.Current;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerable<T> ReverseIterator() {
            for (int i = _items.Count - 1; i >= 0; i--) {
                yield return _items[i];
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

        #endregion Private Methods
    }
}
