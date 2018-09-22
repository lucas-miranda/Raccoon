using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public sealed class Transform : IEnumerable<Transform>, IEnumerable {
        #region Private Members

        private List<Transform> _children = new List<Transform>();
        private Transform _parent;

        #endregion Private Members

        #region Public Properties

        public Vector2 Position { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; }
        public int ChildCount { get { return _children.Count; } }

        public Transform Parent {
            get {
                return _parent;
            }

            set {
                if (value == _parent) {
                    return;
                }

                if (_parent != null) {
                    _parent._children.Remove(this);
                }

                _parent = value;
                _parent._children.Add(this);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void ClearChildren() {
            foreach (Transform child in _children) {
                child._parent = null;
            }

            _children.Clear();
        }

        #endregion Public Methods

        #region Private Methods

        public IEnumerator<Transform> GetEnumerator() {
            foreach (Transform child in _children) {
                yield return child;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion Private Methods
    }
}
