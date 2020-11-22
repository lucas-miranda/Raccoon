using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Util.Collections {
    public class Locker<T> : ICollection<T>, IList<T> {
        #region Private Members

        private List<ItemBox> _toAdd      = new List<ItemBox>(),
                              _items      = new List<ItemBox>();

        private System.Comparison<ItemBox> _sortComparer;
        private int _locks, _itemCount;

        #endregion Private Members

        #region Constructors

        public Locker() { 
            /*
            ToAdd = new ReadOnlyList<T>(_toAdd);
            ToRemove = new ReadOnlyList<T>(_toRemove);
            Items = new ReadOnlyList<T>(_items);
            */
        }

        public Locker(System.Comparison<T> sorter) : this() {
            _sortComparer = (ItemBox a, ItemBox b) => sorter.Invoke(a.Item, b.Item);
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// If it's locked.
        /// </summary>
        public bool IsLocked { get { return _locks > 0; } }

        /// <summary>
        /// Current items count.
        /// All items marked to be removed don't belongs to this count.
        /// But items to be added, yes.
        /// </summary>
        public int Count { get { return _itemCount; } }

        /*
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
        */

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
                if (IsLocked) {
                    throw new System.NotSupportedException($"Get element by index isn't available when locked.");
                }

                return _items[i].Item;
            }

            set {
                if (IsLocked) {
                    throw new System.NotSupportedException($"Set element by index isn't available when locked.");
                }

                _items[i].Item = value;
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
                _toAdd.Add(new ItemBox(item));
                _itemCount += 1;
                return;
            }

            AddItemBox(new ItemBox(item));
            _itemCount += 1;
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
                foreach (T item in items) {
                    _toAdd.Add(new ItemBox(item));
                    _itemCount += 1;
                }
            } else {
                foreach (T item in items) {
                    AddItemBox(new ItemBox(item));
                    _itemCount += 1;
                }

                Sort();
            }
        }

        public void Insert(int index, T item) {
            if (IsLocked) {
                throw new System.NotSupportedException($"Insert element at index isn't available when locked.");
            }

            if (index < 0) {
                throw new System.ArgumentException("Index can't be negative.");
            } else if (index > _items.Count) {
                throw new System.ArgumentException($"Index {index} is out of valid range [0, {_items.Count}]");
            }

            _items.Insert(index, new ItemBox(item));
            _itemCount += 1;
            Sort();
        }

        /// <summary>
        /// Removes a item from collection.
        /// </summary>
        /// <param name="item">An item to be removed.</param>
        /// <returns>True, if item was removed. False, if it's not.</returns>
        public bool Remove(T item) {
            if (IsLocked) {
                foreach (ItemBox box in _items) {
                    if (!box.MarkedToRemove && box.Item.Equals(item)) {
                        box.MarkedToRemove = true;
                        _itemCount -= 1;
                        return true;
                    }
                }

                for (int i = 0; i < _toAdd.Count; i++) {
                    ItemBox box = _toAdd[i];
                    if (!box.MarkedToRemove && box.Item.Equals(item)) {
                        box.MarkedToRemove = true;
                        _toAdd.RemoveAt(i);
                        _itemCount -= 1;
                        return true;
                    }
                }

                return false;
            } else {
                for (int i = 0; i < _items.Count; i++) {
                    ItemBox box = _items[i];
                    if (box.Item.Equals(item)) {
                        box.MarkedToRemove = true;
                        _items.RemoveAt(i);
                        _itemCount -= 1;
                        return true;
                    }
                }
            }

            return false;
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
                    foreach (ItemBox box in _items) {
                        if (!box.MarkedToRemove && box.Item.Equals(item)) {
                            box.MarkedToRemove = true;
                            removed.Add(item);
                            _itemCount -= 1;
                        }
                    }

                    for (int i = 0; i < _toAdd.Count; i++) {
                        ItemBox box = _toAdd[i];
                        if (!box.MarkedToRemove && box.Item.Equals(item)) {
                            box.MarkedToRemove = true;
                            _toAdd.RemoveAt(i);
                            removed.Add(item);
                            _itemCount -= 1;
                        }
                    }
                }
            } else {
                foreach (T item in items) {
                    for (int i = 0; i < _items.Count; i++) {
                        ItemBox box = _items[i];
                        if (box.Item.Equals(item)) {
                            box.MarkedToRemove = true;
                            _items.RemoveAt(i);
                            removed.Add(item);
                            i -= 1;
                            _itemCount -= 1;
                        }
                    }
                }
            }

            return removed;
        }

        public void RemoveAt(int index) {
            if (index < 0) {
                throw new System.ArgumentException("Index can't be negative.");
            }

            if (IsLocked) {
                if (index >= _items.Count + _toAdd.Count) {
                    throw new System.ArgumentException($"Index {index} is out of valid range [0, {_items.Count + _toAdd.Count - 1}]");
                }

                if (index < _items.Count) {
                    _items[index].MarkedToRemove = true;
                } else if (index - _items.Count < _toAdd.Count) {
                    _items[index - _items.Count].MarkedToRemove = true;
                }
            } else {
                if (index >= _items.Count) {
                    throw new System.ArgumentException($"Index {index} is out of valid range [0, {_items.Count - 1}]");
                }

                _items[index].MarkedToRemove = true;
            }
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
                foreach (ItemBox box in _items) {
                    if (!box.MarkedToRemove && match(box.Item)) {
                        box.MarkedToRemove = true;
                        removed.Add(box.Item);
                        _itemCount -= 1;
                    }
                }

                for (int i = 0; i < _toAdd.Count; i++) {
                    ItemBox box = _toAdd[i];
                    if (!box.MarkedToRemove && match(box.Item)) {
                        box.MarkedToRemove = true;
                        _toAdd.RemoveAt(i);
                        removed.Add(box.Item);
                        _itemCount -= 1;
                    }
                }

                return removed;
            } else {
                for (int i = 0; i < _items.Count; i++) {
                    ItemBox box = _items[i];
                    if (!box.MarkedToRemove && match(box.Item)) {
                        box.MarkedToRemove = true;
                        _items.RemoveAt(i);
                        removed.Add(box.Item);
                        i -= 1;
                        _itemCount -= 1;
                    }
                }
            }

            return removed;
        }

        public int IndexOf(T item) {
            if (IsLocked) {
                int index = 0;

                for (int i = 0; i < _items.Count; i++) {
                    ItemBox box = _items[i];

                    if (box.MarkedToRemove) {
                        continue;
                    }

                    if (box.Item.Equals(item)) {
                        return index;
                    }

                    index += 1;
                }

                for (int i = 0; i < _toAdd.Count; i++) {
                    ItemBox box = _toAdd[i];

                    if (box.MarkedToRemove) {
                        continue;
                    }

                    if (box.Item.Equals(item)) {
                        return index;
                    }

                    index += 1;
                }
            } else {
                for (int i = 0; i < _items.Count; i++) {
                    ItemBox box = _items[i];

                    if (box.Item.Equals(item)) {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes every item.
        /// </summary>
        public void Clear() {
            if (IsLocked) {
                _toAdd.Clear();

                foreach (ItemBox box in _items) {
                    box.MarkedToRemove = true;
                }

                _itemCount = 0;
            } else {
                _toAdd.Clear();
                _items.Clear();
                _itemCount = 0;
            }
        }

        /// <summary>
        /// Checks if contains a given item.
        /// </summary>
        /// <param name="item">Item to check if Locker contains.</param>
        public bool Contains(T item) {
            if (IsLocked) {
                foreach (ItemBox box in _items) {
                    if (!box.MarkedToRemove && box.Item.Equals(item)) {
                        return true;
                    }
                }

                foreach (ItemBox box in _toAdd) {
                    if (!box.MarkedToRemove && box.Item.Equals(item)) {
                        return true;
                    }
                }
            } else {
                foreach (ItemBox box in _items) {
                    if (box.Item.Equals(item)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public T Find(System.Predicate<T> match) {
            if (IsLocked) {
                foreach (ItemBox box in _items) {
                    if (!box.MarkedToRemove && match(box.Item)) {
                        return box.Item;
                    }
                }

                foreach (ItemBox box in _toAdd) {
                    if (!box.MarkedToRemove && match(box.Item)) {
                        return box.Item;
                    }
                }
            } else {
                foreach (ItemBox box in _items) {
                    if (match(box.Item)) {
                        return box.Item;
                    }
                }
            }

            return default(T);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            int index = 0;

            for (int i = 0; i < _items.Count; i++) {
                ItemBox box = _items[i];
                if (box.MarkedToRemove) {
                    continue;
                }

                array[arrayIndex + index] = box.Item;
                index += 1;
            }

            for (int i = 0; i < _toAdd.Count; i++) {
                ItemBox box = _toAdd[i];
                if (box.MarkedToRemove) {
                    continue;
                }

                array[arrayIndex + index] = box.Item;
                index += 1;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < _items.Count; i++) {
                ItemBox box = _items[i];

                if (box.MarkedToRemove) {
                    continue;
                }

                yield return box.Item;
            }


            for (int i = 0; i < _toAdd.Count; i++) {
                ItemBox box = _toAdd[i];

                if (box.MarkedToRemove) {
                    continue;
                }

                yield return box.Item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerable<T> ReverseIterator() {
            for (int i = _toAdd.Count - 1; i >= 0; i--) {
                ItemBox box = _toAdd[i];

                if (box.MarkedToRemove) {
                    continue;
                }

                yield return box.Item;
            }

            for (int i = _items.Count - 1; i >= 0; i--) {
                ItemBox box = _items[i];

                if (box.MarkedToRemove) {
                    continue;
                }

                yield return box.Item;
            }
        }

        public override string ToString() {
            return $"Count: {Count} [To Add: {_toAdd.Count}], Locked? {IsLocked.ToPrettyString()}";
        }

        #endregion Public Methods

        #region Private Methods

        private void Upkeep() {
            bool modified = false;

            if (_toAdd.Count > 0) {
                modified = true;
                foreach (ItemBox item in _toAdd) {
                    if (item.MarkedToRemove) {
                        continue;
                    }

                    AddItemBox(item);
                }

                _toAdd.Clear();
            }

            for (int i = 0; i < _items.Count; i++) {
                ItemBox box = _items[i];

                if (box.MarkedToRemove) {
                    modified = true;
                    _items.RemoveAt(i);
                    i -= 1;
                }
            }

            if (modified) {
                Sort();
            }
        }

        private void AddItemBox(ItemBox item) {
            _items.Add(item);
        }

        private bool RemoveItemBox(ItemBox item) {
            return _items.Remove(item);
        }

        private void Sort() {
            if (_sortComparer == null) {
                return;
            }

            _items.Sort(_sortComparer);
        }

        #endregion Private Methods

        #region ItemBox Class

        private class ItemBox {
            public ItemBox(T item) {
                Item = item;
            }

            public T Item;
            public bool MarkedToRemove;
        }

        #endregion ItemBox Class
    }
}
