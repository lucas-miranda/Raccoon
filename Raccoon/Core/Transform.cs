﻿using System.Collections;
using System.Collections.Generic;

using Raccoon.Util.Collections;

namespace Raccoon {
    public sealed class Transform : IEnumerable<Transform>, IEnumerable {
        #region Private Members

        private Locker<Transform> _children = new Locker<Transform>();

        private Transform _parent;

        #endregion Private Members

        #region Constructors

        internal Transform(Entity entity) {
            Entity = entity;
        }

        #endregion Constructors

        #region Public Properties

        public Entity Entity { get; private set; }
        public Scene Scene { get { return Entity.Scene; } }
        public Vector2 LocalPosition { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public Vector2 Origin { get; set; }
        public Vector2 LocalScale { get; set; } = Vector2.One;
        public float LocalRotation { get; set; }
        public int ChildCount { get { return _children.Count; } }
        public bool IsDetached { get; private set; }

        public Transform Parent {
            get {
                return _parent;
            }

            set {
                if (value == this) {
                    value = null;
                    return;
                } else if (value == _parent) {
                    return;
                }

                if (value != null) {
                    value.AddChild(this, changeScene: true, force: false);
                } else {
                    _parent?.RemoveChild(this, dropFromScene: true, allowWipe: true, force: false);
                }
            }
        }

        public Vector2 Position {
            get {
                return _parent == null ? LocalPosition : ((LocalPosition * _parent.Scale) + _parent.Position);
            }

            set {
                LocalPosition = _parent == null ? value : ((value - _parent.Position) / _parent.Scale);
            }
        }

        public Vector2 Scale {
            get {
                return _parent == null ? LocalScale : (LocalScale * _parent.Scale);
            }

            set {
                LocalScale = _parent == null ? value : (value / _parent.Scale);
            }
        }

        public float Rotation {
            get {
                return _parent == null ? LocalRotation : (LocalRotation + _parent.Rotation);
            }

            set {
                LocalRotation = _parent == null ? value : (value - _parent.Rotation);
            }
        }

        public Transform this[int index] {
            get {
                return _children[index];
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adds a Transform as a child.
        /// A transform can already be a child from another parent, in that case the parent will be properly changed without side-effects (ie. triggering unwanted events from the same scene).
        /// </summary>
        /// <param name="transform">A transform to be added as a new child.</param>
        /// <param name="changeScene">Signalizes that transform.Entity.Scene can be changed to match parent Scene.</param>
        /// <param name="force">Indicates that scene change can be forced if necessary. When adding a child that already belongs to a Scene and the intention is to match the parent Scene.</param>
        public void AddChild(Transform transform, bool changeScene = true, bool force = false) {
            if (transform == this
              || IsDetached || Entity.IsWiped
              || transform.IsDetached || transform.Entity.IsWiped
              || _children.Contains(transform)) {
                return;
            }

            bool needStart = false;

            if (changeScene) {
                if (transform.Parent != null) {
                    if (transform.Entity.IsSceneFromTransformAncestor) {
                        if (transform.Scene != Scene && transform.Scene != null) {
                            transform.Entity.SceneRemoved(allowWipe: false);
                            transform.Entity.IsSceneFromTransformAncestor = false;
                        }
                    } else if (force) {
                        if (transform.Scene != Scene && transform.Scene != null) {
                            transform.Scene.RemoveEntity(transform.Entity, wipe: false);
                        }
                    }

                    transform.OnParentRemoved();

                    transform.Parent._children.Remove(transform);
                    transform.Parent.OnChildRemoved(transform);
                }

                if (transform.Scene == null) {
                    if (Scene != null) {
                        transform.Entity.SceneAdded(Scene);

                        if (Scene.HasStarted && !transform.Entity.HasStarted) {
                            needStart = true;
                        }
                    }

                    transform.Entity.IsSceneFromTransformAncestor = true;
                } else if (force) {
                    if (transform.Scene != Scene) {
                        transform.Scene.RemoveEntity(transform.Entity, wipe: false);

                        if (Scene != null) {
                            transform.Entity.SceneAdded(Scene);

                            if (Scene.HasStarted && !transform.Entity.HasStarted) {
                                needStart = true;
                            }
                        }
                    }

                    transform.Entity.IsSceneFromTransformAncestor = true;
                }
            } else {
                if (transform.Parent != null) {
                    transform.OnParentRemoved();

                    transform.Parent._children.Remove(transform);
                    transform.Parent.OnChildRemoved(transform);
                }

                if (transform.Entity.IsSceneFromTransformAncestor) {
                    transform.Entity.IsSceneFromTransformAncestor = false;
                }
            }

            transform._parent = this;
            transform.OnParentAdded();

            _children.Add(transform);
            OnChildAdded(transform);

            if (needStart) {
                transform.Entity.Start();
            }
        }

        /// <summary>
        /// Removes a child from current Transform.
        /// </summary>
        /// <param name="transform">A transform to be removed from children.</param>
        /// <param name="dropFromScene">If True it allows to drop transform from Scene, it'll be the same as removing it from the Scene itself.</param>
        /// <param name="allowWipe">Just pass along the permission to wipe or not the transform.Entity, if dropFromScene is True.</param>
        /// <param name="force">It allows to force a drop from scene, in cases where parent transform doesn't have permission to modify the child Scene (when transform.Entity.IsSceneFromTransformAncestor is false).</param>
        public bool RemoveChild(Transform transform, bool dropFromScene = true, bool allowWipe = true, bool force = false) {
            if (transform == this
              || IsDetached || Entity.IsWiped
              || transform.IsDetached || transform.Entity.IsWiped) {
                return false;
            }

            if (!(_children.Remove(transform) || transform.Parent == this)) {
                return false;
            }

            transform.OnParentRemoved();
            transform._parent = null;

            if (dropFromScene) {
                if (transform.Entity.IsSceneFromTransformAncestor) {
                    // transform.Scene should be equals Scene (indirectly)
                    if (transform.Scene == Scene) {
                        transform.Entity.SceneRemoved(allowWipe);

                        if (allowWipe && transform.IsDetached) {
                            // doesn't need to go any further, transform.Entity is already wiped and transform detached
                            OnChildRemoved(transform);
                            return true;
                        }
                    }

                    transform.Entity.IsSceneFromTransformAncestor = false;
                } else if (force) {
                    // Transform doesn't hold responsability at transform.Scene
                    // but, because of force = true, it can overpass that right

                    transform.Scene?.RemoveEntity(transform.Entity, allowWipe);
                }
            } else {
                if (transform.Entity.IsSceneFromTransformAncestor) {
                    Scene?.AddEntity(transform.Entity);
                    transform.Entity.IsSceneFromTransformAncestor = false;
                }
            }

            OnChildRemoved(transform);
            return true;
        }

        public void ClearChildren(bool wipe = true) {
            if (IsDetached) {
                return;
            }

            _children.Lock();
            foreach (Transform child in _children) {
                if (child.IsDetached) {
                    continue;
                }

                child.OnParentRemoved();
                child._parent = null;

                if (child.Entity.IsSceneFromTransformAncestor && child.Scene != null) {
                    child.Entity.SceneRemoved(wipe);
                }

                OnChildRemoved(child);
            }
            _children.Unlock();

            _children.Clear();
        }

        public void DropIndependentChildren(bool propagate = false) {
            if (IsDetached) {
                return;
            }

            _children.Lock();
            foreach (Transform child in _children) {
                if (child.IsDetached) {
                    _children.Remove(child);
                } else if (!child.Entity.IsSceneFromTransformAncestor) {
                    child.DropIndependentChildren(propagate);

                    child.OnParentRemoved();
                    child._parent = null;

                    _children.Remove(child);

                    OnChildRemoved(child);
                }
            }
            _children.Unlock();
        }

        public override string ToString() {
            return $"Pos: {Position} (l: {LocalPosition}), Scale: {Scale} (l: {LocalScale}), Rot: {Rotation} (l: {LocalRotation})  Origin: {Origin}  Parent? {_parent != null}  Childs: {_children.Count}";
        }

        public void LockChildren() {
            _children.Lock();
        }

        public void UnlockChildren() {
            _children.Unlock();
        }

        public IEnumerator<Transform> GetEnumerator() {
            foreach (Transform child in _children) {
                if (child.IsDetached) {
                    _children.Remove(child);
                    continue;
                }

                yield return child;
            }
        }

        public static implicit operator Transform(Entity entity) {
            return entity.Transform;
        }

        #endregion Public Methods

        #region Private Methods

        private void OnParentAdded() {
            Entity?.TransformParentAdded();
        }

        private void OnParentRemoved() {
            Entity?.TransformParentRemoved();
        }

        private void OnChildAdded(Transform child) {
            if (Entity == null || child == null || child.Entity == null) {
                return;
            }

            if (child.Entity.ShouldUseTransformParentRenderer) {
                child.Entity.Renderer = Entity.Renderer;
            }

            Entity.TransformChildAdded(child);
        }

        private void OnChildRemoved(Transform child) {
            Entity?.TransformChildRemoved(child);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
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

        internal void EntitySceneAdded(Scene scene) {
            _children.Lock();
            foreach (Transform child in _children) {
                if (child.Entity == null || !child.Entity.IsSceneFromTransformAncestor) {
                    continue;
                }

                child.Entity.SceneAdded(scene);

                if (scene.HasStarted
                 && Entity.HasStarted
                 && !child.Entity.HasStarted
                ) {
                    // start this child!
                    // when scene is added and this transform's entity has started already
                    //
                    // this aids the problem when Entity.Start() is called without a Scene
                    // and an Entity child is added at this phase, causing it to never
                    // be able to Start() when parent is added to a Scene

                    child.Entity.Start();
                }
            }
            _children.Unlock();
        }

        internal void EntitySceneRemoved(bool wipe) {
            if (wipe) {
                _children.Lock();
                foreach (Transform child in _children) {
                    if (child.IsDetached) {
                        continue;
                    }

                    child.OnParentRemoved();
                    child._parent = null;

                    if (child.Entity.IsSceneFromTransformAncestor) {
                        child.Entity.SceneRemoved(allowWipe: true);
                    } else {
                        child.Scene.RemoveEntity(child.Entity, wipe: true);
                    }

                    OnChildRemoved(child);
                }
                _children.Unlock();

                return;
            }

            _children.Lock();
            foreach (Transform child in _children) {
                if (child.IsDetached) {
                    continue;
                }

                if (child.Entity.IsSceneFromTransformAncestor) {
                    child.Entity.SceneRemoved(allowWipe: false);
                }
            }
            _children.Unlock();
        }

        #endregion Internal Methods
    }
}
