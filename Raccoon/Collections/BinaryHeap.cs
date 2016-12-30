using System.Collections.Generic;

namespace LD37.DataStructures {
    public enum HeapType {
        Min,
        Max
    }

    public class BinaryHeap<T> where T : IBinaryHeapNode {
        #region Private Members

        private T[] _heap;
        private IComparer<T> _comparer;

        #endregion Private Members

        #region Constructors

        public BinaryHeap(HeapType type, int startSize = 15) {
            _comparer = type == HeapType.Min ? (IComparer<T>) new Min() : new Max();
            _heap = new T[startSize];
            Count = 0;
        }

        #endregion Constructors

        #region Public Properties

        public int Count { get; private set; }
        public T Front { get { return _heap[1]; } }

        #endregion Public Properties

        #region Public Methods

        public void Insert(T value) {
            Count++;

            // expand heap size if needed
            if (_heap.Length <= Count) {
                Expand();
            }

            // heapify-up
            int pos = Count;
            while (pos > 1 && _comparer.Compare(value, _heap[pos / 2]) < 0) {
                _heap[pos] = _heap[pos / 2];
                pos /= 2;
            }

            _heap[pos] = value;
        }

        public T Extract() {
            // extract root and move last element to root position
            T extracted = _heap[1];
            _heap[1] = _heap[Count];
            Count--;
            HeapifyDown(1);
            return extracted;
        }

        #endregion Public Methods

        #region Private Methods

        private void HeapifyDown(int parent) {
            int left = parent * 2, right = parent * 2 + 1, swapPos = parent;
            if (left <= Count && _comparer.Compare(_heap[left], _heap[swapPos]) < 0) {
                swapPos = left;
            }

            if (right <= Count && _comparer.Compare(_heap[right], _heap[swapPos]) < 0) {
                swapPos = right;
            }

            if (parent != swapPos) {
                T i = _heap[swapPos];
                _heap[swapPos] = _heap[parent];
                _heap[parent] = i;
                HeapifyDown(swapPos);
            }
        }

        private void Expand() {
            T[] _oldHeap = _heap;
            _heap = new T[(_oldHeap.Length * 2 + 1) * 2];
            _oldHeap.CopyTo(_heap, 0);
        }

        #endregion Private Methods

        #region Comparers

        protected class Min : IComparer<T> {
            public int Compare(T element, T parent) {
                if (element.Priority == parent.Priority) {
                    return 0;
                }

                if (element.Priority > parent.Priority) {
                    return 1;
                }

                return -1;
            }
        }

        protected class Max : IComparer<T> {
            public int Compare(T element, T parent) {
                if (element.Priority == parent.Priority) {
                    return 0;
                }

                if (element.Priority < parent.Priority) {
                    return 1;
                }

                return -1;
            }
        }

        #endregion Comparers
    }

    public interface IBinaryHeapNode {
        #region Public Property

        int Priority { get; set; }

        #endregion Public Property
    }
}
