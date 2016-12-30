namespace LD37.DataStructures {
    public class PriorityQueue<T> {
        #region Private Members

        private BinaryHeap<PriorityQueueNode<T>> _heap;

        #endregion Private Members

        #region Constructors

        public PriorityQueue() {
            _heap = new BinaryHeap<PriorityQueueNode<T>>(HeapType.Min);
        }

        #endregion Constructors

        #region Public Properties

        public int Count { get { return _heap.Count; } }
        public bool IsEmpty { get { return Count == 0; } }

        #endregion Public Properties

        #region Public Methods

        public void Insert(T item, int priority) {
            _heap.Insert(new PriorityQueueNode<T>(item, priority));
        }

        public T Pop() {
            return _heap.Extract().Value;
        }

        public T Peek() {
            return _heap.Front.Value;
        }

        #endregion Public Methods
    }

    public class PriorityQueueNode<T> : IBinaryHeapNode {
        #region Constructors

        public PriorityQueueNode(T value, int priority) {
            Value = value;
            Priority = priority;
        }

        #endregion Constructors

        #region Public Properties

        public T Value { get; set; }
        public int Priority { get; set; }

        #endregion Public Properties
    }
}
