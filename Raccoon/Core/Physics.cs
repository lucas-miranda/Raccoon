using Raccoon.Components;
using System.Collections.Generic;

namespace Raccoon {
    public class Physics {
        private static readonly Physics _instance = new Physics();
        private Dictionary<string, List<ColliderComponent>> _colliders;

        private Physics() {
            _colliders = new Dictionary<string, List<ColliderComponent>>();
        }

        public static Physics Instance { get { return _instance; } }

        public void RegisterTag(string tagName) {
            if (HasTag(tagName)) {
                return;
            }

            _colliders.Add(tagName, new List<ColliderComponent>());
        }

        public bool HasTag(string tagName) {
            return _colliders.ContainsKey(tagName);
        }

        public void AddCollider(string tagName, ColliderComponent collider) {
            RegisterTag(tagName);
            if (_colliders[tagName].Contains(collider)) {
                return;
            }

            _colliders[tagName].Add(collider);
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

                    default:
                        break;
                }
            }

            return false;
        }
    }
}
