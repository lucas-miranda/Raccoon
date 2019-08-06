//#define DISBALE_RAMPS
#define DISABLE_ASCENDING_RAMP
//#define DISABLE_DESCENDING_RAMP

using System.Collections.ObjectModel;
using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        #region Public Members

        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = 200;       // milliseconds
        public static uint JumpInputBufferTime = 200;   // milliseconds

        public event System.Action OnJumpBegin = delegate { },
                                   OnTouchGround = delegate { },
                                   OnFallingBegin = delegate { };

        #endregion Public Members

        #region Private Members

        private static readonly Vector2 AscendingRampCollisionCheckCorrection = new Vector2(0f, -.25f);

        private static readonly string DebugText = @"
Axes
  Current: {0}
  Last: {1}
  Snap H: {2} V: {3}

Velocity
  Current: {4}
  Max: {5}   Target: {6}
  Acceleration: {7}

Force: {8}
Gravity Force: {9}

Enabled? {10}
Can Move? {11}

OnGround? {12}
IsFalling? {13}

Jump
  Jumps: {14}
  Can Jump? {15}
  Is Jumping? {16}
  Just Jumped? {17}
  Is Still Jumping? {18}
  Height: {19}

  can keep current jump? {20}
  jump max y: {21}

Ramps
  is Walking On Ramp? {22}

Fall Through
  Can Fall Through? {23}

  is trying to fall through? {24}
  apply fall? {25}
";

        // general
        
        /// <summary>
        /// Some systems (sunch as ramp climbing) need to temporarily disable gravity in order to work properly.
        /// </summary>
        private bool _internal_isGravityEnabled = true;

        private bool _touchedBottom, _touchedTop;

        // jump
        private bool _canJump = true, 
                     _canKeepCurrentJump = true, 
                     _requestedJump;

        private int _jumpMaxY, _ledgeJumpTime;
        private uint _lastTimeFirstRequestToJump;

        // ramp movement
        private bool _onRamp; // *
        private Vector2 _rampNormal; // *
        private int _rampFindTries = -1; // *
        private int _previousRampFound = 0; // * // 0 = plain ground; 1 = ascending ramp; -1 = descending ramp
        private bool _isWalkingOnRamp;

        // fall through
        private bool _isTryingToFallThrough, _applyFall, _isAboveSomething;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// A component that handles platformer movement.
        /// </summary>
        /// <param name="maxHorizontalVelocity">Max horizontal velocity.</param>
        /// <param name="horizontalAcceleration">Horizontal speed increase.</param>
        public PlatformerMovement(float maxHorizontalVelocity, float horizontalAcceleration, float jumpHeight, float jumpAcceleration) : base(new Vector2(maxHorizontalVelocity, 0), new Vector2(horizontalAcceleration, jumpAcceleration)) {
            SnapHorizontalAxis = true;
            JumpHeight = jumpHeight;
            OnFallingBegin += () => {
                _ledgeJumpTime = 0;
            };
        }

        /// <summary>
        /// A component that handles platformer movement.
        /// </summary>
        /// <param name="maxHorizontalVelocity">Max horizontal velocity.</param>
        /// <param name="timeToAchieveMaxVelocity">Time (in miliseconds) to reach max velocity.</param>
        public PlatformerMovement(float maxHorizontalVelocity, int timeToAchieveMaxVelocity, float jumpHeight, float jumpAcceleration) : this(maxHorizontalVelocity, maxHorizontalVelocity / (Time.MiliToSec * timeToAchieveMaxVelocity), jumpHeight, jumpAcceleration) {
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// If it's resting on top of something.
        /// </summary>
        public bool OnGround { get; protected set; }

        /// <summary>
        /// If it's doing any vertical movement, basically not resting on top of something.
        /// </summary>
        public bool OnAir { get { return !OnGround; } }

        /// <summary>
        /// Can make a jump by having jumps left and is able to.
        /// </summary>
        public bool CanJump { get { return _canJump && Jumps > 0; } set { _canJump = value; } }

        /// <summary>
        /// Holds True from leaving the ground, by jump, until before start falling.
        /// </summary>
        public bool IsJumping { get; protected set; }

        /// <summary>
        /// If it's moving down, falling from a jump or from a higher place.
        /// </summary>
        public bool IsFalling { get; protected set; } = true;

        /// <summary>
        /// Can continuously jump while calling Jump() or has to wait an update without calling it to jump again.
        /// </summary>
        public bool CanContinuousJump { get; set; } = false;

        /// <summary>
        /// If jump happened just now, set to false after an update.
        /// </summary>
        public bool JustJumped { get; private set; }

        /// <summary>
        /// How many jumps can happen until reaches ground again.
        /// </summary>
        public int MaxJumps { get; set; } = 1;

        /// <summary>
        /// How many jumps are still left.
        /// </summary>
        public int Jumps { get; protected set; }

        /// <summary>
        /// Jump max height (in pixels).
        /// </summary>
        public float JumpHeight { get; private set; }

        /// <summary>
        /// Rate of explosion velocity when jump, based on vertical acceleration.
        /// </summary>
        public float JumpExplosionRate { get; set; } = .5f;

        /// <summary>
        /// Scale of gravity to apply.
        /// </summary>
        public float GravityScale { get; set; } = 1f;

        /// <summary>
        /// Drag force applied when on ground.
        /// </summary>
        public float GroundDragForce { get; set; }

        /// <summary>
        /// Drag force applied when on air.
        /// </summary>
        public float AirDragForce { get; set; }

        /// <summary>
        /// Tags to check using fall through platform logic.
        /// </summary>
        public BitTag FallThroughTags { get; set; }

        /// <summary>
        /// It's on fall through state.
        /// </summary>
        public bool CanFallThrough { get; private set; }

        /// <summary>
        /// Can gravity act.
        /// </summary>
        public bool GravityEnabled { get; set; } = true;

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// If jump condition is renewed.
        /// </summary>
        protected bool IsStillJumping { get; set; }

        #endregion Protected Properties

        #region Public Methods

        public override void BeforeUpdate() {
            base.BeforeUpdate();
            if (!CanContinuousJump) {
                if (!_canKeepCurrentJump && !_requestedJump) {
                    _lastTimeFirstRequestToJump = 0;

                    // continuous jump lock (must release and press jump button to jump again)
                    if (Jumps > 0 || OnGround) {
                        _canKeepCurrentJump = true;
                    }
                }

                _requestedJump = false;
            }

            IsStillJumping = false;
            CanFallThrough = false;
        }

        public override void Update(int delta) {
            base.Update(delta);

            if (IsFalling && _ledgeJumpTime <= LedgeJumpMaxTime) {
                _ledgeJumpTime += delta;
            }
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = string.Format(
                DebugText,
                Axis, LastAxis, SnapHorizontalAxis, SnapVerticalAxis,
                Velocity, MaxVelocity, TargetVelocity, Acceleration,
                Body.Force, GravityForce * GravityScale,
                Enabled, CanMove,
                OnGround, IsFalling,
                Jumps, CanJump, IsJumping, JustJumped, IsStillJumping, JumpHeight, _canKeepCurrentJump, _jumpMaxY,
                _isWalkingOnRamp,
                CanFallThrough, _isTryingToFallThrough, _applyFall
            );

            Debug.DrawString(null, new Vector2(10f, 10f), info);
            Debug.DrawLine(
                new Vector2(Body.Position.X - 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f),
                new Vector2(Body.Position.X + 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f),
                Graphics.Color.Yellow
            );

            //Debug.DrawString(Debug.Transform(Body.Position - new Vector2(16)), $"Impulse Time: {ImpulseTime}\n(I/s: {ImpulsePerSec})");
        }

        public override void PhysicsUpdate(float dt) {
            base.PhysicsUpdate(dt);
            _touchedTop = _touchedBottom = false;

            if (Body.Shape == null) {
                return;
            }

            /*
            if (GravityEnabled) {
                // check if is still on ground
                if (OnGround && !IsJumping
                  && (!Physics.Instance.QueryMultipleCollision(Body.Shape, Body.Position + Vector2.Down, CollisionTags, out CollisionList<Body> collisions)
                  || !collisions.Contains(ci => ci.Contacts.Contains(c => c.PenetrationDepth > 0f && Helper.InRangeLeftExclusive(Vector2.Dot(c.Normal, Vector2.Down), .3f, 1f))))) {
                    Fall();
                }
            }
            */

            //_isTryingToFallThrough = true;
            //_applyFall = _isAboveSomething = false;
        }

        public override void PhysicsCollisionSubmit(Body otherBody, Vector2 movement, ReadOnlyCollection<Contact> horizontalContacts, ReadOnlyCollection<Contact> verticalContacts) {
            base.PhysicsCollisionSubmit(otherBody, movement, horizontalContacts, verticalContacts);

            if (!otherBody.Tags.HasAny(CollisionTags)) {
                return;
            }


            /*
            // elevations
            //   nearest to 90f => wall; 
            //   close to 0f => straight floor;
            float minElevation = 0f,  // in degrees
                  maxElevation = 60f; // in degrees

            float minSlopeFactor = Math.Cos(maxElevation),
                  maxSlopeFactor = Math.Cos(minElevation);

            Debug.WriteLine($"\nPlatformerMovement - PhysicsCollisionSubmit");
            Debug.WriteLine($"movement: {movement}");
            Debug.WriteLine($"horizontalContacts: [{string.Join(", ", horizontalContacts)}]");
            Debug.WriteLine($"verticalContacts: [{string.Join(", ", verticalContacts)}]");

            bool refreshRampState = false;

            foreach (Contact c in horizontalContacts) {
                if (Math.EqualsEstimate(Math.Abs(Vector2.Dot(c.Normal, Vector2.Right)), 1f)
                  || Math.EqualsEstimate(Math.Abs(Vector2.Dot(c.Normal, Vector2.Down)), 1f)) {
                    // ignore most of contacts
                    continue;
                }

                float horizontalSide = Math.Sign(c.Normal.X),
                      slopeFactor = Math.Abs(c.Normal.X);

                Debug.WriteLine($"side: {horizontalSide}, slope: {slopeFactor}");

                if (Util.Helper.InRange(slopeFactor, minSlopeFactor, maxSlopeFactor)) {
                    Debug.WriteLine($"slope in [{minSlopeFactor}, {maxSlopeFactor}] found!");
                    refreshRampState = true;
                    _onRamp = true;
                    _internal_isGravityEnabled = false;

                    Vector2 foundRampNormal = -c.Normal;
                    if (foundRampNormal != _rampNormal) {
                        _rampNormal = foundRampNormal;
                        break;
                    }
                }
            }

            if (!refreshRampState) {
                Debug.WriteLine($"slope don't found");
                _onRamp = false;
                _rampNormal = Vector2.Zero;
                _internal_isGravityEnabled = true;
            }
            */

            /*
            foreach (Contact contact in verticalContacts) {
                if (!_touchedTop && Vector2.Dot(contact.Normal, Vector2.Up) > .6f) {
                    _touchedTop = true;
                } else if (!_touchedBottom && Vector2.Dot(contact.Normal, Vector2.Down) > .6f) {
                    _touchedBottom = true;
                }
            }

            Debug.WriteLine($"touched (top: {_touchedTop}, bottom: {_touchedBottom})");
            */
        }

        public override void PhysicsLateUpdate() {
            base.PhysicsLateUpdate();

            bool isAboveSomething = Body.CollidesMultiple(Body.Position + Vector2.Down, CollisionTags, out CollisionList<Body> contactsBelow);
            bool isBelowSomething = Body.CollidesMultiple(Body.Position + Vector2.Up, CollisionTags, out CollisionList<Body> contactsAbove);

            /*
            Debug.WriteLine($"\nPlatformerMovement - PhysicsLateUpdate");
            Debug.WriteLine($"c below: {contactsBelow}");
            Debug.WriteLine($"c above: {contactsAbove}");
            */

            if (isAboveSomething) {
                if (_onRamp) {
                    _touchedBottom = contactsBelow.FindIndex(ci => ci.Contacts.Contains(c => Vector2.Dot(c.Normal, Vector2.Down) > .6f && Helper.InRange(c.PenetrationDepth, 0f, 1f))) >= 0;
                } else {
                    _touchedBottom = contactsBelow.FindIndex(ci => ci.Contacts.Contains(c => Vector2.Dot(c.Normal, Vector2.Down) > .6f && Math.EqualsEstimate(c.PenetrationDepth, 1f))) >= 0;
                }
            }

            if (isBelowSomething) {
                _touchedTop = contactsAbove.FindIndex(ci => ci.Contacts.Contains(c => Vector2.Dot(c.Normal, Vector2.Up) > .6f && Helper.InRange(c.PenetrationDepth, 0f, 1f))) >= 0;
            }

            Debug.WriteLine($"touched bottom: {_touchedBottom}, top: {_touchedTop}");

            /*
            if (_isTryingToFallThrough && _applyFall) {
                Fall();
            }
            */

            if (_touchedTop) {
                // moving up and reached a ceiling
                if (Velocity.Y < 0f) { // && !_isTryingToFallThrough) {
                    IsJumping = _canKeepCurrentJump = false;
                    Velocity = new Vector2(Velocity.X, 0f);
                    Debug.WriteLine($"PhysicsLateUpdate - moving up and reached a ceiling, Velocity.Y = 0 and begin falling");

                    IsFalling = true;
                    OnFallingBegin();
                }
            }

            if (_touchedBottom) {
                //if (!_isTryingToFallThrough) {
                // falling and reached the ground
                if (IsFalling) {
                    OnGround = true;
                    IsStillJumping = IsJumping = IsFalling = false;
                    Jumps = MaxJumps;

                    if (!CanContinuousJump && _requestedJump && Body.Entity.Timer - _lastTimeFirstRequestToJump <= JumpInputBufferTime) {
                        _canKeepCurrentJump = true;
                    }

                    OnTouchGround();
                }
                //}

                if (OnGround || IsFalling) {
                    Velocity = new Vector2(Velocity.X, 0f);
                    Debug.WriteLine($"PhysicsLateUpdate - touchedBottom and (OnGround or IsFalling) => Velocity.Y = 0");
                }
            }

            // Check if still is on ground
            if (OnGround && !_touchedBottom) {
                Debug.WriteLine($"PhysicsLateUpdate - Not in ground anymore, falling");
                Fall();
            }
        }

        public override bool CanCollideWith(Vector2 collisionAxes, CollisionInfo<Body> collisionInfo) {
            //base.CanCollideWith(collisionAxes, collisionInfo);

            /*
            if (!collisionInfo.Subject.Tags.HasAny(FallThroughTags)) {
                return true;
            }

            if (collisionAxes.Y > 0 && collisionInfo.Contacts.Contains(c => c.PenetrationDepth == 1f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .4f, 1f))) {
                if (CanFallThrough) {
                    _applyFall = true;
                    return false;
                }
            } else {
                return false;
            }
            */

            return true;
        }

        public override void BodyCollided(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            base.BodyCollided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

            /*
            if (!_isTryingToFallThrough) {
                return;
            }

            if (!otherBody.Tags.HasAny(FallThroughTags)
              && vCollisionInfo != null && vCollisionInfo.Contacts.Contains(c => c.PenetrationDepth > 0f || Helper.InRangeLeftExclusive(Vector2.Dot(c.Normal, Vector2.Up), 0f, 1f))) {
                if (vCollisionInfo != null) {
                    _isTryingToFallThrough = false;
                }

                _applyFall = false;
                return;
            }

            if (vCollisionInfo == null) {
                vCollisionInfo = hCollisionInfo;
            }

            if (collisionAxes.Y > 0) {
                if (vCollisionInfo.Contacts.Contains(c => c.PenetrationDepth == 1f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .4f, 1f))) {
                    if (CanFallThrough) {
                        _applyFall = true;
                    } else if (_isTryingToFallThrough) {
                        _isTryingToFallThrough = false;
                    }
                }
            } else if (collisionAxes.Y > 0) {
                if (vCollisionInfo.Contacts.Contains(c => c.PenetrationDepth == 0f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .4f, 1f))) {
                    _isAboveSomething = true;
                    _applyFall = false;
                }

                if (!_isAboveSomething && (otherBody.Shape is GridShape || Body.Bottom > otherBody.Top)) {
                    _applyFall = true;
                }
            } else {
            }
            */
        }

        public override void Collided(Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            base.Collided(collisionAxes, hCollisionInfo, vCollisionInfo);
        }

        public override Vector2 Integrate(float dt) {
            Vector2 displacement = Vector2.Zero, // in pixels
                    velocity = Velocity; // in pixels/second

            /////////////////////////
            // Horizontal Velocity //
            /////////////////////////

            if (!Math.EqualsEstimate(ImpulseTime, 0f) && ImpulsePerSec.X != 0f) {
                // handling impulse
                velocity.X += ImpulsePerSec.X * dt;
            } else if (Axis.X == 0f) {
                // horizontal axis is resting

                if (Math.EqualsEstimate(velocity.X, 0f)) {
                    velocity.X = 0f;
                } else if (OnGround && !JustReceiveImpulse) {
                    // ground drag force
                    velocity.X *= (1f - DragForce) * (1f - GroundDragForce);
                } else {
                    // air drag force
                    float airDragForce = AirDragForce,
                          impulseSpeedCutoff = 0f;

                    if (JustReceiveImpulse) {
                        if (OnAir) {
                            impulseSpeedCutoff = 5f;
                            airDragForce /= 2f;
                        } else { // on ground
                            impulseSpeedCutoff = MaxVelocity.X / 2f;
                        }
                    }

                    // air drag force
                    velocity.X *= (1f - DragForce) * (1f - airDragForce);

                    if (Math.Abs(velocity.X) < impulseSpeedCutoff) {
                        JustReceiveImpulse = false;
                    }
                }
            } else if (SnapHorizontalAxis && velocity.X != 0f && Math.Sign(Axis.X) != Math.Sign(velocity.X)) {
                // snaps horizontal velocity to zero, if horizontal axis is on opposite direction
                velocity.X = 0f;
            } else if (MaxVelocity.X > 0f) {
                // velocity increasing until reach MaxVelocity.X limit
                velocity.X = Math.Approach(velocity.X, TargetVelocity.X, Acceleration.X * dt);
            } else {
                // velocity increasing without a limit
                velocity.X += Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            velocity.X += Body.Force.X * dt;
            displacement.X = velocity.X * dt;

            ///////////////////////
            // Vertical Velocity //
            ///////////////////////

#if DISABLE_RAMPS
            bool isWalkingOnRamp = false;
#else
            bool isWalkingOnRamp = HandleRamps(displacement, out Vector2 rampDisplacement);

            if (isWalkingOnRamp) {
                displacement = rampDisplacement;
            }
#endif

            if (!Math.EqualsEstimate(ImpulseTime, 0f) && ImpulsePerSec.Y != 0f) {
                // handling impulse
                velocity.Y += ImpulsePerSec.Y * dt;
            }

            //if (!OnGround && GravityEnabled) {
            if (GravityEnabled && _internal_isGravityEnabled) {
                // apply gravity force
                velocity.Y += GravityScale * GravityForce.Y * dt;
            }

            if (IsStillJumping) {
                // apply jumping acceleration if it's jumping
                velocity.Y -= Acceleration.Y * dt;
            }

            velocity.Y += Body.Force.Y * dt;

            if (!isWalkingOnRamp) {
                displacement.Y = velocity.Y * dt;
            }

            if (!Math.EqualsEstimate(ImpulseTime, 0f)) {
                ImpulseTime = Math.Approach(ImpulseTime, 0f, dt);
            }

            Velocity = velocity;
            return displacement;
        }

        public void Jump() {
            // continuous jump lock (must release and press jump button to jump again)
            _requestedJump = true;
            if (_lastTimeFirstRequestToJump == 0) {
                _lastTimeFirstRequestToJump = Body.Entity.Timer;
            }

            if (!_canKeepCurrentJump) {
                return;
            }

            // keep going up, if you not reach the max jump height
            if (IsJumping) {
                IsStillJumping = true;
                return;
            }

            // checks if can jump and ledge jump time
            if (!CanJump || (!OnGround && _ledgeJumpTime > LedgeJumpMaxTime)) {
                return;
            }

            /*
            Debug.WriteLine("JUMP!!!");
            Debug.WriteLine("JUMP!!!");
            Debug.WriteLine("JUMP!!!");
            */

            IsStillJumping = true;
            _isWalkingOnRamp = false;
            _jumpMaxY = (int) (Body.Position.Y - JumpHeight);
            Velocity = new Vector2(Velocity.X, -(Acceleration.Y * JumpExplosionRate));
        }

        public void FallThrough() {
            CanFallThrough = true;

            if (OnGround) {
                Velocity = new Vector2(Velocity.X, Acceleration.Y * JumpExplosionRate);
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            if (distance.Y > 0f) {
                // if it's moving down then it's falling
                if (IsJumping && !IsFalling) { // && !_isWalkingOnRamp) {
                    Debug.WriteLine($"OnMoving with distance.Y > 0 - if it's moving down, then it's falling");
                    Fall();
                }
            } else if (distance.Y < 0f) {
                if (IsStillJumping) {
                    if (!IsJumping) {
                        IsJumping = JustJumped = true;
                        OnGround = IsFalling = false;
                        Jumps--;
                        OnJumpBegin();
                    } else if (JustJumped) {
                        JustJumped = false;
                    }

                    // checks if jump max distance has been reached
                    if (OnAir && Body.Position.Y <= _jumpMaxY) {
                        _canKeepCurrentJump = IsStillJumping = false;
                    }
                }
            }

            // platformer movement only triggers OnMove() on a horizontal movement
            if (Axis.X == 0f) {
                return;
            }

            OnMove();
        }

        #endregion Protected Methods

        #region Private Methods

        private bool HandleRamps(Vector2 displacement, out Vector2 rampDisplacement) {
            if (!OnGround || Math.EqualsEstimate(displacement.X, 0f)) {
                rampDisplacement = Vector2.Zero;
                return false;
            }

            // true horizontal displacement value
            float dX = displacement.X + (float) Body.MoveBufferX;

#if DISABLE_ASCENDING_RAMP
            bool collidesAscendingRamp = false;
            CollisionList<Body> ascdCollisionList = null;
#else
            bool collidesAscendingRamp = Body.CollidesMultiple(Body.Position + new Vector2(dX, 0f), CollisionTags, out CollisionList<Body> ascdCollisionList);
#endif

            /*
            Debug.WriteLine($"\nHandleRamps");
            Debug.WriteLine($"collision list: {collisionList}");
            */

#if DISABLE_DESCENDING_RAMP
            bool collidesDescendingRamp = false;
            CollisionList<Body> descdCollisionList = null;
#else
            bool collidesDescendingRamp = Body.CollidesMultiple(Body.Position + new Vector2(Math.Clamp(dX, -1f, 1f), 1f), CollisionTags, out CollisionList<Body> descdCollisionList);
#endif

            Debug.WriteLine($"collides ascd? {collidesAscendingRamp}, descd? {collidesDescendingRamp}");
            Debug.WriteLine($"_rampFindTries: {_rampFindTries}");

            if (collidesAscendingRamp) {
                Debug.WriteLine("trying ascending ramp");
                bool foundAscending = HandleAscendingRamp(dX, ascdCollisionList, out rampDisplacement);

                if (foundAscending) {
                    _previousRampFound = 1;
                    return true;
                }
            }

            if (collidesDescendingRamp) {
                Debug.WriteLine("trying descending ramp");
                bool foundDescending = HandleDescendingRamp(dX, descdCollisionList, out rampDisplacement);

                if (foundDescending) {
                    _previousRampFound = -1;
                    return true;
                }
            }

            Debug.WriteLine("isn't a ramp at all");
            rampDisplacement = Vector2.Zero;
            _previousRampFound = 0;
            return false;
        }

        private bool HandleAscendingRamp(float dX, CollisionList<Body> collisionList, out Vector2 rampDisplacement) {
            // elevations
            //   nearest to 90f => wall; 
            //   close to 0f => straight floor;
            float minElevation = 0f,  // in degrees
                  maxElevation = 60f; // in degrees

            float minSlopeFactor = Math.Cos(maxElevation),
                  maxSlopeFactor = Math.Cos(minElevation);

            bool refreshRampState = false;

            foreach (CollisionInfo<Body> collInfo in collisionList) {
                foreach (Contact c in collInfo.Contacts) {
                    if (Math.EqualsEstimate(Math.Abs(Vector2.Dot(c.Normal, Vector2.Right)), 1f)
                      || Math.EqualsEstimate(Math.Abs(Vector2.Dot(c.Normal, Vector2.Down)), 1f)) {
                        // ignore most of contacts
                        continue;
                    }

                    float horizontalSide = Math.Sign(c.Normal.X),
                          slopeFactor = Math.Abs(c.Normal.X);

                    Debug.WriteLine($"side: {horizontalSide}, slope: {slopeFactor}");

                    if (Util.Helper.InRange(slopeFactor, minSlopeFactor, maxSlopeFactor)) {
                        Debug.WriteLine($"slope in [{minSlopeFactor}, {maxSlopeFactor}] found!");
                        refreshRampState = true;
                        _onRamp = true;
                        _internal_isGravityEnabled = false;

                        Vector2 foundRampNormal = -c.Normal;
                        if (foundRampNormal != _rampNormal) {
                            _rampNormal = foundRampNormal;
                            break;
                        }
                    }
                }
            }

            if (!refreshRampState) {
                Debug.WriteLine($"slope don't found");
                _onRamp = false;
                _rampNormal = Vector2.Zero;
                _internal_isGravityEnabled = true;
                rampDisplacement = Vector2.Zero;
                return false;
            }

            Vector2 rampMoveNormal = Math.Sign(dX) > 0 ? _rampNormal.PerpendicularCW() : _rampNormal.PerpendicularCCW();
            float rampDisplacementDistance = Vector2.Dot(new Vector2(dX, 0f), rampMoveNormal);
            rampDisplacement = rampMoveNormal * rampDisplacementDistance;
            Body.MoveBufferX = 0;
            return true;
        }

        private bool HandleDescendingRamp(float dX, CollisionList<Body> collisionList, out Vector2 rampDisplacement) {
            Debug.WriteLine($"\nHandleDescendingRamps");
            Debug.WriteLine($"collision list: {collisionList}");

            if (_previousRampFound != -1) {
                _rampFindTries = -1;
            }

            // elevations
            //   nearest to 90f => wall; 
            //   close to 0f => straight floor;
            float minElevation = 0f,  // in degrees
                  maxElevation = 60f; // in degrees

            float minSlopeFactor = Math.Cos(maxElevation),
                  maxSlopeFactor = Math.Cos(minElevation);

            bool refreshRampState = false;
            Contact? contact = null;

            foreach (CollisionInfo<Body> collInfo in collisionList) {
                foreach (Contact c in collInfo.Contacts) {
                    if (Math.EqualsEstimate(Math.Abs(Vector2.Dot(c.Normal, Vector2.Right)), 1f)
                      || Math.EqualsEstimate(Math.Abs(Vector2.Dot(c.Normal, Vector2.Down)), 1f)) {
                        // ignore most of contacts
                        continue;
                    }

                    float horizontalSide = Math.Sign(c.Normal.X),
                          slopeFactor = Math.Abs(c.Normal.X);

                    Debug.WriteLine($"side: {horizontalSide}, slope: {slopeFactor}");

                    if (Util.Helper.InRange(slopeFactor, minSlopeFactor, maxSlopeFactor)) {
                        Debug.WriteLine($"slope in [{minSlopeFactor}, {maxSlopeFactor}] found!");
                        refreshRampState = true;
                        _onRamp = true;
                        _internal_isGravityEnabled = false;

                        Vector2 foundRampNormal = -c.Normal;
                        contact = c;

                        if (foundRampNormal != _rampNormal) {
                            _rampNormal = foundRampNormal;
                            break;
                        }
                    }
                }
            }

            if (!refreshRampState) {
                bool cancelRampState = true;

                if (_onRamp) {
                    if (_rampFindTries < 0) {
                        //Debug.WriteLine("slope don't found, but gives another try");
                        _rampFindTries = 1;
                        cancelRampState = false;
                    } else {
                        _rampFindTries -= 1;
                        if (_rampFindTries == 0) {
                            _rampFindTries = -1;
                            //Debug.WriteLine("slope don't found again :(");
                        } else {
                            cancelRampState = false;
                            //Debug.WriteLine("slope don't found, but we still have another chance");
                        }
                    }
                }

                if (cancelRampState) {
                    //Debug.WriteLine($"slope don't found");
                    _onRamp = false;
                    _rampNormal = Vector2.Zero;
                    _internal_isGravityEnabled = true;
                    rampDisplacement = Vector2.Zero;
                    return false;
                }
            } else if (_rampFindTries > -1) {
                _rampFindTries = -1;
            }

            Vector2 rampMoveNormal = Math.Sign(dX) > 0 ? _rampNormal.PerpendicularCW() : _rampNormal.PerpendicularCCW();
            float rampDisplacementDistance = Vector2.Dot(new Vector2(dX, 0f), rampMoveNormal);
            rampDisplacement = rampMoveNormal * rampDisplacementDistance;

            /*
            Debug.WriteLine($"displacement: {rampDisplacement}, penVec.Y: {contact.Value.PenetrationVector.Y}");
            rampDisplacement.Y += 1f - contact.Value.PenetrationVector.Y;
            */

            Body.MoveBufferX = 0;
            Debug.WriteLine($"desc ramp displacement: {rampDisplacement}");
            return true;
        }

        private void Fall() {
            IsFalling = true;
            OnGround = IsJumping = IsStillJumping = _canKeepCurrentJump = false;
            OnFallingBegin();
        }

        #endregion Private Methods
    }
}
