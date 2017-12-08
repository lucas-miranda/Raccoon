namespace Raccoon.Components {
    public abstract class Movement : Component {
        /// <summary>
        /// A component that handles movements, providing methods and properties to deal with speed.
        /// </summary>
        /// <param name="maxSpeed">Max horizontal and vertical speed. (in pixels/sec)</param>
        /// <param name="acceleration">Speed increase. (in pixels/sec)</param>
        /// <param name="body">Collider used to detect end of movement.</param>
        public Movement(Vector2 maxSpeed, Vector2 acceleration, Body body) {
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Body = body;
            IgnoreDebugRender = true;
        }

        public Body Body { get; private set; }
        public Vector2 LastPosition { get; protected set; }
        public Vector2 CurrentSpeed { get { return Util.Math.Clamp(Entity.Position - LastPosition, -MaxSpeed, MaxSpeed); } }
        public Vector2 MaxSpeed { get; set; }
        //public Vector2 TargetSpeed { get; protected set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 AccumulatedForce { get; protected set; }
        public Vector2 Axis { get; set; }
        public Vector2 LastAxis { get; protected set; }
        public float DragForce { get; set; } = .8f;
        public bool HorizontalAxisSnap { get; set; }
        public bool VerticalAxisSnap { get; set; }
        public bool AxesSnap { get { return HorizontalAxisSnap && VerticalAxisSnap; } set { HorizontalAxisSnap = VerticalAxisSnap = value; } }
        public bool CanMove { get; set; } = true;
        public bool Sleeping { get; protected set; }

        protected Vector2 NextAxis { get; set; }
        protected float MoveHorizontalBuffer { get; set; }
        protected float MoveVerticalBuffer { get; set; }

        public override void OnAdded(Entity entity) {
            base.OnAdded(entity);
            LastPosition = Entity.Position;

            // register collider, if isn't already registered
            if (Body.Entity != Entity) {
                Entity.AddComponent(Body);
            }

            Physics.Instance.AddMovement(this);
        }

        public override void OnRemoved() {
            base.OnRemoved();
            if (Body.Entity != null) {
                Entity.RemoveComponent(Body);
            }

            Physics.Instance.RemoveMovement(this);
        }

        public override void Update(int delta) {
            Axis = NextAxis;
            NextAxis = Vector2.Zero;
        }

        public override void Render() { }
        public override void DebugRender() { }

        /*public bool HasCollisionTag(string tag) {
            return CollisionTags.Contains(tag);
        }

        public bool HasCollisionTag(System.Enum tag) {
            return HasCollisionTag(tag.ToString());
        }

        public void AddCollisionTag(string tag) {
            if (HasCollisionTag(tag)) {
                return;
            }

            CollisionTags.Add(tag);
        }

        public void AddCollisionTag(System.Enum tag) {
            AddCollisionTag(tag.ToString());
        }

        public void RemoveCollisionTag(string tag) {
            CollisionTags.Remove(tag);
        }

        public void RemoveCollisionTag(System.Enum tag) {
            RemoveCollisionTag(tag.ToString());
        }

        public void ClearCollisionTags() {
            CollisionTags.Clear();
        }*/

        //public abstract void OnCollide(Vector2 moveDirection);
        public abstract void OnMoveUpdate(float dt);

        public void ApplyForce(Vector2 force) {
            if (!CanMove) {
                return;
            }

            AccumulatedForce += force / Body.Mass;
        }

        public virtual void Move(Vector2 axis) {
            if (!CanMove) {
                return;
            }

            axis = Util.Math.Clamp(axis, new Vector2(-1, -1), new Vector2(1, 1));
            if (axis != Vector2.Zero) {
                LastAxis = axis;
            }

            NextAxis = axis;
        }

        public virtual void Move(float x, float y) {
            Move(new Vector2(x, y));
        }

        public virtual void MoveHorizontal(float x) {
            Move(new Vector2(x, LastAxis.Y));
        }

        public virtual void MoveVertical(float y) {
            Move(new Vector2(LastAxis.X, y));
        }
    }
}
