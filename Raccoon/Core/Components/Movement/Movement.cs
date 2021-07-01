using System.Collections.ObjectModel;
using Raccoon.Util;

namespace Raccoon.Components {
    public abstract class Movement : System.IDisposable {
        #region Public Members

        public static readonly float UnitVelocity = Math.Ceiling(1f / Physics.FixedDeltaTimeSeconds);
        public const float MaxImpulsePerSecond = 60f * 5f;

        public delegate void MovementAction(Vector2 distance);
        public MovementAction OnMove;

        public delegate void MovementEventAction();
        public MovementEventAction OnStartMove,
                                   OnStopMove;
        
        public delegate void PhysicsAction();
        public PhysicsAction OnPhysicsLateUpdate = delegate { };

        public delegate void PhysicsUpdateAction(float dt);
        public PhysicsUpdateAction OnPhysicsUpdate = delegate { };

        // force
        public delegate void ForceMovementAction(Vector2 force, float duration);
        public ForceMovementAction OnReceiveForce;
        public PhysicsAction OnForceEnds;

        // impulse
        public delegate void ImpulseMovementAction(Vector2 impulse);
        public ImpulseMovementAction OnReceiveImpulse;

        #endregion Public Members

        #region Private Members

        private uint _axesSnap;
        private Vector2 _axis,
                        _maxVelocity, _acceleration;

        #endregion Private Members

        #region Constructors

        protected Movement() {
        }

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
        public Movement(Vector2 maxVelocity, uint timeToAchieveMaxVelocity) {
            MaxVelocity = maxVelocity;
            Acceleration = MaxVelocity / (Util.Time.MiliToSec * timeToAchieveMaxVelocity);
        }

        ~Movement() {
        }

        #endregion Constructors

        #region Public Properties

        public Body Body { get; private set; }
        public BitTag Tags { get { return CollisionTags | ExtraCollisionTags; } }
        public BitTag CollisionTags { get; set; } = BitTag.None;
        public Vector2 Velocity { get { return Body.Velocity; } set { Body.Velocity = value; } }
        public Vector2 MaxVelocity { get { return _maxVelocity + ExtraMaxVelocity + BonusMaxVelocity; } set { _maxVelocity = value; } }
        public Vector2 BaseMaxVelocity { get { return _maxVelocity; } }
        public Vector2 BonusMaxVelocity { get; set; }
        public Vector2 ExtraMaxVelocity { get; protected set; }
        public Vector2 Acceleration { get { return _acceleration + ExtraAcceleration + BonusAcceleration; } set { _acceleration = value; } }
        public Vector2 BaseAcceleration { get { return _acceleration; } }
        public Vector2 BonusAcceleration { get; set; }
        public Vector2 ExtraAcceleration { get; protected set; }
        public Vector2 TargetVelocity { get; protected set; }
        public Vector2 LastAxis { get; protected set; }
        //public Vector2 Impulse { get; protected set; }
        public float DragForce { get; set; } = 1f;
        public Vector2? MoveTargetPosition { get; protected set; }
        public bool Enabled { get; set; } = true;
        public bool CanMove { get; set; } = true;
        public bool IsMoving { get; private set; } = false;
        public bool IsDisposed { get; private set; }
        /*public bool TouchedTop { get; private set; }
        public bool TouchedRight { get; private set; }
        public bool TouchedBottom { get; private set; }
        public bool TouchedLeft { get; private set; }*/
        public Vector2 ForcePerSec { get; protected set; }
        public float ForceDuration { get; protected set; }
        public bool JustReceiveForce { get; protected set; }
        public bool JustReceiveImpulse { get; protected set; }
        public bool IsReceivingForce { get; protected set; }
        public bool IsMovingToTargetPosition { get { return MoveTargetPosition.HasValue; } }
        public bool UseSmoothStopWithMovingToTarget { get; private set; }

        public Vector2 Axis {
            get {
                return _axis;
            }

            protected set {
                _axis = value;

                if (_axis != Vector2.Zero) {
                    LastAxis = _axis;
                }

                TargetVelocity = _axis * MaxVelocity;
            }
        }


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
            NextAxis = Vector2.Zero;
        }

        public virtual void Update(int delta) {
        }

        public virtual void LateUpdate() {
        }

        public virtual void DebugRender() {
        }

        public virtual void PhysicsUpdate(float dt) {
            OnPhysicsUpdate(dt);
        }

        public virtual void PhysicsCollisionSubmit(Body otherBody, Vector2 movement, ReadOnlyCollection<Contact> horizontalContacts, ReadOnlyCollection<Contact> verticalContacts) {
        }

        public virtual void PhysicsStepMove(int movementX, int movementY) {
        }

        public virtual void PhysicsLateUpdate(float dt) {
            OnPhysicsLateUpdate();

            if (!Math.EqualsEstimate(ForceDuration, 0f)) {
                ForceDuration = Math.Approach(ForceDuration, 0f, dt);
                if (Math.EqualsEstimate(ForceDuration, 0f)) {
                    ForcePerSec = Vector2.Zero;
                    JustReceiveForce = IsReceivingForce = false;
                    ForceEnds();
                    OnForceEnds?.Invoke();
                }
            }

            Vector2 posDiff = Body.Position - Body.LastPosition;
            if (posDiff.LengthSquared() > 0f) {
                if (!IsMoving) {
                    OnStartMove?.Invoke();
                }

                IsMoving = true;
                OnMoving(posDiff);
            } else if (IsMoving) {
                IsMoving = false;
                OnStopMove?.Invoke();
            }
        }

        public virtual bool CanExecuteMove(int movementX, int movementY) {
            return true;
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

            NextAxis = Util.Math.Clamp(axis, -Vector2.One, Vector2.One);
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

        public virtual void MoveTo(Vector2 position, bool smoothStop = true) {
            MoveTargetPosition = position;
            UseSmoothStopWithMovingToTarget = smoothStop;
        }

        public void ApplyImpulse(Vector2 impulse) {
            Velocity += impulse;
            JustReceiveImpulse = true;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyImpulse(Vector2 distance, float duration) {
            if (Math.EqualsEstimate(duration, 0f)) {
                return;
            }

            Vector2 impulse = distance / duration;
            Velocity += impulse;
            JustReceiveImpulse = true;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyImpulse(Vector2 distance, float duration, out Vector2 impulse) {
            if (Math.EqualsEstimate(duration, 0f)) {
                impulse = default;
                return;
            }

            impulse = distance / duration;
            Velocity += impulse;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyImpulse(float distance, Vector2 direction, float duration) {
            if (Math.EqualsEstimate(duration, 0f)) {
                return;
            }

            Vector2 impulse = direction * (distance / duration);
            Velocity += impulse;
            JustReceiveImpulse = true;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyImpulse(float distance, Vector2 direction, float duration, out Vector2 impulse) {
            if (Math.EqualsEstimate(duration, 0f)) {
                impulse = default;
                return;
            }

            impulse = direction * (distance / duration);
            Velocity += impulse;
            JustReceiveImpulse = true;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyPreciseImpulse(float distance, Vector2 direction, float dragForce) {
            Vector2 impulse = direction * Math.Ceiling(Math.Sqrt(Math.Abs(2f * dragForce * distance)));
            Velocity += impulse;
            JustReceiveImpulse = true;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyPreciseImpulse(float distance, Vector2 direction, float dragForce, out Vector2 impulse) {
            impulse = direction * Math.Ceiling(Math.Sqrt(Math.Abs(2f * dragForce * distance)));
            Velocity += impulse;
            JustReceiveImpulse = true;
            OnReceiveImpulse?.Invoke(impulse);
        }

        public void ApplyForce(Vector2 force, float duration) {
            ForcePerSec = ((2f * force) / (duration * duration)); // acceleration
            ForceDuration = duration;

            JustReceiveForce = IsReceivingForce = true;
            OnReceiveForce?.Invoke(ForcePerSec, ForceDuration);
        }

        public void ApplyForce(Vector2 normal, float distance, float duration) {
            ApplyForce(normal * distance, duration);
        }

        public void FullStop() {
            NextAxis = Vector2.Zero;
            Body.Velocity = Vector2.Zero;

            // impulse
            ForcePerSec = Vector2.Zero;
            ForceDuration = 0f;
        }

        public void CancelMoveToTargetPosition() {
            MoveTargetPosition = null;
        }

        public virtual void Dispose() {
            if (IsDisposed) {
                return;
            }

            Body = null;
            OnMove = null;
            OnStopMove = null;
            OnPhysicsUpdate = null;
            OnPhysicsLateUpdate = null;
            OnReceiveForce = null;
            OnReceiveImpulse = null;

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected abstract void OnMoving(Vector2 distance);

        protected virtual void ForceEnds() {
        }

        protected float CalculateAcceleration(float speed, float targetSpeed, float dt, float acceleration) {
            if (Math.EqualsEstimate(speed, targetSpeed)) {
                return 0f;
            }

            if (targetSpeed > speed) {
                if (speed + acceleration * dt <= targetSpeed) {
                    return acceleration;
                }
            } else {
                if (speed - acceleration * dt >= targetSpeed) {
                    return -acceleration;
                }
            }

            return (targetSpeed - speed) / dt;
        }

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
