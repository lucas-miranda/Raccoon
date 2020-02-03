//#define DISABLE_RAMPS
//#define DISABLE_ASCENDING_RAMP
//#define DISABLE_DESCENDING_RAMP

#if DISABLE_RAMPS
#define DISABLE_ASCENDING_RAMP
#define DISABLE_DESCENDING_RAMP
#endif

using System.Collections.ObjectModel;
using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        #region Public Members

        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = 200;       // milliseconds
        public static uint JumpInputBufferTime = 200;   // milliseconds

        /// <summary>
        /// Elevation range degrees where it's considered a ramp, and will be walkable,
        /// greater than max value it'll be a wall.
        /// Preferred to be values in ]0, 90[ range.
        /// </summary>
        public static Range AllowedRampElevation = new Range(1, 60); // in degrees

        public delegate void PlatformerMovementAction();
        public event PlatformerMovementAction OnJumpBegin = delegate { },
                                              OnTouchGround = delegate { },
                                              OnFallingBegin = delegate { };

        public delegate void RampEvent(int climbDirection);
        public event RampEvent OnTouchRamp, OnLeaveRamp, OnEnteringRamp, OnLeavingRamp;

        #endregion Public Members

        #region Private Members

        private static readonly Range AllowedSlopeFactorRange = Range.From(Math.Cos(AllowedRampElevation.Min), Math.Cos(AllowedRampElevation.Max));

        private static readonly string DebugText = @"
    Axes
      Current: {0}
      Last: {1}
      Snap H: {2} V: {3}

    Velocity
      Current: {4}
      Bonus: {5}
      Extra: {6}
      Max: {7}   Target: {8}

      Acceleration
        Current: {9}
        Bonus: {10}
        Extra: {11}

    Force: {12}
    Gravity Force: {13}

    Enabled? {14}
    Can Move? {15}

    OnGround? {16}
    IsFalling? {17}

    Jump
      Jumps: {18}
      Can Jump? {19}
      Is Jumping? {20}
      Just Jumped? {21}
      Is Still Jumping? {22}
      Height: {23}

      can keep current jump? {24}
      jump max y: {25}

    Ramps
      On Ramp? {26}
      Ascd? {27} Descd? {28}
      internal isGravityEnabled {29}
      isEnteringRamp? {30}, isLeavingRamp? {31}
      smooth entering (a: {32}, d: {33})
      smooth leaving (a: {34}, d: {35})

    Fall Through
      Can Fall Through? {36}
      is trying to fall through? {37}
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
                     _requestedJump,
                     _canPerformEarlyJumpInput = true;

        private int _jumpMaxY, _ledgeJumpTime;
        private uint _lastTimeFirstRequestToJump;

        // ramp movement
        /*
        private static readonly Vector2[] AscendingRampChecks = new Vector2[] {
            new Vector2(0f, -1f),
            new Vector2(1f, -1f)
        };

        private static readonly Vector2[] DescendingRampChecks = new Vector2[] {
            new Vector2(-2f, 2f),
            new Vector2(-3f, 1f),
            new Vector2(-4f, 0f),
            new Vector2(-9f, 0f)
        };
        */

        private Vector2 _rampNormal;
        private int _previousRampDirection;
        private float _rampAccSmoothing = 1f;
        private bool _isEnteringRamp, _justLeavedRamp;

        // fall through
        private BitTag _fallthroughTags;
        private bool _isTryingToFallThrough;

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
        /// This modifier will be applied to ramp displacement (in px).
        /// </summary>
        public float RampSpeedModifier { get; set; } = 1f;

        /// <summary>
        /// Is Body on a ramp.
        /// </summary>
        public bool IsOnRamp { get; private set; }

        /// <summary>
        /// Is Body on an ascending ramp.
        /// </summary>
        public bool IsOnAscendingRamp { get; private set; }

        public float AscendingRampVelocityModifier { get; set; }

        public float AscendingRampEnteringAccelerationSmoothing { get; set; } = 1f;
        public float AscendingRampLeavingAccelerationSmoothing { get; set; } = 1f;

        /// <summary>
        /// Is Body on a descending ramp.
        /// </summary>
        public bool IsOnDescendingRamp { get; private set; }

        public float DescendingRampVelocityModifier { get; set; }

        public float DescendingRampEnteringAccelerationSmoothing { get; set; } = 1f;
        public float DescendingRampLeavingAccelerationSmoothing { get; set; } = 1f;

        public bool IsLeavingRamp { get; private set; }

        /// <summary>
        /// Tags to check using fall through platform logic.
        /// </summary>
        public BitTag FallThroughTags {
            get {
                return _fallthroughTags;
            }

            set {
                if (value == _fallthroughTags) {
                    return;
                }

                BitTag TagsToRemove = _fallthroughTags & ~value;
                ExtraCollisionTags -= TagsToRemove;
                BitTag TagsToAdd = ~TagsToRemove & value;
                ExtraCollisionTags += TagsToAdd;
                _fallthroughTags = value;
            }
        }

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
                if (!_requestedJump) {
                    // reset permission to perform early jump input
                    // since jump button was released
                    if (!_canPerformEarlyJumpInput) {
                        _lastTimeFirstRequestToJump = 0;
                        _canPerformEarlyJumpInput = true;
                    }

                    if (!_canKeepCurrentJump) {
                        _lastTimeFirstRequestToJump = 0;
                        _canPerformEarlyJumpInput = true;

                        // continuous jump lock (must release and press jump button to jump again)
                        if (Jumps > 0 || OnGround) {
                            _canKeepCurrentJump = true;
                        }
                    }
                }

                _requestedJump = false;
            }

            IsStillJumping = false;
            CanFallThrough = false;
        }

        public override void Update(int delta) {
            base.Update(delta);

            if (IsFalling) {
                if (_ledgeJumpTime <= LedgeJumpMaxTime) {
                    _ledgeJumpTime += delta;
                } else if (Jumps > 0) {
                    // ledge jump time has been missed, so lost all jumps
                    Jumps = 0;
                    _canKeepCurrentJump = false; // Body has just fallen
                }
            }
        }

        public override void DebugRender() {
            base.DebugRender();
            Debug.DrawString(null, new Vector2(10f, 10f), ToStringDetailed());
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
        }

        public override void PhysicsCollisionSubmit(Body otherBody, Vector2 movement, ReadOnlyCollection<Contact> horizontalContacts, ReadOnlyCollection<Contact> verticalContacts) {
            base.PhysicsCollisionSubmit(otherBody, movement, horizontalContacts, verticalContacts);
        }

        public override void PhysicsLateUpdate() {
            base.PhysicsLateUpdate();

            bool isAboveSomething = false,
                 isBelowSomething = Body.CollidesMultiple(Body.Position + Vector2.Up, CollisionTags, out CollisionList<Body> contactsAbove);

            if (GravityEnabled) {
                CollisionList<Body> contactsBelow;

                if (IsOnRamp) {
                    isAboveSomething = Body.CollidesMultiple(Body.Position + new Vector2(0f, 1.5f), CollisionTags, out contactsBelow);
                } else {
                    isAboveSomething = Body.CollidesMultiple(Body.Position + Vector2.Down, CollisionTags, out contactsBelow);
                }

                if (isAboveSomething) {
                    foreach (CollisionInfo<Body> collisionInfo in contactsBelow) {
                        foreach (Contact contact in collisionInfo.Contacts) {
                            if (Vector2.Dot(contact.Normal, Vector2.Down) <= .6f) {
                                continue;
                            }

                            if (contact.PenetrationDepth > 0f) {
                                _touchedBottom = true;
                                break;
                            }
                        }

                        if (_touchedBottom) {
                            break;
                        }
                    }
                }
            } else {
                _touchedBottom = true;
            }

            if (isBelowSomething) {
                foreach (CollisionInfo<Body> collisionInfo in contactsAbove) {
                    foreach (Contact contact in collisionInfo.Contacts) {
                        if (Vector2.Dot(contact.Normal, Vector2.Up) <= .6f) {
                            continue;
                        }

                        if (Helper.InRangeLeftExclusive(contact.PenetrationDepth, 0f, 1f)
                          && !collisionInfo.Subject.Tags.HasAny(FallThroughTags)) {
                            _touchedTop = true;
                            break;
                        }
                    }
                }
            }

            // clear just leaved ramp
            if (_justLeavedRamp) {
                _justLeavedRamp = false;
            }

            if (_touchedTop) {
                // moving up and reached a ceiling
                if (Velocity.Y < 0f) {
                    IsJumping = _canKeepCurrentJump = false;
                    Velocity = new Vector2(Velocity.X, 0f);
                    IsFalling = true;
                    _ledgeJumpTime = 0;
                    OnFallingBegin();
                }
            }

            if (_touchedBottom) {
                // falling and reached the ground
                if (IsFalling || IsJumping) {
                    bool isNotJumpingAnymore = false;

                    if (!IsFalling) {
                        if (Velocity.Y >= 0) {
                            _canKeepCurrentJump = false;
                            isNotJumpingAnymore = true;
                        }
                    } else if (!CanContinuousJump && _canPerformEarlyJumpInput && _requestedJump && Body.Entity.Timer - _lastTimeFirstRequestToJump <= JumpInputBufferTime) {
                        _canKeepCurrentJump = true;
                        isNotJumpingAnymore = true;
                    } else {
                        isNotJumpingAnymore = true;
                    }

                    if (isNotJumpingAnymore) {
                        OnGround = true;
                        IsStillJumping = IsJumping = IsFalling = false;
                        Jumps = MaxJumps;
                        _isTryingToFallThrough = false;

                        OnTouchGround();
                    }
                }

                if (OnGround || IsFalling) {
                    Velocity = new Vector2(Velocity.X, 0f);
                }
            }

            // Check if still is on ground
            if (OnGround && !_touchedBottom && Velocity.Y >= 0f) {
                Fall();
            }
        }

        public override bool CanCollideWith(Vector2 collisionAxes, CollisionInfo<Body> collisionInfo) {
            if (collisionInfo.Subject.Tags.HasAny(FallThroughTags)) {
                if ((CanFallThrough || _isTryingToFallThrough) && collisionAxes.Y > 0f) {
                    // falling through the other body
                    if (!_isTryingToFallThrough) {
                        _isTryingToFallThrough = true;
                    }

                    return false;
                } else if (collisionAxes.Y < 0f || collisionAxes.X != 0f) {
                    // pass through from below to above
                    
                    return false;
                } else if (!collisionInfo.Contacts.Contains(c => Vector2.Dot(c.Normal, Vector2.Down) >= .6f && Math.EqualsEstimate(c.PenetrationDepth, 1f))) {
                    // body can pass through the other body, if isn't directly above
                    if (_isTryingToFallThrough) {
                        _isTryingToFallThrough = false;
                    }

                    return false;
                }
            }

            return base.CanCollideWith(collisionAxes, collisionInfo);
        }

        public override void BodyCollided(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            base.BodyCollided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);
        }

        public override void Collided(Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            base.Collided(collisionAxes, hCollisionInfo, vCollisionInfo);
        }

        public override Vector2 Integrate(float dt) {
            Vector2 displacement = Vector2.Zero, // in pixels
                    velocity = Velocity;         // in pixels/second

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

                    // resetting ramp leaving values
                    if (IsLeavingRamp) {
                        ExtraAcceleration = Vector2.Zero;
                        _rampAccSmoothing = 1f;
                        IsLeavingRamp = false;
                    } else if (_isEnteringRamp) {
                        ExtraAcceleration = Vector2.Zero;
                        _rampAccSmoothing = 1f;
                        _isEnteringRamp = false;
                    }
                } else if (OnGround && !JustReceiveImpulse) {
                    // ground drag force

                    float factor = (1f - DragForce) * (1f - GroundDragForce);

                    velocity.X *= factor;
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

                // resetting ramp leaving values
                if (IsLeavingRamp) {
                    ExtraAcceleration = Vector2.Zero;
                    _rampAccSmoothing = 1f;
                    IsLeavingRamp = false;
                } else if (_isEnteringRamp) {
                    ExtraAcceleration = Vector2.Zero;
                    _rampAccSmoothing = 1f;
                    _isEnteringRamp = false;
                }
            } else if (MaxVelocity.X > 0f) {
                // velocity increasing until reach MaxVelocity.X limit

                float acceleration = Acceleration.X;

                // smoothing when entering on a ramp
                if (_isEnteringRamp) {
                    if (Math.EqualsEstimate(velocity.X, TargetVelocity.X)) {
                        // already reached max horizontal velocity at a ramp
                        _isEnteringRamp = false;
                    } else if (IsOnAscendingRamp) {
                        OnEnteringRamp?.Invoke(-1);

                        // only apply custom smoothing if is atleast at full speed
                        if (velocity.X < BaseMaxVelocity.X) {
                            acceleration *= 2f - Vector2.Dot(_rampNormal, Vector2.Up);
                        } else {
                            acceleration *= AscendingRampEnteringAccelerationSmoothing;
                        }
                    } else if (IsOnDescendingRamp) {
                        OnEnteringRamp?.Invoke(1);

                        // only apply custom smoothing if is atleast at full speed
                        if (velocity.X < BaseMaxVelocity.X) {
                            acceleration *= 2f - Vector2.Dot(_rampNormal, Vector2.Up);
                        } else {
                            acceleration *= DescendingRampEnteringAccelerationSmoothing;
                        }
                    }
                } else if (IsLeavingRamp) {
                    if (Math.EqualsEstimate(velocity.X, TargetVelocity.X)) {
                        // already reached max horizontal velocity off a ramp
                        IsLeavingRamp = false;
                    } else {
                        OnLeavingRamp?.Invoke(_previousRampDirection);
                        acceleration *= _rampAccSmoothing;
                    }
                }

                velocity.X = Math.Approach(velocity.X, TargetVelocity.X, acceleration * dt);
            } else {
                // velocity increasing without a limit
                velocity.X += Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            displacement.X = (velocity.X + Body.Force.X) * dt;

            ///////////////////////
            // Vertical Velocity //
            ///////////////////////

#if !DISABLE_RAMPS
            bool isWalkingOnRamp = HandleRamps(displacement, out Vector2 rampDisplacement);

            if (isWalkingOnRamp) {
                displacement = rampDisplacement;
            }
#endif

            if (!Math.EqualsEstimate(ImpulseTime, 0f) && ImpulsePerSec.Y != 0f) {
                // handling impulse
                velocity.Y += ImpulsePerSec.Y * dt;
            }

            if (GravityEnabled && _internal_isGravityEnabled) {
                // apply gravity force
                velocity.Y += GravityScale * GravityForce.Y * dt;
            }

            if (IsStillJumping) {
                // apply jumping acceleration if it's jumping
                velocity.Y -= Acceleration.Y * dt;
            }

            displacement.Y += (velocity.Y + Body.Force.Y) * dt;

            if (!Math.EqualsEstimate(ImpulseTime, 0f)) {
                ImpulseTime = Math.Approach(ImpulseTime, 0f, dt);
                if (Math.EqualsEstimate(ImpulseTime, 0f)) {
                    ImpulsePerSec = Vector2.Zero;
                }
            }

            Velocity = velocity;
            //Debug.WriteLine($"d: {displacement}");
            return displacement;
        }

        public void Jump() {
            // continuous jump lock (must release and press jump button to jump again)
            _requestedJump = true;
            if (_canPerformEarlyJumpInput && _lastTimeFirstRequestToJump == 0) {
                if (!IsFalling) {
                    // block early jump input until release and press button again
                    _canPerformEarlyJumpInput = false;
                } else {
                    _lastTimeFirstRequestToJump = Body.Entity.Timer;
                }
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

            IsStillJumping = true;
            _jumpMaxY = (int) (Body.Position.Y - JumpHeight);
            Velocity = new Vector2(Velocity.X, -(Acceleration.Y * JumpExplosionRate));
        }

        public void FallThrough() {
            CanFallThrough = true;

            if (OnGround) {
                Velocity = new Vector2(Velocity.X, Acceleration.Y * JumpExplosionRate);
            }
        }

        public string ToStringDetailed() {
            return string.Format(
                DebugText,
                Axis, LastAxis, SnapHorizontalAxis, SnapVerticalAxis,
                Velocity, BonusMaxVelocity, ExtraMaxVelocity, MaxVelocity, TargetVelocity, 
                Acceleration, BonusAcceleration, ExtraAcceleration,
                Body.Force, GravityForce * GravityScale,
                Enabled, CanMove,
                OnGround, IsFalling,
                Jumps, CanJump, IsJumping, JustJumped, IsStillJumping, JumpHeight, _canKeepCurrentJump, _jumpMaxY,
                IsOnRamp, IsOnAscendingRamp, IsOnDescendingRamp, _internal_isGravityEnabled, _isEnteringRamp, IsLeavingRamp, 
                AscendingRampEnteringAccelerationSmoothing, DescendingRampEnteringAccelerationSmoothing,
                AscendingRampLeavingAccelerationSmoothing, DescendingRampLeavingAccelerationSmoothing,
                CanFallThrough, _isTryingToFallThrough
            );
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            OnJumpBegin = OnTouchGround = OnFallingBegin = null;
            OnTouchRamp = OnLeaveRamp = null;

            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            if (distance.Y > 0f) {
                // if it's moving down then it's falling
                if (IsJumping && !IsFalling) {
                    Fall();
                }
            } else if (distance.Y < 0f) {
                if (IsStillJumping || JustReceiveImpulse) {
                    if (!IsJumping) {
                        IsJumping = JustJumped = true;
                        OnGround = IsFalling = false;
                        Jumps--;
                        OnJumpBegin();
                        //Debug.WriteLine("By jump begin");
                        ClearRampState();
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

            OnMove(new Vector2(distance.X, 0f));
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Checks if Body is actually on a ramp and the displacement in it.
        /// </summary>
        /// <param name="displacement">Movement displacement.</param>
        /// <param name="rampDisplacement">Calculated ramp displacement.</param>
        /// <returns>True, if it's on a ramp, False otherwise.</returns>
        private bool HandleRamps(Vector2 displacement, out Vector2 rampDisplacement) {
            bool wasOnAscendingRamp = IsOnAscendingRamp,
                 wasOnDescendingRamp = IsOnDescendingRamp;

            if (!OnGround) {
                rampDisplacement = Vector2.Zero;
                //Debug.WriteLine("By not on ground");
                ClearRampState();
                return false;
            }

            if (Math.EqualsEstimate(displacement.X, 0f)) {
                rampDisplacement = Vector2.Zero;
                return false;
            }

            //Debug.WriteLine($"\nDisplacement: {displacement}");

            // true horizontal displacement value
            float dX = displacement.X + (float) Body.MoveBufferX;

#if !DISABLE_ASCENDING_RAMP
            //Debug.WriteLine("> Verifying Asc ramp...");
            if (Body.CollidesMultiple(Body.Position + new Vector2(Math.Sign(dX) + .5f, 0f), CollisionTags, out CollisionList<Body> ascdCollisionList)) {
                //Debug.WriteLine("Maybe found it");

                Vector2[] ascRampChecks = new Vector2[] {
                    new Vector2(0f, -1f),
                    new Vector2(1f, -1f)
                };

                if (HandleRamp(dX, ascRampChecks, ascdCollisionList, directionSameAsNormal: false, out rampDisplacement)) {
                    //Debug.WriteLine("Confirmed");
                    Body.MoveBufferY = 0;
                    IsOnAscendingRamp = true;
                    IsOnDescendingRamp = false;

                    if (wasOnDescendingRamp) {
                        IsLeavingRamp = true;
                        _previousRampDirection = 1;
                        OnLeaveRamp?.Invoke(1);
                        _rampAccSmoothing = DescendingRampLeavingAccelerationSmoothing;
                        ExtraMaxVelocity = Vector2.Zero;
                    }

                    if (!wasOnAscendingRamp) {
                        IsLeavingRamp = false;
                        _isEnteringRamp = true;
                        OnTouchRamp?.Invoke(-1);
                        _rampAccSmoothing = AscendingRampEnteringAccelerationSmoothing;
                        ExtraMaxVelocity = new Vector2(AscendingRampVelocityModifier, 0f);
                    }

                    //Debug.WriteLine($"Ramp displacement: {rampDisplacement}");
                    return true;
                }
            }
#endif

#if !DISABLE_DESCENDING_RAMP
            //Debug.WriteLine("> Verifying Desc ramp...");
            if (Body.CollidesMultiple(Body.Position + new Vector2(Math.Sign(dX) * .5f, 2f), CollisionTags, out CollisionList<Body> descdCollisionList)) {
            //Debug.WriteLine("Maybe found it");
                Rectangle bodyBounds = Body.Bounds;
                Size halfBodySize = bodyBounds.Size / 2f;

                Vector2[] descRampChecks = new Vector2[] {
                    new Vector2(-2f, 2f),
                    new Vector2(-3f, 1f),
                    // one quarter to help check ramps
                    new Vector2(-(halfBodySize.Width / 2f), 0f),
                    // when leaving desc ramp
                    new Vector2(-(halfBodySize.Width), 0f),
                    new Vector2(-(bodyBounds.Width + 1f), 0f)
                };

                if (HandleRamp(dX, descRampChecks, descdCollisionList, directionSameAsNormal: true, out rampDisplacement)) {
                    //Debug.WriteLine("Confirmed");
                    Body.MoveBufferY = 0;
                    IsOnAscendingRamp = false;
                    IsOnDescendingRamp = true;

                    if (wasOnAscendingRamp) {
                        IsLeavingRamp = true;
                        _previousRampDirection = -1;
                        OnLeaveRamp?.Invoke(-1);
                        _rampAccSmoothing = AscendingRampLeavingAccelerationSmoothing;
                        ExtraMaxVelocity = Vector2.Zero;
                    }

                    if (!wasOnDescendingRamp) {
                        IsLeavingRamp = false;
                        _isEnteringRamp = true;
                        OnTouchRamp?.Invoke(1);
                        _rampAccSmoothing = DescendingRampEnteringAccelerationSmoothing;
                        ExtraMaxVelocity = new Vector2(DescendingRampVelocityModifier, 0f);
                    }

                    //Debug.WriteLine($"Ramp displacement: {rampDisplacement}");
                    return true;
                }
            }
#endif

            rampDisplacement = Vector2.Zero;
            //Debug.WriteLine($"No one has been found");
            ClearRampState();
            return false;
        }

        /// <summary>
        /// Handle any kind of ramp check and gives a ramp displacement Vector2 given a horizontal displacement.
        /// </summary>
        /// <param name="dX">Movement horizontal displacement (in pixels).</param>
        /// <param name="rampChecks">Positions to check for a ramp, relative to Body.Shape, if it it were to the right. (To the left is the same, but mirrored)</param>
        /// <param name="collisionList">Collision list to find for a ramp.</param>
        /// <param name="directionSameAsNormal">If displacement direction should be the same as ramp normal face, for validation.</param>
        /// <param name="rampDisplacement">Calculated ramp displacement, it should be used to move Body on ramp.</param>
        /// <returns>True, if found a valid ramp, and False otherwise.</returns>
        private bool HandleRamp(float dX, Vector2[] rampChecks, CollisionList<Body> collisionList, bool directionSameAsNormal, out Vector2 rampDisplacement) {
            int direction = Math.Sign(dX);
            Rectangle boundingBox = Body.Shape.BoundingBox;

            Vector2[] rampPositionsToCheck = new Vector2[rampChecks.Length];
            Vector2 realign = direction < 0 ? new Vector2(-1f, 0f) : Vector2.Zero;

            for (int i = 0; i < rampChecks.Length; i++) {
                Vector2 currentRampCheck = rampChecks[i];
                rampPositionsToCheck[i] = Body.Position + new Vector2(direction * (boundingBox.Width / 2f + currentRampCheck.X), boundingBox.Height / 2f + currentRampCheck.Y) + realign;
            }
            
            bool refreshRampState = false;

            foreach (CollisionInfo<Body> collInfo in collisionList) {
                bool isValidRamp = false,
                     hasFindRamp = LookForRamp(collInfo, rampPositionsToCheck, out Vector2 rampNormal);

                if (hasFindRamp) {
                    /*
                        Ascending Ramp

                        Displacement Direction  Ramp Normal
                                  ->               x < 0
                                  <-               x > 0

                        ----

                        Descending Ramp

                        Displacement Direction  Ramp Normal
                                  ->               x > 0
                                  <-               x < 0
                     */

                    if (directionSameAsNormal) {
                        if (Math.Sign(rampNormal.X) == direction) {
                            isValidRamp = true;
                        }
                    } else {
                        if (Math.Sign(rampNormal.X) != direction) {
                            isValidRamp = true;
                        }
                    }
                }

                if (isValidRamp) {
                    refreshRampState = true;
                    IsOnRamp = true;
                    _rampNormal = rampNormal;
                    _internal_isGravityEnabled = false;
                    break;
                }
            }

            if (!refreshRampState) {
                rampDisplacement = Vector2.Zero;
                return false;
            }

            Vector2 rampMoveNormal;
            float rampDisplacementDistance;

            //Debug.WriteLine($"ramp normal: {_rampNormal}");

            //if (Math.EqualsEstimate(validRampHorizontalDist, 0f)) {
                rampMoveNormal = Math.Sign(dX) > 0 ? _rampNormal.PerpendicularCW() : _rampNormal.PerpendicularCCW();
                rampDisplacementDistance = Vector2.Dot(new Vector2(dX, 0f), rampMoveNormal);
            /*
            } else {
                rampMoveNormal = new Vector2(Math.Sign(validRampHorizontalDist), 0f);
                rampDisplacementDistance = Math.Round(validRampHorizontalDist);
            }
            */

            rampDisplacement = rampMoveNormal * rampDisplacementDistance * RampSpeedModifier;
            Body.MoveBufferX = 0;
            return true;
        }

        /// <summary>
        /// Try to find a ramp in a given CollisionInfo data and position.
        /// </summary>
        /// <param name="collisionInfo">Collision data to look for a ramp.</param>
        /// <param name="positionsToCheck">World positions to check for a ramp.</param>
        /// <param name="rampNormal">Ramp normal, if a ramp was found.</param>
        /// <returns>True, if a ramp was found, False otherwise.</returns>
        private bool LookForRamp(CollisionInfo<Body> collisionInfo, Vector2[] positionsToCheck, out Vector2 rampNormal) {
            switch (collisionInfo.Subject.Shape) {
                case BoxShape boxShape:
                    // not implemented yet
                    break;

                case CircleShape circleShape:
                    // not implemented yet
                    break;

                case PolygonShape polygonShape:
                    // not implemented yet
                    break;

                case GridShape gridShape:
                    foreach (Vector2 rampPositionCheck in positionsToCheck) {
                        (int gridColumn, int gridRow) = gridShape.ConvertPosition(collisionInfo.Subject.Position, rampPositionCheck);
                        ref GridShape.TileShape tileShape = ref gridShape.GetTileInfo(gridColumn, gridRow);

                        switch (tileShape) {
                            case GridShape.BoxTileShape boxTileShape:
                                //boxTileShape.CreateCollisionPolygon(gridShape, collInfo.Subject.Position, gridColumn, gridRow);
                                // BoxTileShape will always be a straight wall or ground
                                // in this case it'll be 90 degree wall
                                break;

                            case GridShape.PolygonTileShape polygonTileShape:
                                Polygon tilePolygon = polygonTileShape.CreateCollisionPolygon(gridShape, collisionInfo.Subject.Position, gridColumn, gridRow);

                                // find closest edge
                                (Line? Edge, float Distance, Vector2 Normal, float SlopeFactor) closestEdge = (null, float.PositiveInfinity, Vector2.Zero, 0f);

                                int edgeIndex = 0;
                                foreach (Line edge in tilePolygon.Edges()) {
                                    Vector2 edgeNormal = tilePolygon.Normals[edgeIndex];
                                    float slopeFactor = edgeNormal.Projection(Vector2.Up);

                                    // ignore some edges by slope factor
                                    if (!Helper.InRange(slopeFactor, AllowedSlopeFactorRange.Min, AllowedSlopeFactorRange.Max)) {
                                        edgeIndex += 1;
                                        continue;
                                    }

                                    float distanceToEdge = edge.DistanceSquared(rampPositionCheck);

                                    if (distanceToEdge < closestEdge.Distance
                                      || (distanceToEdge == closestEdge.Distance && slopeFactor > closestEdge.SlopeFactor)) { // always prefer an edge who owns a normal closer to Vector2.Up
                                        closestEdge = (edge, distanceToEdge, edgeNormal, slopeFactor);
                                    }

                                    edgeIndex += 1;
                                }

                                // tile doesn't contains a valid ramp
                                if (closestEdge.Edge == null) {
                                    break;
                                }

                                rampNormal = closestEdge.Normal;
                                return true;

                            case null:
                            default:
                                break;
                        }
                    }

                    break;

                case null:
                default:
                    break;
            }

            rampNormal = Vector2.Zero;
            return false;
        }

        private void Fall() {
            //Debug.WriteLine("By falling");
            ClearRampState();
            IsFalling = true;
            OnGround = IsJumping = IsStillJumping = _canKeepCurrentJump = false;
            _ledgeJumpTime = 0;
            OnFallingBegin();
        }

        private void ClearRampState() {
            if (!IsOnRamp) {
                return;
            }

            //Debug.WriteLine("Clear ramp state");
            IsOnRamp = false;
            _rampNormal = Vector2.Zero;
            _internal_isGravityEnabled = true;

            IsLeavingRamp = _justLeavedRamp = true;
            if (IsOnDescendingRamp) {
                _previousRampDirection = 1;
                OnLeaveRamp?.Invoke(1);
                _rampAccSmoothing = DescendingRampLeavingAccelerationSmoothing;
                ExtraMaxVelocity = Vector2.Zero;
            } else {
                _previousRampDirection = -1;
                OnLeaveRamp?.Invoke(-1);
                _rampAccSmoothing = AscendingRampLeavingAccelerationSmoothing;
                ExtraMaxVelocity = Vector2.Zero;
            }

            IsOnDescendingRamp = IsOnAscendingRamp = false;
        }

        #endregion Private Methods
    }
}
