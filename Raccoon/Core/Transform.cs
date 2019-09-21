using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public sealed class Transform : IEnumerable<Transform>, IEnumerable, System.IDisposable {
        #region Private Members

        private List<Transform> _children = new List<Transform>();
        private Transform _parent;

        #endregion Private Members

        #region Constructors

        internal Transform(Entity entity) {
            Entity = entity;
        }

        ~Transform() {
            Dispose();
        }

        #endregion Constructors

        #region Public Properties

        public Entity Entity { get; private set; }
        public Vector2 LocalPosition { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; }
        public int ChildCount { get { return _children.Count; } }
        public bool IsHandledByParent { get; private set; }
        public bool IsDisposed { get; private set; }

        public Transform Parent {
            get {
                return _parent;
            }

            set {
                if (value == this) {
                    value = null;
                }

                if (value == _parent) {
                    return;
                }

                if (_parent != null) {
                    if (_parent._children != null) {
                        _parent._children.Remove(this);
                    }

                    OnParentRemoved();
                }

                _parent = value;
                if (_parent != null) {
                    _parent._children.Add(this);
                    OnParentAdded();
                }
            }
        }

        public Vector2 Position {
            get {
                return _parent == null ? LocalPosition : LocalPosition + _parent.Position;
            }

            set {
                LocalPosition = _parent == null ? value : value - _parent.Position;
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

        public override string ToString() {
            return $"{Position}  Rot: {Rotation}  Origin: {Origin}  Parent? {_parent != null}  Child Count: {_children.Count}";
        }

        public IEnumerator<Transform> GetEnumerator() {
            foreach (Transform child in _children) {
                yield return child;
            }
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            _children = null;
            _parent = null;
            Entity = null;

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Private Methods

        private void OnParentAdded() {
            if (Entity.Scene == null) {
                IsHandledByParent = true;
            } else {
                LocalPosition -= Parent.Position;
            }
        }

        private void OnParentRemoved() {
            if (!IsHandledByParent) {
                LocalPosition += Parent.Position;
            }

            IsHandledByParent = false;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion Private Methods
    }
}
