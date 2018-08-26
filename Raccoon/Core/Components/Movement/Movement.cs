using Raccoon.Util;

namespace Raccoon.Components {
    public abstract class Movement {
        #region Public Members

        public static readonly float UnitVelocity = Math.Ceiling(1f / Physics.FixedDeltaTimeSeconds);
        public const float MaxImpulsePerSecond = 60f * 5f;

        public System.Action OnMove = delegate { };

        #endregion Public Members

        #region Private Members

        private uint _axesSnap;

        #endregion Private Members

        #region Constructors

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
            Acceleration = MaxVelocity / (Util.Time.MiliToSec * timeToAchieveMaxVelocity);
        }

        #endregion Constructors

        #region Public Properties

        public Body Body { get; private set; }
        public BitTag CollisionTags { get; set; } = BitTag.None;
        public Vector2 Axis { get; set; }
        public Vector2 Velocity { get { return Body.Velocity; } set { Body.Velocity = value; } }
        public Vector2 MaxVelocity { get; set; }
        public Vector2 TargetVelocity { get; protected set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 LastAxis { get; protected set; }
        //public Vector2 Impulse { get; protected set; }
        public float DragForce { get; set; }
        public bool Enabled { get; set; } = true;
        public bool CanMove { get; set; } = true;
        public bool TouchedTop { get; private set; }
        public bool TouchedRight { get; private set; }
        public bool TouchedBottom { get; private set; }
        public bool TouchedLeft { get; private set; }

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

        #endregion Public Properties

        #region Protected Properties

        protected Vector2 NextAxis { get; set; }
        protected Vector2 ImpulsePerSec { get; set; }
        protected float ImpulseTime { get; set; }
        protected bool JustReceiveImpulse { get; set; }

        #endregion Protected Properties

        #region Public Methods

        public virtual void OnAdded(Body body) {
            Body = body;
        }

        public virtual void OnRemoved() {
            Body = null;
            OnMove = null;
        }

        public virtual void BeforeUpdate() {
        }

        public virtual void Update(int delta) {
            Axis = NextAxis;
            TargetVelocity = Axis * MaxVelocity;
            NextAxis = Vector2.Zero;
        }

        public virtual void LateUpdate() {
        }

        public virtual void DebugRender() {
        }
        
        public virtual void PhysicsUpdate(float dt) {
            TouchedTop = TouchedRight = TouchedBottom = TouchedLeft = false;
        }

        public virtual void PhysicsLateUpdate() {
            if (Body.LastPosition != Body.Position) {
                Vector2 posDiff = Body.Position - Body.LastPosition;
                if (posDiff.LengthSquared() > 0f) {
                    OnMoving(posDiff);
                }
            }
        }

        public virtual void OnCollide(Vector2 collisionAxes) {
            if (collisionAxes.Y < 0f) {
                TouchedTop = true;
            } else if (collisionAxes.Y > 0f) {
                TouchedBottom = true;
            }

            if (collisionAxes.X < 0f) {
                TouchedLeft = true;
            } else if (collisionAxes.X > 0f) {
                TouchedRight = true;
            }
        }

        public virtual void OnBodyCollide(Body otherBody, Vector2 collisionAxes) {
        }

        public abstract Vector2 Integrate(float dt);

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

        public void ApplyCustomImpulse(Vector2 distance, float duration) {
            // a = 2d / t²

            ImpulsePerSec = (2f * distance) / (duration * duration); // acceleration
            ImpulseTime = duration;
            JustReceiveImpulse = true;
        }

        public void ApplyCustomImpulse(Vector2 normal, float distance, float duration) {
            ApplyCustomImpulse(normal * distance, duration);
        }

        #endregion Public Methods

        #region Protected Methods

        /*public virtual Vector2 HandleForce(Vector2 force) {
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
        }*/


        protected abstract void OnMoving(Vector2 distance);

        #endregion Protected Methods
    }
}
