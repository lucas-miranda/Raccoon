//#define RENDER_COLLISION_CONTACT_POINTS

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

        public delegate void CollisionDelegate(Body body, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo);
        public delegate void EndCollisionDelegate(Body body);

        public event CollisionDelegate OnBeginCollision, OnCollided;
        public event EndCollisionDelegate OnEndCollision;

        #endregion Public Members

        #region Private Members

        private Movement _movement;
        private BitTag _tags = BitTag.None;
        private bool _isPhysicsActive;

        /// <summary>
        /// Keeps a list of bodies colliding with this Body.
        /// </summary>
        private List<Body> _collisionList = new List<Body>();

        /// <summary>
        /// A list used to verify if a collision no longer happens
        /// </summary>
        private HashSet<Body> _currentUpdateCollisionList = new HashSet<Body>();

        private List<Body> _notCollidingAnymoreList = new List<Body>();

#if RENDER_COLLISION_CONTACT_POINTS
        private List<Contact> _contactsToRender = new List<Contact>();
#endif

        #endregion Private Members

        #region Constructors

        public Body() {
            IgnoreDebugRender = false;
            CollisionList = _collisionList.AsReadOnly();
        }

        public Body(IShape shape) : this() {
            Shape = shape;
        }

        #endregion Constructors

        #region Public Properties

        public IShape Shape { get; set; }
        /*
        public IMaterial Material { get; set; }
        public float Mass { get; private set; }
        public float InverseMass { get; private set; }
        */
        public Vector2 LastPosition { get; private set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Force { get; set; }
        //public int Constraints { get { return _constraints.Count; } }
        public bool IsResting { get; private set; } = true;

        /// <summary>
        /// Allow a static body, that doesn't have a Movement defined, to check
        /// for collision at Physics.Step() narrow phase.
        /// Useful to an Entity with multiple body parts.
        /// </summary>
        public bool AllowIndependentCollisionChecksAsStatic { get; set; }

        public float Top { get { return Shape != null ? Position.Y + Shape.BoundingBox.Top : Position.Y; } }
        public float Right { get { return Shape != null ? Position.X + Shape.BoundingBox.Right : Position.X; } }
        public float Bottom { get { return Shape != null ? Position.Y + Shape.BoundingBox.Bottom : Position.Y; } }
        public float Left { get { return Shape != null ? Position.X + Shape.BoundingBox.Left : Position.X; } }
        public Rectangle Bounds { get { return Shape != null ? Shape.BoundingBox + Position : new Rectangle(Position, Size.Empty); } }
        public ReadOnlyCollection<Body> CollisionList { get; private set; }

#if DEBUG
        public Color Color { get; set; } = Color.White;
#endif

        public Vector2 Position { 
            get { 
                return Entity.Transform.Position - Shape.Origin; 
            } 

            set { 
                Entity.Transform.Position = value + Shape.Origin; 
            } 
        }

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
                if (_movement != null && _movement.Body == this) {
                    _movement.OnRemoved();
                    _movement.Dispose();
                }

                _movement = value;

                if (Entity != null && _movement != null) {
                    _movement.OnAdded(this);
                }
            }
        }

        public double MoveBufferX { get; set; }
        public double MoveBufferY { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);

            if (Movement != null && Movement.Body != this) {
                Movement.OnAdded(this);
            }
        }

        public override void OnRemoved() {
            base.OnRemoved();
            if (Movement != null) {
                Movement.OnRemoved();
                Movement.Dispose();
                Movement = null;
            }

            _collisionList.Clear();
            _currentUpdateCollisionList.Clear();
            //_constraints.Clear();

            OnBeginCollision = OnCollided = null;
            OnEndCollision = null;
        }

        public override void OnSceneAdded() {
            base.OnSceneAdded();

            if (Entity.Scene == null || _isPhysicsActive) {
                return;
            }

            Physics.Instance.AddCollider(this);
            _isPhysicsActive = true;
        }

        public override void OnSceneRemoved(bool wipe) {
            base.OnSceneRemoved(wipe);

            if (!_isPhysicsActive) {
                return;
            }

            Physics.Instance.RemoveCollider(this);
            _collisionList.Clear();
            _currentUpdateCollisionList.Clear();
            _isPhysicsActive = false;
        }

        public override void BeforeUpdate() {
            base.BeforeUpdate();

            if (Active && Entity != null && Entity.Active) {
                Movement?.BeforeUpdate();
            }
        }

        public override void Update(int delta) {
            if (Active && Entity != null && Entity.Active) {
                Movement?.Update(delta);
            }
        }

        public override void LateUpdate() {
            base.LateUpdate();

            if (Active && Entity != null && Entity.Active) {
                Movement?.LateUpdate();
            }
        }

        public override void Render() {
        }

#if DEBUG
        public override void DebugRender() {
            if (Active) {
                Shape.DebugRender(Entity.Transform.Position, Color);
            }

#if RENDER_COLLISION_CONTACT_POINTS
            if (_contactsToRender.Count > 0) {
                foreach (Contact contact in _contactsToRender) {
                    Vector2 contactEndPos = contact.Position + contact.Normal * contact.PenetrationDepth;
                    Debug.DrawLine(
                        contact.Position, 
                        contactEndPos, 
                        Color.Magenta
                    );

                    Debug.DrawCircle(
                        contactEndPos,
                        radius: 1f,
                        Color.Magenta
                    );
                }

                _contactsToRender.Clear();
            }
#endif

            // Position and Velocity
            if (ShowDebugInfo) {
                Debug.DrawString(
                    Position + new Vector2(Shape.BoundingBox.Width / 1.9f, 0),
                    string.Format("[{0:0.##}, {1:0.##}]\nVelocity: [{2:0.##}, {3:0.##}]\nM: {4}",
                        Position.X,
                        Position.Y,
                        Velocity.X,
                        Velocity.Y,
                        1f
                        //Mass
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
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            _currentUpdateCollisionList.Clear();
            _notCollidingAnymoreList.AddRange(_collisionList);
            Movement?.PhysicsUpdate(dt);
        }

        public void PhysicsCollisionSubmit(Body otherBody, Vector2 movement, ReadOnlyCollection<Contact> horizontalContacts, ReadOnlyCollection<Contact> verticalContacts) {
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            Movement?.PhysicsCollisionSubmit(otherBody, movement, horizontalContacts, verticalContacts);
        }

        public void PhysicsStepMove(int movementX, int movementY) {
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            Movement?.PhysicsStepMove(movementX, movementY);
        }

        public void PhysicsLateUpdate() {
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            _collisionList.Clear();
            _collisionList.AddRange(_currentUpdateCollisionList);
            _currentUpdateCollisionList.Clear();

            foreach (Body notCollidingBody in _notCollidingAnymoreList) {
                EndCollision(notCollidingBody);
            }
            _notCollidingAnymoreList.Clear();

            Movement?.PhysicsLateUpdate();
            IsResting = (Position - LastPosition).LengthSquared() == 0f;
        }

        public void BeforeSolveCollisions() {
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            Movement?.BeforeBodySolveCollisions();
        }

        public void CollidedWith(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            if (_currentUpdateCollisionList.Contains(otherBody)) {
                return;
            }

            _currentUpdateCollisionList.Add(otherBody);
            _notCollidingAnymoreList.Remove(otherBody);

            if (!_collisionList.Contains(otherBody)) {
                _collisionList.Add(otherBody);
                BeginCollision(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);
            }

            Collided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);
        }

        public void AfterSolveCollisions() {
            if (!Active || Entity == null || !Entity.Active) {
                return;
            }

            Movement?.AfterBodySolveCollisions();
        }

        public Vector2 Integrate(float dt) {
            if (!Active || Entity == null || !Entity.Active) {
                return Position;
            }

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

            return Position + (Velocity + Force) * dt;
        }

        public void ApplyConstantForce(Vector2 force) {
            Force += force;
        }

        public void ClearConstantForces() {
            Force = Vector2.Zero;
        }

        public void SetStaticPosition(Vector2 staticPosition) {
            staticPosition = Util.Math.Floor(staticPosition);
            Position = staticPosition;
            Velocity = Vector2.Zero;
        }

        public override string ToString() {
            return $"[Body | Shape: {Shape}, Movement: {Movement}]";
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }
        
            if (Shape != null) {
                if (Shape is GridShape gridShape) {
                    gridShape.Dispose();
                }

                Shape = null;
            }

            if (Movement != null) {
                Movement.Dispose();
                Movement = null;
            }

            //_constraints.Clear();
            _collisionList.Clear();
            _currentUpdateCollisionList.Clear();

            base.Dispose();
        }

        #region Direct Collision Test

        public bool Collides(Body otherBody, out ContactList contacts) {
            return Physics.Instance.Collides(Shape, Position, otherBody.Shape, otherBody.Position, out contacts);
        }

        public bool Collides(Body otherBody, Vector2 position, out ContactList contacts) {
            return Physics.Instance.Collides(Shape, Position, otherBody.Shape, position, out contacts);
        }

        public bool Collides(Vector2 position, Body otherBody, Vector2 otherPosition, out ContactList contacts) {
            return Physics.Instance.Collides(Shape, position, otherBody.Shape, otherPosition, out contacts);
        }

        #endregion Direct Collision Test

        #region Collides [Single Output]

        public bool Collides(Vector2 position, BitTag tags, out ContactList contacts) {
            if (Entity == null || Shape == null) {
                contacts = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, position, tags, out contacts);
        }

        public bool Collides(BitTag tags, out ContactList contacts) {
            if (Entity == null || Shape == null) {
                contacts = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, Position, tags, out contacts);
        }

        public bool Collides(Vector2 position, out ContactList contacts) {
            if (Entity == null || Shape == null) {
                contacts = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out contacts);
        }

        public bool Collides(out ContactList contacts) {
            return Collides(Position, out contacts);
        }

        public bool Collides(Vector2 position, BitTag tags, out CollisionInfo<Body> collisionInfo) {
            if (Entity == null || Shape == null) {
                collisionInfo = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, position, tags, out collisionInfo);
        }

        public bool Collides(BitTag tags, out CollisionInfo<Body> collisionInfo) {
            if (Entity == null || Shape == null) {
                collisionInfo = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, Position, tags, out collisionInfo);
        }

        public bool Collides(Vector2 position, out CollisionInfo<Body> collisionInfo) {
            if (Entity == null || Shape == null) {
                collisionInfo = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionInfo);
        }

        public bool Collides(out CollisionInfo<Body> collisionInfo) {
            return Collides(Position, out collisionInfo);
        }

        public bool Collides<T>(Vector2 position, BitTag tags, out CollisionInfo<T> collisionInfo) where T : Entity {
            if (Entity == null || Shape == null) {
                collisionInfo = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, position, tags, out collisionInfo);
        }

        public bool Collides<T>(BitTag tags, out CollisionInfo<T> collisionInfo) where T : Entity {
            if (Entity == null || Shape == null) {
                collisionInfo = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, Position, tags, out collisionInfo);
        }

        public bool Collides<T>(Vector2 position, out CollisionInfo<T> collisionInfo) where T : Entity {
            if (Entity == null || Shape == null) {
                collisionInfo = null;
                return false;
            }

            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionInfo);
        }

        public bool Collides<T>(out CollisionInfo<T> collisionInfo) where T : Entity {
            return Collides(Position, out collisionInfo);
        }

        #endregion Collides [Single Output]

        #region Collides [Multiple Output]

        public bool CollidesMultiple(Vector2 position, BitTag tags, out CollisionList<Body> collisionList) {
            if (Entity == null || Shape == null) {
                collisionList = null;
                return false;
            }

            return Physics.Instance.QueryMultipleCollision(Shape, position, tags, out collisionList);
        }

        public bool CollidesMultiple(BitTag tags, out CollisionList<Body> collisionList) {
            if (Entity == null || Shape == null) {
                collisionList = null;
                return false;
            }

            return Physics.Instance.QueryMultipleCollision(Shape, Position, tags, out collisionList);
        }

        public bool CollidesMultiple(Vector2 position, out CollisionList<Body> collisionList) {
            if (Entity == null || Shape == null) {
                collisionList = null;
                return false;
            }

            return Physics.Instance.QueryMultipleCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collisionList);
        }

        public bool CollidesMultiple(out CollisionList<Body> collisionList) {
            return CollidesMultiple(Position, out collisionList);
        }

        public bool CollidesMultiple<T>(Vector2 position, BitTag tags, out CollisionList<T> collisionList) where T : Entity {
            if (Entity == null || Shape == null) {
                collisionList = null;
                return false;
            }

            return Physics.Instance.QueryMultipleCollision(Shape, position, tags, out collisionList);
        }

        public bool CollidesMultiple<T>(BitTag tags, out CollisionList<T> collisionList) where T : Entity {
            if (Entity == null || Shape == null) {
                collisionList = null;
                return false;
            }

            return Physics.Instance.QueryMultipleCollision(Shape, Position, tags, out collisionList);
        }

        public bool CollidesMultiple<T>(Vector2 position, out CollisionList<T> collisionList) where T : Entity {
            if (Entity == null || Shape == null) {
                collisionList = null;
                return false;
            }

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
            OnBeginCollision?.Invoke(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

            if (Movement != null) {
                Movement.BeginBodyCollision(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

                if (otherBody.Tags.HasAny(Movement.CollisionTags)) {
                    Movement.BeginCollision(collisionAxes, hCollisionInfo, vCollisionInfo);
                }
            }
        }

        protected virtual void Collided(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            OnCollided?.Invoke(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

            if (Movement != null) {
                Movement.BodyCollided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

                if (otherBody.Tags.HasAny(Movement.CollisionTags)) {
                    Movement.Collided(collisionAxes, hCollisionInfo, vCollisionInfo);
                }
            }

#if RENDER_COLLISION_CONTACT_POINTS
            _contactsToRender.AddRange(hCollisionInfo.Contacts);
            _contactsToRender.AddRange(vCollisionInfo.Contacts);
#endif
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
