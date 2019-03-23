using System.Collections.Generic;
using System.Collections.ObjectModel;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon.Components {
    public class Body : Component {
        #region Public Members

#if DEBUG
        public static bool ShowDebugInfo = false;
#endif

        public static IMaterial StandardMaterial = new StandardMaterial();
        public event System.Action<Body, Vector2> OnBeginCollision, OnCollided;
        public event System.Action<Body> OnEndCollision;

        #endregion Public Members

        #region Private Members

        private List<IConstraint> _constraints = new List<IConstraint>();
        private Movement _movement;
        private BitTag _tags = BitTag.None;
        private bool _isPhysicsActive;

        private List<Body> _collisionList = new List<Body>();
        private HashSet<Body> _currentUpdateCollisionList = new HashSet<Body>();

        #endregion Private Members

        #region Constructors

        public Body(IShape shape, IMaterial material = null) {
            Shape = shape;
            Material = material ?? StandardMaterial;
            Mass = Shape.ComputeMass(1f);
            InverseMass = Mass == 0f ? 0f : (1f / Mass);
            CollisionList = _collisionList.AsReadOnly();
        }

        #endregion Constructors

        #region Public Properties

        public IShape Shape { get; set; }
        public IMaterial Material { get; set; }
        public float Mass { get; private set; }
        public float InverseMass { get; private set; }
        public Vector2 LastPosition { get; private set; }
        public Vector2 Position { get { return Entity.Transform.Position - Shape.Origin; } set { Entity.Transform.Position = value + Shape.Origin; } }
        public Vector2 Velocity { get; set; }
        public Vector2 Force { get; set; }
        public int Constraints { get { return _constraints.Count; } }
        public bool IsResting { get; private set; } = true;
        public float Top { get { return Shape != null ? Position.Y + Shape.BoundingBox.Top : Position.Y; } }
        public float Right { get { return Shape != null ? Position.X + Shape.BoundingBox.Right : Position.X; } }
        public float Bottom { get { return Shape != null ? Position.Y + Shape.BoundingBox.Bottom : Position.Y; } }
        public float Left { get { return Shape != null ? Position.X + Shape.BoundingBox.Left : Position.X; } }
        public Rectangle Bounds { get { return Shape.BoundingBox + Position; } }
        public ReadOnlyCollection<Body> CollisionList { get; }

#if DEBUG
        public Color Color { get; set; } = Color.White;
#endif

        public BitTag Tags {
            get {
                return _tags;
            }

            set {
                if (_isPhysicsActive) {
                    BitTag oldTags = _tags;
                    _tags = value;
                    Physics.Instance.UpdateColliderTagsEntry(this, oldTags);
                    return;
                }

                _tags = value;
            }
        }

        public Movement Movement {
            get {
                return _movement;
            }

            set {
                if (_movement != null) {
                    _movement.OnRemoved();
                }

                _movement = value;

                if (Entity != null && _movement != null) {
                    _movement.OnAdded(this);
                }
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal double MoveBufferX { get; set; }
        internal double MoveBufferY { get; set; }

        #endregion Internal Properties

        #region Public Methods

        public override void OnAdded(Entity entity) {
            if (entity.Scene == null) {
                Active = false;
                return;
            }

            base.OnAdded(entity);

            if (Movement != null && Movement.Body != this) {
                Movement.OnAdded(this);
            }
        }

        public override void OnRemoved() {
            base.OnRemoved();

            Movement?.OnRemoved();
        }

        public override void OnSceneAdded() {
            base.OnSceneAdded();

            if (Entity.Scene == null || _isPhysicsActive) {
                return;
            }

            Physics.Instance.AddCollider(this);
            _isPhysicsActive = true;
        }

        public override void OnSceneRemoved() {
            base.OnSceneRemoved();

            if (!_isPhysicsActive) {
                return;
            }

            Physics.Instance.RemoveCollider(this);
            _isPhysicsActive = false;
        }

        public override void BeforeUpdate() {
            base.BeforeUpdate();
            Movement?.BeforeUpdate();
        }

        public override void Update(int delta) {
            Movement?.Update(delta);
        }

        public override void LateUpdate() {
            base.LateUpdate();
            Movement?.LateUpdate();
        }

        public override void Render() {
        }

#if DEBUG
        Contact[] contacts = null;
        public override void DebugRender() {
            Shape.DebugRender(Entity.Transform.Position, Color);

            if (contacts != null) {
                foreach (Contact contact in contacts) {
                    Debug.DrawLine(contact.Position, contact.Position + contact.Normal * contact.PenetrationDepth, Color.Magenta);
                }

                contacts = null;
            }

            // Position and Velocity
            if (ShowDebugInfo) {
                Debug.DrawString(
                    Position + new Vector2(Shape.BoundingBox.Width / 1.9f, 0), 
                    string.Format("[{0:0.##}, {1:0.##}]\nVelocity: [{2:0.##}, {3:0.##}]\nM: {4}", 
                        Position.X, 
                        Position.Y, 
                        Velocity.X, 
                        Velocity.Y, 
                        Mass
                    )
                );
            }

            if (Movement != null && Movement.IsDebugRenderEnabled) {
                Movement.DebugRender();
            }
        }
#endif

        public void PhysicsUpdate(float dt) {
            LastPosition = Position;
            _currentUpdateCollisionList.Clear();

            if (Movement != null) {
                Movement.PhysicsUpdate(dt);
            }
        }

        public void PhysicsLateUpdate() {
            foreach (Body otherBody in _collisionList) {
                if (_currentUpdateCollisionList.Contains(otherBody)) {
                    continue;
                }

                EndCollision(otherBody);
            }

            _collisionList.Clear();
            _collisionList.AddRange(_currentUpdateCollisionList);

            if (Movement != null) {
                Movement.PhysicsLateUpdate();
            }

            IsResting = (Position - LastPosition).LengthSquared() == 0f;
        }

        public void CollidedWith(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            _currentUpdateCollisionList.Add(otherBody);

            if (!_collisionList.Contains(otherBody)) {
                _collisionList.Add(otherBody);
                BeginCollision(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);
            }

            Collided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);
        }

        public Vector2 Integrate(float dt) {
            Vector2 velocity = Velocity;

            // velocity X correction
            if (Math.EqualsEstimate(velocity.X, 0f)) {
                velocity.X = 0f;
            }

            // velocity Y correction
            if (Math.EqualsEstimate(velocity.Y, 0f)) {
                velocity.Y = 0f;
            }

            Velocity = velocity;

            if (Movement != null) {
                return Position + Movement.Integrate(dt);
            }

            Velocity += Force * dt;

            return Position + Velocity * dt;
        }

        public void SolveConstraints() {
            foreach (IConstraint constraint in _constraints) {
                constraint.Solve();
            }
        }

        public void AddConstraint(IConstraint constraint) {
            _constraints.Add(constraint);
        }

        public void RemoveConstraint(IConstraint constraint) {
            _constraints.Remove(constraint);
        }

        /*public void ApplyForce(Vector2 force) {
            Force += Movement?.HandleForce(force) ?? force;
        }*/

        public void SetStaticPosition(Vector2 staticPosition) {
            staticPosition = Util.Math.Floor(staticPosition);
            Position = staticPosition;
            Velocity = Vector2.Zero;
        }

        public override string ToString() {
            return $"[Body | Shape: {Shape}, Movement: {Movement}]";
        }

        #region Collides [Single Output]

        public bool Collides(Vector2 position, BitTag tags, out ContactList contacts) {
            return Physics.Instance.QueryCollision(Shape, position, tags, out contacts);
        }

        public bool Collides(BitTag tags, out ContactList contacts) {
            return Physics.Instance.QueryCollision(Shape, Position, tags, out contacts);
        }

        public bool Collides(Vector2 position, out ContactList contacts) {
            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out contacts);
        }

        public bool Collides(out ContactList contacts) {
            return Collides(Position, out contacts);
        }

        public bool Collides(Vector2 position, BitTag tags, out CollisionInfo<Body> collisionInfo) {
            return Physics.Instance.QueryCollision(Shape, position, tags, out collisionInfo);
        }

        public bool Collides(BitTag tags, out CollisionInfo<Body> collisionInfo) {
            return Physics.Instance.QueryCollision(Shape, Position, tags, out collisionInfo);
        }

        public bool Collides(Vector2 position, out CollisionInfo<Body> collisionInfo) {
            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionInfo);
        }

        public bool Collides(out CollisionInfo<Body> collisionInfo) {
            return Collides(Position, out collisionInfo);
        }

        public bool Collides<T>(Vector2 position, BitTag tags, out CollisionInfo<T> collisionInfo) where T : Entity {
            return Physics.Instance.QueryCollision(Shape, position, tags, out collisionInfo);
        }

        public bool Collides<T>(BitTag tags, out CollisionInfo<T> collisionInfo) where T : Entity {
            return Physics.Instance.QueryCollision(Shape, Position, tags, out collisionInfo);
        }

        public bool Collides<T>(Vector2 position, out CollisionInfo<T> collisionInfo) where T : Entity {
            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionInfo);
        }

        public bool Collides<T>(out CollisionInfo<T> collisionInfo) where T : Entity {
            return Collides(Position, out collisionInfo);
        }

        #endregion Collides [Single Output]

        #region Collides [Multiple Output]

        public bool CollidesMultiple(Vector2 position, BitTag tags, out CollisionList<Body> collisionList) {
            return Physics.Instance.QueryMultipleCollision(Shape, position, tags, out collisionList);
        }

        public bool CollidesMultiple(BitTag tags, out CollisionList<Body> collisionList) {
            return Physics.Instance.QueryMultipleCollision(Shape, Position, tags, out collisionList);
        }

        public bool CollidesMultiple(Vector2 position, out CollisionList<Body> collisionList) {
            return Physics.Instance.QueryMultipleCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionList);
        }

        public bool CollidesMultiple(out CollisionList<Body> collisionList) {
            return CollidesMultiple(Position, out collisionList);
        }

        public bool CollidesMultiple<T>(Vector2 position, BitTag tags, out CollisionList<T> collisionList) where T : Entity {
            return Physics.Instance.QueryMultipleCollision(Shape, position, tags, out collisionList);
        }

        public bool CollidesMultiple<T>(BitTag tags, out CollisionList<T> collisionList) where T : Entity {
            return Physics.Instance.QueryMultipleCollision(Shape, Position, tags, out collisionList);
        }

        public bool CollidesMultiple<T>(Vector2 position, out CollisionList<T> collisionList) where T : Entity {
            return Physics.Instance.QueryMultipleCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionList);
        }

        public bool CollidesMultiple<T>(out CollisionList<T> collisionList) where T : Entity {
            return CollidesMultiple(Position, out collisionList);
        }

        #endregion Collides [Multiple Output]

        #endregion Public Methods

        #region Protected Methods

        protected override void OnActivate() {
            base.OnActivate();

            if (Entity.Scene == null || _isPhysicsActive) {
                return;
            }

            Physics.Instance.AddCollider(this);
            _isPhysicsActive = true;
        }

        protected override void OnDeactivate() {
            base.OnDeactivate();

            if (!_isPhysicsActive) {
                return;
            }

            Physics.Instance.RemoveCollider(this);
            _isPhysicsActive = false;
        }

        protected virtual void BeginCollision(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            OnBeginCollision?.Invoke(otherBody, collisionAxes);

            if (Movement != null) {
                Movement.BeginBodyCollision(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

                if (otherBody.Tags.HasAny(Movement.CollisionTags)) {
                    Movement.BeginCollision(collisionAxes, hCollisionInfo, vCollisionInfo);
                }
            }
        }

        protected virtual void Collided(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            OnCollided?.Invoke(otherBody, collisionAxes);

            if (Movement != null) {
                Movement.BodyCollided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

                if (otherBody.Tags.HasAny(Movement.CollisionTags)) {
                    Movement.Collided(collisionAxes, hCollisionInfo, vCollisionInfo);
                }
            }
        }

        protected virtual void EndCollision(Body otherBody) {
            OnEndCollision?.Invoke(otherBody);

            if (Movement != null) {
                Movement.EndBodyCollision(otherBody);

                if (otherBody.Tags.HasAny(Movement.CollisionTags)) {
                    Movement.EndCollision();
                }
            }
        }

        #endregion Protected Methods
    }
}
