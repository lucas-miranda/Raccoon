using System.Collections.ObjectModel;
using Raccoon.Util;

namespace Raccoon.Components {
    public abstract class Movement : System.IDisposable {
        #region Public Members

        public static readonly float UnitVelocity = Math.Ceiling(1f / Physics.FixedDeltaTimeSeconds);
        public const float MaxImpulsePerSecond = 60f * 5f;

        public delegate void MovementAction();
        public MovementAction OnMove = delegate { };

        #endregion Public Members

        #region Private Members

        private uint _axesSnap;
        private Vector2 _maxVelocity, _acceleration;

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

        ~Movement() {
            Dispose();
        }

        #endregion Constructors

        #region Public Properties

        public Body Body { get; private set; }
        public BitTag Tags { get { return CollisionTags | ExtraCollisionTags; } }
        public BitTag CollisionTags { get; set; } = BitTag.None;
        public Vector2 Axis { get; set; }
        public Vector2 Velocity { get { return Body.Velocity; } set { Body.Velocity = value; } }
        public Vector2 MaxVelocity { get { return _maxVelocity + ExtraMaxVelocity + BonusMaxVelocity; } set { _maxVelocity = value; } }
        public Vector2 BaseMaxVelocity { get { return _maxVelocity; } }
        public Vector2 BonusMaxVelocity { get; set; }
        public Vector2 ExtraMaxVelocity { get; protected set; }
        public Vector2 TargetVelocity { get; protected set; }
        public Vector2 Acceleration { get { return _acceleration + ExtraAcceleration + BonusAcceleration; } set { _acceleration = value; } }
        public Vector2 BaseAcceleration { get { return _acceleration; } }
        public Vector2 BonusAcceleration { get; set; }
        public Vector2 ExtraAcceleration { get; protected set; }
        public Vector2 LastAxis { get; protected set; }
        //public Vector2 Impulse { get; protected set; }
        public float DragForce { get; set; }
        public bool Enabled { get; set; } = true;
        public bool CanMove { get; set; } = true;
        public bool IsDisposed { get; private set; }
        /*public bool TouchedTop { get; private set; }
        public bool TouchedRight { get; private set; }
        public bool TouchedBottom { get; private set; }
        public bool TouchedLeft { get; private set; }*/

        public bool SnapHorizontalAxis {
            get {
                return Bit.HasSet(ref _axesSnap, 0);
            }

            set {
                if (value) {
                    Bit.Set(ref _axesSnap, 0);
                } else {
                    Bit.Clear(ref _axesSnap, 0);
                }
            }
        }

        public bool SnapVerticalAxis {
            get {
                return Bit.HasSet(ref _axesSnap, 1);
            }

            set {
                if (value) {
                    Bit.Set(ref _axesSnap, 1);
                } else {
                    Bit.Clear(ref _axesSnap, 1);
                }
            }
        }

        public bool SnapAxes {
            get {
                return Bit.HasSet(ref _axesSnap, 0) && Bit.HasSet(ref _axesSnap, 1);
            }

            set {
                if (value) {
                    Bit.Set(ref _axesSnap, 0);
                    Bit.Set(ref _axesSnap, 1);
                } else {
                    Bit.Clear(ref _axesSnap, 0);
                    Bit.Clear(ref _axesSnap, 1);
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
        protected BitTag ExtraCollisionTags { get; set; } = BitTag.None;

        #endregion Protected Properties

        #region Public Methods

        public virtual void OnAdded(Body body) {
            Body = body;
        }

        public virtual void OnRemoved() {
            Body = null;
        }

        public virtual void BeforeUpdate() {
            Axis = NextAxis;
            TargetVelocity = Axis * MaxVelocity;
            NextAxis = Vector2.Zero;
        }

        public virtual void Update(int delta) {
        }

        public virtual void LateUpdate() {
        }

        public virtual void DebugRender() {
        }

        public virtual void PhysicsUpdate(float dt) {
            //ResetTouch();
        }

        public virtual void PhysicsCollisionSubmit(Body otherBody, Vector2 movement, ReadOnlyCollection<Contact> horizontalContacts, ReadOnlyCollection<Contact> verticalContacts) {
        }

        public virtual void PhysicsLateUpdate() {
            Vector2 posDiff = Body.Position - Body.LastPosition;
            if (posDiff.LengthSquared() > 0f) {
                OnMoving(posDiff);
            }
        }

        public virtual bool CanCollideWith(Vector2 collisionAxes, CollisionInfo<Body> collisionInfo) {
            return true;
        }

        public virtual void BeforeBodySolveCollisions() {
        }

        public virtual void AfterBodySolveCollisions() {
        }

        public virtual void BeginBodyCollision(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            //CheckTouch(otherBody);
        }

        public virtual void BodyCollided(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            //CheckTouch(otherBody);
        }

        public virtual void EndBodyCollision(Body otherBody) {
            //CheckUntouch(otherBody);
        }

        public virtual void BeginCollision(Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
        }

        public virtual void Collided(Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
        }

        public virtual void EndCollision() {
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

        public virtual void Dispose() {
            if (IsDisposed) {
                return;
            }

            Body = null;
            OnMove = null;

            IsDisposed = true;
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

        #region Private Methods

        /*private void ResetTouch() {
            TouchedTop = TouchedRight = TouchedBottom = TouchedLeft = false;
        }

        private void CheckTouch(Body body) {
            if (!Physics.Instance.CheckCollision(Body, body, out Contact[] contacts)) {
                return;
            }


        }

        private void CheckUntouch(Body body) {
        }*/

        #endregion Private Methods
    }
}
