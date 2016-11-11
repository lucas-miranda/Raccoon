using Raccoon.Components;
using System;
using System.Collections.Generic;

namespace Raccoon {
    public class Physics {
        #region Private Static Readonly Members

        private static readonly Physics _instance = new Physics();

        #endregion Private Static Readonly Members

        #region Private Members

        private Dictionary<string, List<Collider>> _colliders;

        #endregion Private Members

        #region Constructors

        private Physics() {
            _colliders = new Dictionary<string, List<Collider>>();
        }

        #endregion Constructors

        #region Public Static Properties

        public static Physics Instance { get { return _instance; } }
        public static int MinUpdateInterval { get; set; } = (int) (1 / 60f * 1000);

        #endregion Public Static Properties

        #region Public Methods

        public void RegisterTag(string tag) {
            if (HasTag(tag)) {
                return;
            }

            _colliders.Add(tag, new List<Collider>());
        }

        public void RegisterTag(Enum tag) {
            RegisterTag(tag.ToString());
        }

        public bool HasTag(string tag) {
            return _colliders.ContainsKey(tag);
        }

        public void HasTag(Enum tag) {
            HasTag(tag.ToString());
        }

        public void AddCollider(Collider collider, string tag) {
            RegisterTag(tag);
            if (_colliders[tag].Contains(collider)) {
                return;
            }

            _colliders[tag].Add(collider);
        }

        public void AddCollider(Collider collider, Enum tag) {
            AddCollider(collider, tag.ToString());
        }

        public void RemoveCollider(Collider collider, string tag = "") {
            if (tag == "") {
                foreach (List<Collider> colls in _colliders.Values) {
                    colls.Remove(collider);
                }
            } else if (HasTag(tag)) {
                _colliders[tag].Remove(collider);
            }
        }

        public void RemoveCollider(Collider collider, Enum tag) {
            RemoveCollider(collider, tag.ToString());
        }

        public bool Collides(Vector2 position, Collider collider, string tag) {
            if (!HasTag(tag)) {
                return false;
            }

            foreach (Collider c in _colliders[tag]) {
                if (c != collider && CollisionCheck(collider, position - collider.Origin, c)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Vector2 position, Collider collider, Enum tag) {
            return Collides(position, collider, tag.ToString());
        }

        public bool Collides(int x, int y, Collider collider, string tag) {
            return Collides(new Vector2(x, y), collider, tag);
        }

        public bool Collides(int x, int y, Collider collider, Enum tag) {
            return Collides(new Vector2(x, y), collider, tag.ToString());
        }

        public bool Collides(Collider collider, string tag) {
            return Collides(collider.Position, collider, tag);
        }

        public bool Collides(Collider collider, Enum tag) {
            return Collides(collider.Position, collider, tag.ToString());
        }

        public bool Collides(Vector2 position, Collider collider, IEnumerable<string> tags) {
            foreach (string tag in tags) {
                if (Collides(position, collider, tag)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Vector2 position, Collider collider, IEnumerable<Enum> tags) {
            foreach (Enum tag in tags) {
                if (Collides(position, collider, tag.ToString())) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(int x, int y, Collider collider, IEnumerable<string> tags) {
            return Collides(new Vector2(x, y), collider, tags);
        }

        public bool Collides(int x, int y, Collider collider, IEnumerable<Enum> tags) {
            return Collides(new Vector2(x, y), collider, tags);
        }

        public bool Collides(Collider collider, IEnumerable<string> tags) {
            return Collides(collider.Position, collider, tags);
        }

        public bool Collides(Collider collider, IEnumerable<Enum> tags) {
            return Collides(collider.Position, collider, tags);
        }

        #endregion Public Methods

        #region Private Methods

        private bool CollisionCheck(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            if (colliderA is BoxCollider || colliderB is BoxCollider) {
                BoxCollider boxColl;
                Collider otherColl;
                Vector2 boxPos, otherPos;
                if (colliderA is BoxCollider) {
                    boxColl = colliderA as BoxCollider;
                    boxPos = colliderAPos;
                    otherColl = colliderB;
                    otherPos = colliderBPos;
                } else {
                    boxColl = colliderB as BoxCollider;
                    boxPos = colliderBPos;
                    otherColl = colliderA;
                    otherPos = colliderAPos;
                }

                if (otherColl is BoxCollider) {
                    return new Rectangle(boxPos, boxColl.Size) & new Rectangle(otherPos, otherColl.Size);
                } else if (otherColl is GridCollider) {
                    GridCollider gridColl = otherColl as GridCollider;
                    Rectangle boxRect = new Rectangle(boxPos, boxColl.Size);
                    if (!(boxRect & new Rectangle(otherPos, gridColl.Size))) {
                        return false;
                    }

                    int startColumn = (int) (boxRect.Left - otherPos.X) / (int) gridColl.TileSize.Width, startRow = (int) (boxRect.Top - otherPos.Y) / (int) gridColl.TileSize.Height;
                    int endColumn = (int) (boxRect.Right - otherPos.X - 1) / (int) gridColl.TileSize.Width, endRow = (int) (boxRect.Bottom - otherPos.Y - 1) / (int) gridColl.TileSize.Height;
                    for (int row = startRow; row <= endRow; row++) {
                        for (int column = startColumn; column <= endColumn; column++) {
                            if (gridColl.IsCollidable(column, row)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool CollisionCheck(Collider colliderA, Vector2 colliderAPos, Collider colliderB) {
            return CollisionCheck(colliderA, colliderAPos, colliderB, colliderB.Position);
        }

        private bool CollisionCheck(Collider colliderA, Collider colliderB) {
            return CollisionCheck(colliderA, colliderA.Position, colliderB, colliderB.Position);
        }

        #endregion Private Methods
    }
}
