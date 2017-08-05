using System;
using System.Collections.Generic;

using Raccoon.Graphics;

namespace Raccoon.Components {
    public abstract class Collider : Component {
        protected Collider() { }

        protected Collider(params string[] tags) {
            Tags.AddRange(tags);
        }

        protected Collider(params Enum[] tags) {
            foreach (Enum e in tags) {
                Tags.Add(e.ToString());
            }
        }

        public List<string> Tags { get; private set; } = new List<string>();
        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public Size Size { get; protected set; }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public float Top { get { return Y - Origin.Y; } }
        public float Right { get { return X + Width - Origin.X; } }
        public float Bottom { get { return Y + Height - Origin.Y; } }
        public float Left { get { return X - Origin.X; } }
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }
        
        protected Graphic Graphic { get; set; }
        protected Color Color { get; set; } = Color.Red;

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);
            if (Entity.Scene != null && Tags.Count > 0) {
                Physics.Instance.AddCollider(this, Tags);
            }
        }

        public override void OnRemoved() {
            Physics.Instance.RemoveCollider(this, Tags);
        }

        public override void Update(int delta) {
#if DEBUG
            if (Graphic == null) {
                return;
            }

            Graphic.Update(delta);
#endif
        }

        public override void Render() { }

        [System.Diagnostics.Conditional("DEBUG")]
        public void DebugRender(Color color) {
            Color = color;
            DebugRender();
        }

        #region Collides [Single Tag] [Single Output]

        public bool Collides(Vector2 position, string tag) {
            return Physics.Instance.Collides(position, this, tag);
        }

        public bool Collides(Vector2 position, Enum tag) {
            return Collides(position, tag.ToString());
        }

        public bool Collides(Vector2 position, string tag, out Collider collidedCollider) {
            return Physics.Instance.Collides(position, this, tag, out collidedCollider);
        }

        public bool Collides(Vector2 position, Enum tag, out Collider collidedCollider) {
            return Collides(position, tag.ToString(), out collidedCollider);
        }

        public bool Collides<T>(Vector2 position, string tag, out T collidedEntity) where T : Entity {
            return Physics.Instance.Collides(position, this, tag, out collidedEntity);
        }

        public bool Collides<T>(Vector2 position, Enum tag, out T collidedEntity) where T : Entity {
            return Collides(position, tag.ToString(), out collidedEntity);
        }

        public bool Collides(string tag) {
            return Physics.Instance.Collides(this, tag);
        }

        public bool Collides(Enum tag) {
            return Collides(tag.ToString());
        }

        public bool Collides(string tag, out Collider collidedCollider) {
            return Physics.Instance.Collides(this, tag, out collidedCollider);
        }

        public bool Collides(Enum tag, out Collider collidedCollider) {
            return Collides(tag.ToString(), out collidedCollider);
        }

        public bool Collides<T>(string tag, out T collidedEntity) where T : Entity {
            return Physics.Instance.Collides(this, tag, out collidedEntity);
        }

        public bool Collides<T>(Enum tag, out T collidedEntity) where T : Entity {
            return Collides(tag.ToString(), out collidedEntity);
        }

        #endregion Collides [Single Tag] [Single Output]

        #region Collides [Single Tag] [Multiple Output]

        public bool Collides(Vector2 position, string tag, out List<Collider> collidedColliders) {
            return Physics.Instance.Collides(position, this, tag, out collidedColliders);
        }

        public bool Collides(Vector2 position, Enum tag, out List<Collider> collidedColliders) {
            return Collides(position, tag.ToString(), out collidedColliders);
        }

        public bool Collides<T>(Vector2 position, string tag, out List<T> collidedEntities) where T : Entity {
            return Physics.Instance.Collides(position, this, tag, out collidedEntities);
        }

        public bool Collides<T>(Vector2 position, Enum tag, out List<T> collidedEntities) where T : Entity {
            return Collides(position, tag.ToString(), out collidedEntities);
        }

        public bool Collides(string tag, out List<Collider> collidedColliders) {
            return Physics.Instance.Collides(this, tag, out collidedColliders);
        }

        public bool Collides(Enum tag, out List<Collider> collidedColliders) {
            return Collides(tag.ToString(), out collidedColliders);
        }

        public bool Collides<T>(string tag, out List<T> collidedEntities) where T : Entity {
            return Physics.Instance.Collides(this, tag, out collidedEntities);
        }

        public bool Collides<T>(Enum tag, out List<T> collidedEntities) where T : Entity {
            return Collides(tag.ToString(), out collidedEntities);
        }

        #endregion Collides [Single Tag] [Multiple Output]

        #region Collides [Multiple Tag] [Single Output]

        public bool Collides(Vector2 position, IEnumerable<string> tags) {
            return Physics.Instance.Collides(position, this, tags);
        }

        public bool Collides(Vector2 position, IEnumerable<Enum> tags) {
            foreach (Enum tag in tags) {
                if (Collides(position, tag)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Vector2 position, IEnumerable<string> tags, out Collider collidedCollider) {
            return Physics.Instance.Collides(position, this, tags, out collidedCollider);
        }

        public bool Collides(Vector2 position, IEnumerable<Enum> tags, out Collider collidedCollider) {
            foreach (Enum tag in tags) {
                if (Collides(position, tag, out collidedCollider)) {
                    return true;
                }
            }

            collidedCollider = null;
            return false;
        }

        public bool Collides<T>(Vector2 position, IEnumerable<string> tags, out T collidedEntity) where T : Entity {
            return Physics.Instance.Collides(position, this, tags, out collidedEntity);
        }

        public bool Collides<T>(Vector2 position, IEnumerable<Enum> tags, out T collidedEntity) where T : Entity {
            foreach (Enum tag in tags) {
                if (Collides(position, tag, out collidedEntity)) {
                    return true;
                }
            }

            collidedEntity = null;
            return false;
        }

        public bool Collides(IEnumerable<string> tags) {
            return Physics.Instance.Collides(this, tags);
        }

        public bool Collides(IEnumerable<Enum> tags) {
            foreach (Enum tag in tags) {
                if (Collides(tag)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(IEnumerable<string> tags, out Collider collidedCollider) {
            return Physics.Instance.Collides(this, tags, out collidedCollider);
        }

        public bool Collides(IEnumerable<Enum> tags, out Collider collidedCollider) {
            foreach (Enum tag in tags) {
                if (Collides(tag, out collidedCollider)) {
                    return true;
                }
            }

            collidedCollider = null;
            return false;
        }

        public bool Collides<T>(IEnumerable<string> tags, out T collidedEntity) where T : Entity {
            return Physics.Instance.Collides(this, tags, out collidedEntity);
        }

        public bool Collides<T>(IEnumerable<Enum> tags, out T collidedEntity) where T : Entity {
            foreach (Enum tag in tags) {
                if (Collides(tag, out collidedEntity)) {
                    return true;
                }
            }

            collidedEntity = null;
            return false;
        }

        #endregion Collides [Multiple Tag] [Single Output]

        #region Collides [Multiple Tag] [Multiple Output]

        public bool Collides(Vector2 position, IEnumerable<string> tags, out List<Collider> collidedColliders) {
            return Physics.Instance.Collides(position, this, tags, out collidedColliders);
        }

        public bool Collides(Vector2 position, IEnumerable<Enum> tags, out List<Collider> collidedColliders) {
            collidedColliders = new List<Collider>();
            foreach (Enum tag in tags) {
                List<Collider> collidedTagColliders = new List<Collider>();
                if (Collides(position, tag, out collidedTagColliders)) {
                    collidedColliders.AddRange(collidedTagColliders);
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool Collides<T>(Vector2 position, IEnumerable<string> tags, out List<T> collidedEntities) where T : Entity {
            return Physics.Instance.Collides(position, this, tags, out collidedEntities);
        }

        public bool Collides<T>(Vector2 position, IEnumerable<Enum> tags, out List<T> collidedEntities) where T : Entity {
            collidedEntities = new List<T>();
            foreach (Enum tag in tags) {
                List<T> collidedTagEntities = new List<T>();
                if (Collides(position, tag, out collidedTagEntities)) {
                    collidedEntities.AddRange(collidedTagEntities);
                }
            }

            return collidedEntities.Count > 0;
        }

        public bool Collides(IEnumerable<string> tags, out List<Collider> collidedColliders) {
            return Physics.Instance.Collides(this, tags, out collidedColliders);
        }

        public bool Collides(IEnumerable<Enum> tags, out List<Collider> collidedColliders) {
            collidedColliders = new List<Collider>();
            foreach (Enum tag in tags) {
                List<Collider> collidedTagColliders = new List<Collider>();
                if (Collides(tag, out collidedTagColliders)) {
                    collidedColliders.AddRange(collidedTagColliders);
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool Collides<T>(IEnumerable<string> tags, out List<T> collidedEntities) where T : Entity {
            return Physics.Instance.Collides(this, tags, out collidedEntities);
        }

        public bool Collides<T>(IEnumerable<Enum> tags, out List<T> collidedEntities) where T : Entity {
            collidedEntities = new List<T>();
            foreach (Enum tag in tags) {
                List<T> collidedTagEntities = new List<T>();
                if (Collides(tag, out collidedTagEntities)) {
                    collidedEntities.AddRange(collidedTagEntities);
                }
            }

            return collidedEntities.Count > 0;
        }

        #endregion Collides [Multiple Tag] [Multiple Output]
    }
}
