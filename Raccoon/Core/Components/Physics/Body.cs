using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon.Components {
    public class Body : Component {
        #region Public Members

#if DEBUG
        public static bool ShowDebugInfo = false;
#endif

        public static IMaterial StandardMaterial = new StandardMaterial();
        public event System.Action<Body, Vector2> OnCollided;

        #endregion Public Members

        #region Private Members

        private List<IConstraint> _constraints = new List<IConstraint>();
        private Movement _movement;
        private BitTag _tags = BitTag.None;
        private bool _isPhysicsActive;

        #endregion Private Members

        #region Constructors

        public Body(IShape shape, IMaterial material = null) {
            Shape = shape;
            Material = material ?? StandardMaterial;
            Mass = Shape.ComputeMass(1f);
            InverseMass = Mass == 0f ? 0f : (1f / Mass);
        }

        #endregion Constructors

        #region Public Properties

        public IShape Shape { get; private set; }
        public IMaterial Material { get; set; }
        public float Mass { get; private set; }
        public float InverseMass { get; private set; }
        public Vector2 LastPosition { get; private set; }
        public Vector2 Position { get { return Entity.Position - Shape.Origin; } set { Entity.Position = value + Shape.Origin; } }
        public Vector2 Velocity { get; set; }
        public Vector2 Force { get; set; }
        public int Constraints { get { return _constraints.Count; } }
        public bool IsResting { get; private set; } = true;
        public float Top { get { return Shape != null ? Position.Y - Shape.BoundingBox.Height / 2f : Position.Y; } }
        public float Right { get { return Shape != null ? Position.X + Shape.BoundingBox.Width / 2f : Position.X; } }
        public float Bottom { get { return Shape != null ? Position.Y + Shape.BoundingBox.Height / 2f : Position.Y; } }
        public float Left { get { return Shape != null ? Position.X - Shape.BoundingBox.Width / 2f : Position.X; } }

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

                if (Entity != null) {
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
                return;
            }

            base.OnAdded(entity);
            Physics.Instance.AddCollider(this);
            _isPhysicsActive = true;

            if (Movement != null && Movement.Body != this) {
                Movement.OnAdded(this);
            }
        }

        public override void OnRemoved() {
            base.OnRemoved();
            Physics.Instance.RemoveCollider(this);
            _isPhysicsActive = false;

            Movement?.OnRemoved();
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
            Shape.DebugRender(Entity.Position, Color);

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
            if (Movement != null) {
                Movement.PhysicsUpdate(dt);
            }
        }

        public void PhysicsLateUpdate() {
            if (Movement != null) {
                Movement.PhysicsLateUpdate();
            }

            IsResting = (Position - LastPosition).LengthSquared() == 0f;
        }

        public void OnCollide(Body otherBody, Vector2 collisionAxes) {
            OnCollided?.Invoke(otherBody, collisionAxes);

            if (Movement != null) {
                Movement.OnBodyCollide(otherBody, collisionAxes);

                if (otherBody.Tags.HasAny(Movement.CollisionTags)) {
                    Movement.OnCollide(collisionAxes);
                }
            }
        }

        public Vector2 Integrate(float dt) {
            Vector2 velocity = Velocity;

            // velocity X correction
            if (Util.Math.EqualsEstimate(velocity.X, 0f)) {
                velocity.X = 0f;
            }

            // velocity Y correction
            if (Util.Math.EqualsEstimate(velocity.Y, 0f)) {
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

        public bool Collides(Vector2 position, BitTag tags, out Contact[] contacts) {
            return Physics.Instance.QueryCollision(Shape, position, tags, out contacts);
        }

        public bool Collides(BitTag tags, out Contact[] contacts) {
            return Physics.Instance.QueryCollision(Shape, Position, tags, out contacts);
        }

        public bool Collides(Vector2 position, out Contact[] contacts) {
            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out contacts);
        }

        public bool Collides(out Contact[] contacts) {
            return Collides(Position, out contacts);
        }

        public bool Collides(Vector2 position, BitTag tags, out Body collidedCollider, out Contact[] contact) {
            return Physics.Instance.QueryCollision(Shape, position, tags, out collidedCollider, out contact);
        }

        public bool Collides(BitTag tags, out Body collidedCollider, out Contact[] contact) {
            return Physics.Instance.QueryCollision(Shape, Position, tags, out collidedCollider, out contact);
        }

        public bool Collides(Vector2 position, out Body collidedCollider, out Contact[] contacts) {
            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collidedCollider, out contacts);
        }

        public bool Collides(out Body collidedCollider, out Contact[] contacts) {
            return Collides(Position, out collidedCollider, out contacts);
        }

        public bool Collides<T>(Vector2 position, BitTag tags, out T collidedEntity, out Contact[] contacts) where T : Entity {
            return Physics.Instance.QueryCollision(Shape, position, tags, out collidedEntity, out contacts);
        }

        public bool Collides<T>(BitTag tags, out T collidedEntity, out Contact[] contacts) where T : Entity {
            return Physics.Instance.QueryCollision(Shape, Position, tags, out collidedEntity, out contacts);
        }

        public bool Collides<T>(Vector2 position, out T collidedEntity, out Contact[] contacts) where T : Entity {
            return Physics.Instance.QueryCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collidedEntity, out contacts);
        }

        public bool Collides<T>(out T collidedEntity, out Contact[] contacts) where T : Entity {
            return Collides(Position, out collidedEntity, out contacts);
        }

        #endregion Collides [Single Output]

        #region Collides [Multiple Output]

        public bool CollidesMultiple(Vector2 position, BitTag tags, out List<(Body collider, Contact[] contacts)> collidedColliders) {
            return Physics.Instance.QueryMultipleCollision(Shape, position, tags, out collidedColliders);
        }

        public bool CollidesMultiple(BitTag tags, out List<(Body collider, Contact[] contacts)> collidedColliders) {
            return Physics.Instance.QueryMultipleCollision(Shape, Position, tags, out collidedColliders);
        }

        public bool CollidesMultiple(Vector2 position, out List<(Body collider, Contact[] contacts)> collidedColliders) {
            return Physics.Instance.QueryMultipleCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collidedColliders);
        }

        public bool CollidesMultiple(out List<(Body collider, Contact[] contacts)> collidedColliders) {
            return CollidesMultiple(Position, out collidedColliders);
        }

        public bool CollidesMultiple<T>(Vector2 position, BitTag tags, out List<(T entity, Contact[] contacts)> collidedEntities) where T : Entity {
            return Physics.Instance.QueryMultipleCollision(Shape, position, tags, out collidedEntities);
        }

        public bool CollidesMultiple<T>(BitTag tags, out List<(T entity, Contact[] contacts)> collidedEntities) where T : Entity {
            return Physics.Instance.QueryMultipleCollision(Shape, Position, tags, out collidedEntities);
        }

        public bool CollidesMultiple<T>(Vector2 position, out List<(T entity, Contact[] contacts)> collidedEntities) where T : Entity {
            return Physics.Instance.QueryMultipleCollision(Shape, position, Physics.Instance.GetCollidableTags(Tags), out collidedEntities);
        }

        public bool CollidesMultiple<T>(out List<(T entity, Contact[] contacts)> collidedEntities) where T : Entity {
            return CollidesMultiple(Position, out collidedEntities);
        }

        #endregion Collides [Multiple Output]

        #endregion Public Methods
    }
}
