using System.Collections.Generic;
using System.Collections.ObjectModel;

using Raccoon.Physics;
using Raccoon.Graphics;

namespace Raccoon.Components {
    public abstract class Collider : Component {
        #region Private Members

        private event System.Action<Collider, Collider> GeneralCollisionCallback = delegate { };
        private Dictionary<string, System.Action<Collider, Collider>> _collisionTagCallbacks = new Dictionary<string, System.Action<Collider, Collider>>();
        private List<string> _tags = new List<string>();

        #endregion Private Members

        #region Constructors
        
        public Collider(params string[] tags) {
            if (tags.Length == 0) {
                throw new System.ArgumentException("Collider needs at least one tag.");
            }

            for (int i = 0; i < tags.Length; i++) {
                string tag = tags[i];
                if (string.IsNullOrWhiteSpace(tag)) {
                    throw new System.InvalidOperationException($"Invalid tag value (tag index: {i}).");
                }

                _tags.Add(tag);
            }
        }

        public Collider(params System.Enum[] tags) {
            if (tags.Length == 0) {
                throw new System.ArgumentException("Collider needs at least one tag.");
            }

            foreach (System.Enum e in tags) {
                _tags.Add(e.ToString());
            }
        }

        #endregion Constructors

        #region Public Properties

        public ReadOnlyCollection<string> Tags { get { return _tags.AsReadOnly(); } }
        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } set { Entity.Position = value + Origin; } }
        public Size Size { get; protected set; }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public float Top { get { return Y - Origin.Y; } }
        public float Right { get { return X + Width - Origin.X; } }
        public float Bottom { get { return Y + Height - Origin.Y; } }
        public float Left { get { return X - Origin.X; } }
        public float Mass { get; set; } = 1f;
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }
        public int ConstraintsCount { get { return Constraints.Count; } }

        #endregion Public Properties

        #region Protected Properties

        protected Graphic Graphic { get; set; }
        protected Color Color { get; set; } = Color.Red;
        protected List<IConstraint> Constraints { get; } = new List<IConstraint>();

        #endregion Protected Properties

        #region Public Methods

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);
            Physics.Physics.Instance.AddCollider(this);
        }

        public override void OnRemoved() {
            Physics.Physics.Instance.RemoveCollider(this);
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

        public void SolveConstraints() {
            foreach (IConstraint constraint in Constraints) {
                constraint.Solve();
            }
        }

        public void AddConstraint(IConstraint constraint) {
            Constraints.Add(constraint);
        }

        public void RemoveConstraint(IConstraint constraint) {
            Constraints.Remove(constraint);
        }

        public virtual void OnCollide(Collider collider) {
            GeneralCollisionCallback(this, collider);
            foreach (string tagName in collider.Tags) {
                if (_collisionTagCallbacks.TryGetValue(tagName, out System.Action<Collider, Collider> SpecificCollisionCallback)) {
                    SpecificCollisionCallback(this, collider);
                }
            }
        }

        public void RegisterCallback(System.Action<Collider, Collider> callback, params string[] tagNames) {
            foreach (string tagName in tagNames) {
                if (!Physics.Physics.Instance.HasTag(tagName)) {
                    throw new System.ArgumentException($"Tag '{tagName}' has not been registered.");
                }

                _collisionTagCallbacks[tagName] = callback;
            }
        }

        public void RegisterCallback(System.Action<Collider, Collider> callback, params System.Enum[] tags) {
            string[] tagNames = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++) {
                tagNames[i] = tags[i].ToString();
            }

            RegisterCallback(callback, tagNames);
        }

        #region Collides [Single Tag] [Single Output]

        public bool Collides(Vector2 position, string tag) {
            return Physics.Physics.Instance.Collides(position, this, tag);
        }

        public bool Collides(Vector2 position, System.Enum tag) {
            return Collides(position, tag.ToString());
        }

        public bool Collides(Vector2 position, string tag, out Collider collidedCollider) {
            return Physics.Physics.Instance.Collides(position, this, tag, out collidedCollider);
        }

        public bool Collides(Vector2 position, System.Enum tag, out Collider collidedCollider) {
            return Collides(position, tag.ToString(), out collidedCollider);
        }

        public bool Collides<T>(Vector2 position, string tag, out T collidedEntity) where T : Entity {
            return Physics.Physics.Instance.Collides(position, this, tag, out collidedEntity);
        }

        public bool Collides<T>(Vector2 position, System.Enum tag, out T collidedEntity) where T : Entity {
            return Collides(position, tag.ToString(), out collidedEntity);
        }

        public bool Collides(string tag) {
            return Physics.Physics.Instance.Collides(this, tag);
        }

        public bool Collides(System.Enum tag) {
            return Collides(tag.ToString());
        }

        public bool Collides(string tag, out Collider collidedCollider) {
            return Physics.Physics.Instance.Collides(this, tag, out collidedCollider);
        }

        public bool Collides(System.Enum tag, out Collider collidedCollider) {
            return Collides(tag.ToString(), out collidedCollider);
        }

        public bool Collides<T>(string tag, out T collidedEntity) where T : Entity {
            return Physics.Physics.Instance.Collides(this, tag, out collidedEntity);
        }

        public bool Collides<T>(System.Enum tag, out T collidedEntity) where T : Entity {
            return Collides(tag.ToString(), out collidedEntity);
        }

        #endregion Collides [Single Tag] [Single Output]

        #region Collides [Single Tag] [Multiple Output]

        public bool Collides(Vector2 position, string tag, out List<Collider> collidedColliders) {
            return Physics.Physics.Instance.Collides(position, this, tag, out collidedColliders);
        }

        public bool Collides(Vector2 position, System.Enum tag, out List<Collider> collidedColliders) {
            return Collides(position, tag.ToString(), out collidedColliders);
        }

        public bool Collides<T>(Vector2 position, string tag, out List<T> collidedEntities) where T : Entity {
            return Physics.Physics.Instance.Collides(position, this, tag, out collidedEntities);
        }

        public bool Collides<T>(Vector2 position, System.Enum tag, out List<T> collidedEntities) where T : Entity {
            return Collides(position, tag.ToString(), out collidedEntities);
        }

        public bool Collides(string tag, out List<Collider> collidedColliders) {
            return Physics.Physics.Instance.Collides(this, tag, out collidedColliders);
        }

        public bool Collides(System.Enum tag, out List<Collider> collidedColliders) {
            return Collides(tag.ToString(), out collidedColliders);
        }

        public bool Collides<T>(string tag, out List<T> collidedEntities) where T : Entity {
            return Physics.Physics.Instance.Collides(this, tag, out collidedEntities);
        }

        public bool Collides<T>(System.Enum tag, out List<T> collidedEntities) where T : Entity {
            return Collides(tag.ToString(), out collidedEntities);
        }

        #endregion Collides [Single Tag] [Multiple Output]

        #region Collides [Multiple Tag] [Single Output]

        public bool Collides(Vector2 position, IEnumerable<string> tags) {
            return Physics.Physics.Instance.Collides(position, this, tags);
        }

        public bool Collides(Vector2 position, IEnumerable<System.Enum> tags) {
            foreach (System.Enum tag in tags) {
                if (Collides(position, tag)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(Vector2 position, IEnumerable<string> tags, out Collider collidedCollider) {
            return Physics.Physics.Instance.Collides(position, this, tags, out collidedCollider);
        }

        public bool Collides(Vector2 position, IEnumerable<System.Enum> tags, out Collider collidedCollider) {
            foreach (System.Enum tag in tags) {
                if (Collides(position, tag, out collidedCollider)) {
                    return true;
                }
            }

            collidedCollider = null;
            return false;
        }

        public bool Collides<T>(Vector2 position, IEnumerable<string> tags, out T collidedEntity) where T : Entity {
            return Physics.Physics.Instance.Collides(position, this, tags, out collidedEntity);
        }

        public bool Collides<T>(Vector2 position, IEnumerable<System.Enum> tags, out T collidedEntity) where T : Entity {
            foreach (System.Enum tag in tags) {
                if (Collides(position, tag, out collidedEntity)) {
                    return true;
                }
            }

            collidedEntity = null;
            return false;
        }

        public bool Collides(IEnumerable<string> tags) {
            return Physics.Physics.Instance.Collides(this, tags);
        }

        public bool Collides(IEnumerable<System.Enum> tags) {
            foreach (System.Enum tag in tags) {
                if (Collides(tag)) {
                    return true;
                }
            }

            return false;
        }

        public bool Collides(IEnumerable<string> tags, out Collider collidedCollider) {
            return Physics.Physics.Instance.Collides(this, tags, out collidedCollider);
        }

        public bool Collides(IEnumerable<System.Enum> tags, out Collider collidedCollider) {
            foreach (System.Enum tag in tags) {
                if (Collides(tag, out collidedCollider)) {
                    return true;
                }
            }

            collidedCollider = null;
            return false;
        }

        public bool Collides<T>(IEnumerable<string> tags, out T collidedEntity) where T : Entity {
            return Physics.Physics.Instance.Collides(this, tags, out collidedEntity);
        }

        public bool Collides<T>(IEnumerable<System.Enum> tags, out T collidedEntity) where T : Entity {
            foreach (System.Enum tag in tags) {
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
            return Physics.Physics.Instance.Collides(position, this, tags, out collidedColliders);
        }

        public bool Collides(Vector2 position, IEnumerable<System.Enum> tags, out List<Collider> collidedColliders) {
            collidedColliders = new List<Collider>();
            foreach (System.Enum tag in tags) {
                List<Collider> collidedTagColliders = new List<Collider>();
                if (Collides(position, tag, out collidedTagColliders)) {
                    collidedColliders.AddRange(collidedTagColliders);
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool Collides<T>(Vector2 position, IEnumerable<string> tags, out List<T> collidedEntities) where T : Entity {
            return Physics.Physics.Instance.Collides(position, this, tags, out collidedEntities);
        }

        public bool Collides<T>(Vector2 position, IEnumerable<System.Enum> tags, out List<T> collidedEntities) where T : Entity {
            collidedEntities = new List<T>();
            foreach (System.Enum tag in tags) {
                List<T> collidedTagEntities = new List<T>();
                if (Collides(position, tag, out collidedTagEntities)) {
                    collidedEntities.AddRange(collidedTagEntities);
                }
            }

            return collidedEntities.Count > 0;
        }

        public bool Collides(IEnumerable<string> tags, out List<Collider> collidedColliders) {
            return Physics.Physics.Instance.Collides(this, tags, out collidedColliders);
        }

        public bool Collides(IEnumerable<System.Enum> tags, out List<Collider> collidedColliders) {
            collidedColliders = new List<Collider>();
            foreach (System.Enum tag in tags) {
                List<Collider> collidedTagColliders = new List<Collider>();
                if (Collides(tag, out collidedTagColliders)) {
                    collidedColliders.AddRange(collidedTagColliders);
                }
            }

            return collidedColliders.Count > 0;
        }

        public bool Collides<T>(IEnumerable<string> tags, out List<T> collidedEntities) where T : Entity {
            return Physics.Physics.Instance.Collides(this, tags, out collidedEntities);
        }

        public bool Collides<T>(IEnumerable<System.Enum> tags, out List<T> collidedEntities) where T : Entity {
            collidedEntities = new List<T>();
            foreach (System.Enum tag in tags) {
                List<T> collidedTagEntities = new List<T>();
                if (Collides(tag, out collidedTagEntities)) {
                    collidedEntities.AddRange(collidedTagEntities);
                }
            }

            return collidedEntities.Count > 0;
        }

        #endregion Collides [Multiple Tag] [Multiple Output]

        #endregion Public Methods
    }
}
