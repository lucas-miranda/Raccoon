namespace Raccoon.Components {
    public abstract class Movement {
        public event System.Action OnMove = delegate { };

        private uint _axesSnap;

        /// <summary>
        /// A component that handles movements, providing methods and properties to deal with speed.
        /// </summary>
        /// <param name="acceleration">Speed increase.</param>
        public Movement(Vector2 acceleration) {
            Acceleration = acceleration;
        }

        /// <summary>
        /// A component that handles movements, providing methods and properties to deal with speed.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="acceleration">Speed increase.</param>
        public Movement(Vector2 maxVelocity, Vector2 acceleration) {
            MaxVelocity = maxVelocity;
            Acceleration = acceleration;
        }

        /// <summary>
        /// A component that handles movements, providing methods and properties to deal with speed.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="timeToAchieveMaxVelocity">Time (in miliseconds) to reach max velocity.</param>
        public Movement(Vector2 maxVelocity, int timeToAchieveMaxVelocity) {
            MaxVelocity = maxVelocity;
            Acceleration = MaxVelocity / (Util.Time.MiliToSec * timeToAchieveMaxVelocity / Physics.FixedDeltaTime);
        }

        public Body Body { get; set; }
        public Vector2 Axis { get; set; }
        public Vector2 Velocity { get { return Body.Velocity; } }
        public Vector2 MaxVelocity { get; set; }
        public Vector2 TargetVelocity { get; protected set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 LastAxis { get; protected set; }
        public float DragForce { get; set; } = .8f;
        public bool Enabled { get; set; } = true;
        public bool CanMove { get; set; } = true;

        public bool SnapHorizontalAxis {
            get {
                return Util.Bit.HasSet(ref _axesSnap, 0);
            }

            set {
                if (value) {
                    Util.Bit.Set(ref _axesSnap, 0);
                } else {
                    Util.Bit.Clear(ref _axesSnap, 0);
                }
            }
        }

        public bool SnapVerticalAxis {
            get {
                return Util.Bit.HasSet(ref _axesSnap, 1);
            }

            set {
                if (value) {
                    Util.Bit.Set(ref _axesSnap, 1);
                } else {
                    Util.Bit.Clear(ref _axesSnap, 1);
                }
            }
        }

        public bool SnapAxes {
            get {
                return Util.Bit.HasSet(ref _axesSnap, 0) && Util.Bit.HasSet(ref _axesSnap, 1);
            }

            set {
                if (value) {
                    Util.Bit.Set(ref _axesSnap, 0);
                    Util.Bit.Set(ref _axesSnap, 1);
                } else {
                    Util.Bit.Clear(ref _axesSnap, 0);
                    Util.Bit.Clear(ref _axesSnap, 1);
                }
            }
        }

#if DEBUG
        public bool IsDebugRenderEnabled { get; set; }
#endif

        protected Vector2 NextAxis { get; set; }

        public virtual void Update(int delta) {
            if (!Enabled) {
                return;
            }

            Axis = NextAxis;
            NextAxis = Vector2.Zero;
        }

        public virtual void DebugRender() { }

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

        public abstract Vector2 HandleVelocity(Vector2 velocity, float dt);

        public virtual void LateHandleVelocity(Vector2 velocity) {
            if (!Enabled || !CanMove) {
                return;
            }

            if (velocity.LengthSquared() >= 0) {
                OnMove();
            }
        }

        public virtual Vector2 HandleForce(Vector2 force) {
            if (!Enabled || !CanMove) {
                return Vector2.Zero;
            }

            return force;
        }

        public virtual Vector2 HandleImpulse(Vector2 impulse) {
            if (!Enabled || !CanMove) {
                return Vector2.Zero;
            }

            return impulse;
        }

        public virtual void OnCollide() {
        }

        public virtual void Move(Vector2 axis) {
            if (!Enabled || !CanMove) {
                return;
            }

            axis = Util.Math.Clamp(axis, -Vector2.One, Vector2.One);
            if (axis != Vector2.Zero) {
                LastAxis = axis;
            }

            NextAxis = axis;
        }

        public void Move(float x, float y) {
            Move(new Vector2(x, y));
        }

        public void MoveHorizontal(float x) {
            Move(new Vector2(x, LastAxis.Y));
        }

        public void MoveVertical(float y) {
            Move(new Vector2(LastAxis.X, y));
        }
    }
}
