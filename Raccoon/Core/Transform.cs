using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public sealed class Transform : IEnumerable<Transform>, IEnumerable {
        #region Private Members

        private List<Transform> _children = new List<Transform>();
        private Transform _parent;

        #endregion Private Members

        #region Constructors

        internal Transform(Entity entity) {
            Entity = entity;
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
        public bool IsHandledByParent { get; internal set; }
        public bool IsDetached { get; private set; }

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
                    if (_parent.RemoveChild(this)) {
                        OnParentRemoved();
                    }
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
                if (child.IsDetached) {
                    continue;
                }

                child.OnParentRemoved();
                child._parent = null;
            }

            _children.Clear();
        }

        public void EntitySceneAdded(Scene scene) {
            foreach (Transform child in _children) {
                if (!child.IsHandledByParent) {
                    continue;
                }

                child.Entity.SceneAdded(scene);
            }
        }

        public void EntitySceneRemoved(bool wipe) {
            if (Parent != null && !IsHandledByParent) {
                if (wipe) {
                    // manually detach from parent to avoid
                    // another call to Entity.SceneRemoved
                    _parent.RemoveChild(this);
                    LocalPosition += _parent.Position;
                    _parent = null;
                } else {
                    // return to parent
                    IsHandledByParent = true;
                }
            }

            foreach (Transform child in _children) {
                if (wipe) {
                    if (child.IsDetached) {
                        continue;
                    }

                    child.OnParentRemoved();
                    child._parent = null;
                }

                if (child.IsHandledByParent) {
                    child.Entity.SceneRemoved(wipe);
                } else {
                    child.Entity.Scene.RemoveEntity(child.Entity, wipe);
                }
            }
        }

        public override string ToString() {
            return $"{Position}  Rot: {Rotation}  Origin: {Origin}  Parent? {_parent != null}  Child Count: {_children.Count}";
        }

        public IEnumerator<Transform> GetEnumerator() {
            int count = _children.Count;
            for (int i = 0; i < count; i++) {
                yield return _children[i];
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void OnParentAdded() {
            LocalPosition -= Parent.Position;

            if (Entity.Scene == null) {
                IsHandledByParent = true;

                // virtually add transform to ancestor scene
                Scene ancestorScene = FindFirstAncestorScene();
                if (ancestorScene != null) {
                    Entity.SceneAdded(ancestorScene);
                }
            }
        }

        private void OnParentRemoved() {
            LocalPosition += Parent.Position;

            if (IsHandledByParent) {
                // virtually remove transform from ancestor scene
                if (FindFirstAncestorScene() != null) {
                    Entity.SceneRemoved(allowWipe: true);
                }

                IsHandledByParent = false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private bool RemoveChild(Transform child) {
            if (IsDetached) {
                return false;
            }

            return _children.Remove(child);
        }

        private Scene FindFirstAncestorScene() {
            Transform ancestor = Parent;

            while (ancestor != null && ancestor.Entity != null) {
                if (ancestor.Entity.Scene != null) {
                    return ancestor.Entity.Scene;
                }

                ancestor = ancestor.Parent;
            }

            return null;
        }

        #endregion Private Methods

        #region Internal Methods

        internal void Detach() {
            if (IsDetached) {
                return;
            }

            ClearChildren();

            _parent = null;

            Entity = null;
            IsDetached = true;
        }

        #endregion Internal Methods
    }
}
