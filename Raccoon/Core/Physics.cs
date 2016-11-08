using Raccoon.Components;
using System;
using System.Collections.Generic;

namespace Raccoon {
    public class Physics {
        #region Private Static Readonly Members

        private static readonly Physics _instance = new Physics();

        #endregion Private Static Readonly Members

        #region Private Members

        private Dictionary<string, List<ColliderComponent>> _colliders;

        #endregion Private Members

        #region Constructors

        private Physics() {
            _colliders = new Dictionary<string, List<ColliderComponent>>();
        }

        #endregion Constructors

        #region Public Static Properties

        public static Physics Instance { get { return _instance; } }

        #endregion Public Static Properties

        #region Public Methods

        public void RegisterTag(string tagName) {
            if (HasTag(tagName)) {
                return;
            }

            _colliders.Add(tagName, new List<ColliderComponent>());
        }

        public void RegisterTag(Enum tagName) {
            RegisterTag(tagName.ToString());
        }

        public bool HasTag(string tagName) {
            return _colliders.ContainsKey(tagName);
        }

        public void HasTag(Enum tagName) {
            HasTag(tagName.ToString());
        }

        public void AddCollider(ColliderComponent collider, string tagName) {
            RegisterTag(tagName);
            if (_colliders[tagName].Contains(collider)) {
                return;
            }

            _colliders[tagName].Add(collider);
        }

        public void AddCollider(ColliderComponent collider, Enum tagName) {
            AddCollider(collider, tagName.ToString());
        }

        public void RemoveCollider(ColliderComponent collider, string tagName = "") {
            if (tagName == "") {
                foreach (List<ColliderComponent> colls in _colliders.Values) {
                    colls.Remove(collider);
                }
            } else if (HasTag(tagName)) {
                _colliders[tagName].Remove(collider);
            }
        }

        public void RemoveCollider(ColliderComponent collider, Enum tagName) {
            RemoveCollider(collider, tagName.ToString());
        }

        public bool Collides(ColliderComponent collider, string tagName) {
            if (!HasTag(tagName)) {
                return false;
            }

            foreach (ColliderComponent c in _colliders[tagName]) {
                if (c != collider && CollisionCheck(collider, c)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(ColliderComponent collider, Enum tagName) {
            return Collides(collider, tagName.ToString());
        }

        #endregion Public Methods

        #region Private Methods

        private bool CollisionCheck(ColliderComponent colliderA, ColliderComponent colliderB) {
            if (colliderA.Type == ColliderType.Box || colliderB.Type == ColliderType.Box) {
                BoxCollider boxColl;
                ColliderComponent otherColl;
                if (colliderA.Type == ColliderType.Box) {
                    boxColl = colliderA as BoxCollider;
                    otherColl = colliderB;
                } else {
                    boxColl = colliderB as BoxCollider;
                    otherColl = colliderA;
                }

                switch (otherColl.Type) {
                    case ColliderType.Box:
                        return boxColl.Rect & (otherColl as BoxCollider).Rect;

                    case ColliderType.Grid:
                        GridCollider gridColl = otherColl as GridCollider;
                        if (!(boxColl.Rect & gridColl.Rect)) {
                            break;
                        }

                        int startColumn = (int) (boxColl.Left - gridColl.X) / (int) gridColl.TileSize.Width, startRow = (int) (boxColl.Top - gridColl.Y) / (int) gridColl.TileSize.Height;
                        int endColumn = (int) (boxColl.Right - gridColl.X - 1) / (int) gridColl.TileSize.Width, endRow = (int) (boxColl.Bottom - gridColl.Y - 1) / (int) gridColl.TileSize.Height;
                        for (int row = startRow; row <= endRow; row++) {
                            for (int column = startColumn; column <= endColumn; column++) {
                                if (gridColl.IsCollidable(column, row)) {
                                    return true;
                                }
                            }
                        }

                        break;

                    default:
                        break;
                }
            }

            return false;
        }

        #endregion Private Methods
    }
}
