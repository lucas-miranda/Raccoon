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
      
    Impulse
      Per Sec: {12} 
      Duration: {13} sec
      Just Received? {14}
      Is Receiving? {15}

    Force: {16}
    Gravity Force: {17}

    Enabled? {18}
    Can Move? {19}

    OnGround? {20}
    IsFalling? {21}

    Jump
      Jumps: {22}/{23}
      Can Jump? {24}
      Has Jumped? {25}
      Is Jumping? {26}
      Just Jumped? {27}
      Is Still Jumping? {28}
      Height: {29}

      can keep current jump? {30}
      jump max y: {31}

    Ramps
      On Ramp? {32}
      Ascd? {33} Descd? {34}
      internal isGravityEnabled {35}
      isEnteringRamp? {36}, isLeavingRamp? {37}

    Fall Through
      Can Fall Through? {38}
      is trying to fall through? {39}
    ";

        // general

        /// <summary>
        /// Some systems (sunch as ramp climbing) need to temporarily disable gravity in order to work properly.
        /// </summary>
        private bool _internal_isGravityEnabled = true;

        private bool _touchedBottom, _touchedTop;

        // jump
        private int _maxJumps = 1;
        private bool _canJump = true, 
                     _canKeepCurrentJump = true, 
                     _requestedJump,
                     _jumpStart,
                     _canPerformEarlyJumpInput = true,
                     _canPerformLedgeJump = true,
                     _canPerformAdditionalJump;

        private int _jumpMaxY, _ledgeJumpTime, _currentLedgeJumpMaxTime;
        private uint _lastTimeFirstRequestToJump;

        // ramp movement
        private Vector2 _rampNormal;
        private int _previousRampDirection;
        private bool _isEnteringRamp, _justLeavedRamp;

        private float _rampCurrentAcceleration;
        private bool _isRampAccelerationApplied;

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
        /// Means, even when is falling, that is at a jump sequence
        ///  and not just fall from somewhere.
        /// </summary>
        public bool HasJumped { get; protected set; }

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
        public int MaxJumps {
            get {
                return _maxJumps;
            }

            set {
                int previousMaxJumps = _maxJumps;
                _maxJumps = Math.Max(0, value);

                // update current jumps amount
                if (Jumps != _maxJumps) {
                    if (OnGround) {
                        Jumps = _maxJumps;
                    } else {
                        if (_maxJumps > previousMaxJumps) {
                            Jumps += _maxJumps - previousMaxJumps;
                        } else if (_maxJumps < previousMaxJumps) {
                            Jumps = Math.Max(0, Jumps - (previousMaxJumps - _maxJumps));
                        }
                    }
                }
            }
        }

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

        /// <summary>
        /// Is Body on a descending ramp.
        /// </summary>
        public bool IsOnDescendingRamp { get; private set; }

        /// <summary>
        /// Is leaving ramp (ascending or descending).
        /// This stays true until velocity smooth ends.
        /// </summary>
        public bool IsLeavingRamp { get; private set; }

        /// <summary>
        /// Ramp acceleration that should be applied.
        /// It'll be calculated dynamically.
        /// </summary>
        public float RampAcceleration { get; private set; }

        /// <summary>
        /// Extra velocity applied at an ascending ramp.
        /// </summary>
        public float AscendingRampVelocityModifier { get; set; }

        /// <summary>
        /// Extra velocity applied at a descending ramp.
        /// </summary>
        public float DescendingRampVelocityModifier { get; set; }

        /// <summary>
        /// Controls ascending ramp acceleration gain.
        /// </summary>
        public int TimeToAchieveMaxVelocityAtAscendingRamp { get; set; } = 1000;

        /// <summary>
        /// Controls descending ramp acceleration gain.
        /// </summary>
        public int TimeToAchieveMaxVelocityAtDescendingRamp { get; set; } = 1000;

        /// <summary>
        /// When leaving ascending ramp, velocity will be reduced to this amount smooth it.
        /// </summary>
        public float SmoothLeavingAscendingRampVelocityCuttoff { get; set; } = 1f;

        /// <summary>
        /// When leaving descending ramp, velocity will be reduced to this amount smooth it.
        /// </summary>
        public float SmoothLeavingDescendingRampVelocityCuttoff { get; set; } = 1f;

        public float SmoothFallingFromDescendingRampVelocityCuttoff { get; set; } = 1f;

        /// <summary>
        /// Time limit that it's allowed to jump action perform a jump when start falling.
        /// </summary>
        public int LedgeJumpMaxTime { get; set; }

        public int RampLedgeJumpMaxTime { get; set; }

        /// <summary>
        /// If jump action continuously happens until this time (in milliseconds) before touching the ground,
        /// jump will happen automatically after ground is touched.
        /// </summary>
        public uint JumpInputBufferTime { get; set; }

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

        public bool IsStoppingFromImpulse { get; private set; }

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// If jump condition is renewed.
        /// </summary>
        protected bool IsStillJumping { get; set; }

        protected bool IsJumpRequested { get { return _requestedJump; } }
        protected bool IsTryingToFallThrough { get { return _isTryingToFallThrough; } }

        #endregion Protected Properties

        #region Public Methods

        public override void BeforeUpdate() {
            base.BeforeUpdate();

            if (!_requestedJump) {
                // user keeps one entire frame without calling Jump()
                // we can do some unlocks now

                if (!CanContinuousJump) {
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

                // allows to perform additional jump while still going up
                if (Jumps > 0) {
                    _canPerformAdditionalJump = true;
                }
            }

            _jumpStart = false;
            _requestedJump = false;
            IsStillJumping = false;
            CanFallThrough = false;
        }

        public override void Update(int delta) {
            base.Update(delta);

            if (IsFalling && _canPerformLedgeJump) {
                if (_ledgeJumpTime <= _currentLedgeJumpMaxTime) {
                    _ledgeJumpTime += delta;
                } else if (Jumps > 0) {
                    // ledge jump time missed, so jump has been lost
                    Jumps -= 1;
                    _canKeepCurrentJump = false; // Body has just fallen
                    _canPerformLedgeJump = false;
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

        public override void PhysicsLateUpdate(float dt) {
            base.PhysicsLateUpdate(dt);

            bool isAboveSomething = false,
                 isBelowSomething = Body.CollidesMultiple(Body.Position + Vector2.Up, CollisionTags, out CollisionList<Body> contactsAbove);

            if (GravityEnabled && _internal_isGravityEnabled) {
                CollisionList<Body> contactsBelow;

                float downLength = IsOnRamp ? 1.5f : 1f;
                isAboveSomething = Body.CollidesMultiple(Body.Position + new Vector2(0f, downLength), CollisionTags, out contactsBelow);

                if (isAboveSomething) {
                    foreach (CollisionInfo<Body> collisionInfo in contactsBelow) {
                        foreach (Contact contact in collisionInfo.Contacts) {
                            if (Vector2.Dot(contact.Normal, Vector2.Down) <= .6f) {
                                continue;
                            }

                            if (Helper.InRangeLeftExclusive(contact.PenetrationDepth, 0f, downLength)) {
                                if (collisionInfo.Subject.Tags.HasAny(FallThroughTags)
                                  && (_isTryingToFallThrough || contact.PenetrationDepth > 1f)) {
                                    // ignore collision due to falling through it
                                    continue;
                                }

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

                    // ledge jump
                    _ledgeJumpTime = 0;
                    _canPerformLedgeJump = false;

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
                        _canKeepCurrentJump = 
                            isNotJumpingAnymore = true;
                    } else {
                        isNotJumpingAnymore = true;
                    }

                    if (isNotJumpingAnymore) {
                        OnGround = true;

                        IsStillJumping = 
                            IsJumping = 
                            IsFalling = 
                            HasJumped = false;

                        Jumps = MaxJumps;
                        _canPerformLedgeJump = true;
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
                if (collisionAxes.Y > 0f || collisionAxes.X != 0f) {
                    // falling through the other body
                    // or just passing horizontally through it

                    if (CanFallThrough) {
                        // trying to actively force fall through
                        
                        if (!_isTryingToFallThrough) {
                            _isTryingToFallThrough = true;
                        }

                        return false;
                    } else {
                        // fallthrough bodies on top each other will be treated as one
                        // they must be at least 1px a part to be treated separetely
                        foreach (Body collisionBody in Body.CollisionList) {
                            if (collisionBody != collisionInfo.Subject 
                             && collisionBody.Tags.HasAny(FallThroughTags)
                             && collisionBody.Bottom - collisionInfo.Subject.Top >= -Math.Epsilon
                             && collisionBody.Top < collisionInfo.Subject.Top
                            ) {
                                return false;
                            }
                        }

                        // check if we have reached a platform top
                        // if yes, we should stop trying to pass through
                        if (collisionInfo.Contacts.Contains(c => Vector2.Down.Projection(c.Normal) >= .6f && c.PenetrationDepth <= 1f)) {
                            if (_isTryingToFallThrough) {
                                _isTryingToFallThrough = false;
                            }

                            return true;
                        }
                    }

                    // enable pass through
                    if (!_isTryingToFallThrough) {
                        _isTryingToFallThrough = true;
                    }

                    return false;
                } else if (collisionAxes.Y < 0f) {
                    // pass through from below to above

                    if (!_isTryingToFallThrough) {
                        _isTryingToFallThrough = true;
                    }

                    return false;
                } else if (_isTryingToFallThrough) {
                    // default case to avoid being stuck
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
                } else if (OnGround && !IsReceivingImpulse && !IsStoppingFromImpulse) {
                    // ground drag force
                    float factor = (1f - DragForce) * (1f - GroundDragForce);

                    velocity.X *= factor;
                } else {
                    // air drag force
                    float airDragForce = AirDragForce,
                          impulseSpeedCutoff = 0f;

                    if (IsReceivingImpulse || IsStoppingFromImpulse) {
                        if (OnAir) {
                            impulseSpeedCutoff = 5f;
                            airDragForce /= 2f;
                        } else { // on ground
                            impulseSpeedCutoff = MaxVelocity.X / 2f;
                        }
                    }

                    // air drag force
                    velocity.X *= (1f - DragForce) * (1f - airDragForce);

                    if (IsStoppingFromImpulse && Math.Abs(velocity.X) < impulseSpeedCutoff) {
                        IsStoppingFromImpulse = false;
                    }
                }

                if (Math.Abs(velocity.X) <= Math.Abs(MaxVelocity.X * .5f)) {
                    // resetting ramp leaving values
                    if (IsLeavingRamp) {
                        ExtraAcceleration = Vector2.Zero;
                        IsLeavingRamp = false;
                    } else if (_isEnteringRamp) {
                        ExtraAcceleration = Vector2.Zero;
                        _isEnteringRamp = false;
                    }

                    // reset ramp smooth control
                    _rampCurrentAcceleration = 0f;
                    _isRampAccelerationApplied = false;
                }
            } else if (SnapHorizontalAxis && velocity.X != 0f && Math.Sign(Axis.X) != Math.Sign(velocity.X)) {
                // snaps horizontal velocity to zero, if horizontal axis is on opposite direction
                velocity.X = 0f;

                if (IsStoppingFromImpulse) {
                    IsStoppingFromImpulse = false;
                }

                // resetting ramp leaving values
                if (IsLeavingRamp) {
                    ExtraAcceleration = Vector2.Zero;
                    IsLeavingRamp = false;
                } else if (_isEnteringRamp) {
                    ExtraAcceleration = Vector2.Zero;
                    _isEnteringRamp = false;
                }

                // reset ramp smooth control
                _rampCurrentAcceleration = 0f;
                _isRampAccelerationApplied = false;
            } else if (MaxVelocity.X > 0f) {
                // velocity increasing until reach MaxVelocity.X limit
                float acceleration = Acceleration.X;

                // smoothing when entering on a ramp
                if (_isEnteringRamp) {
                    if (IsOnAscendingRamp) {
                        OnEnteringRamp?.Invoke(-1);
                    } else if (IsOnDescendingRamp) {
                        OnEnteringRamp?.Invoke(1);
                    }

                    _isEnteringRamp = false;
                } else if (IsLeavingRamp) {
                    if (OnGround && !Math.EqualsEstimate(Math.Abs(velocity.X), MaxVelocity.X)) {
                        // we'll need to smooth this
                        acceleration = _rampCurrentAcceleration;
                    } else {
                        // ensure we'll not fall from ramp at high speed
                        if (!OnGround && Math.Abs(velocity.X) > MaxVelocity.X) {
                            velocity.X =  Math.Sign(TargetVelocity.X) * MaxVelocity.X;
                        }

                        OnLeavingRamp?.Invoke(_previousRampDirection);
                        IsLeavingRamp =
                            _isRampAccelerationApplied = false;

                        _rampCurrentAcceleration = 0f;
                    }
                } else if (IsOnRamp) {
                    // velocity grows a lot slower on ramps
                    // to avoid suddenly velocity changes

                    if (Math.Abs(velocity.X) < BaseMaxVelocity.X) {
                        if (_rampCurrentAcceleration < Acceleration.X) {
                            _rampCurrentAcceleration = Acceleration.X * 4f;
                            _isRampAccelerationApplied = false;
                        }
                    } else if (!_isRampAccelerationApplied) {
                        _rampCurrentAcceleration = RampAcceleration;
                        _isRampAccelerationApplied = true;
                    }

                    if (IsOnAscendingRamp) {
                    } else if (IsOnDescendingRamp) {
                        if (_isRampAccelerationApplied) {
                            _rampCurrentAcceleration += 10;
                        }
                    }

                    acceleration = _rampCurrentAcceleration;
                }

                velocity.X = Math.Approach(velocity.X, TargetVelocity.X, acceleration * dt);

                if (IsStoppingFromImpulse) {
                    IsStoppingFromImpulse = false;
                }
            } else {
                // velocity increasing without a limit
                velocity.X += Math.Sign(Axis.X) * Acceleration.X * dt;

                if (IsStoppingFromImpulse) {
                    IsStoppingFromImpulse = false;
                }
            }

            displacement.X = (velocity.X + Body.Force.X) * dt;

            ///////////////////////
            // Vertical Velocity //
            ///////////////////////

#if !DISABLE_RAMPS
            bool isWalkingOnRamp = HandleRamps(displacement, out Vector2 rampDisplacement, ref velocity);

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

            Velocity = velocity;
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
            // and don't have or can't perform an additional jump
            if (IsJumping && (Jumps == 0 || !_canPerformAdditionalJump)) {
                IsStillJumping = true;
                return;
            }

            // checks if can jump and ledge jump time
            if (!CanJump || (!OnGround && Jumps == 0 && _ledgeJumpTime > _currentLedgeJumpMaxTime)) {
                return;
            }

            IsStillJumping = true;
            _jumpStart = true;
            _canPerformAdditionalJump = false; // it'll need to be reevaluated later
            _canPerformLedgeJump = false; // after any jump, can't perform ledge jump until reaches ground again
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

                // axes
                Axis, LastAxis, SnapHorizontalAxis, SnapVerticalAxis,

                // velocity
                Velocity, BonusMaxVelocity, ExtraMaxVelocity, MaxVelocity, TargetVelocity, 

                // * acceleration
                Acceleration, BonusAcceleration, ExtraAcceleration,

                // impulse
                ImpulsePerSec, 
                ImpulseTime, 
                JustReceiveImpulse, 
                IsReceivingImpulse,

                Body.Force, GravityForce * GravityScale,
                Enabled, CanMove,
                OnGround, IsFalling,

                // jump
                Jumps, MaxJumps, CanJump, HasJumped, IsJumping, JustJumped, IsStillJumping, JumpHeight, _canKeepCurrentJump, _jumpMaxY,

                // ramps
                IsOnRamp, IsOnAscendingRamp, IsOnDescendingRamp, _internal_isGravityEnabled, _isEnteringRamp, IsLeavingRamp, 

                // fallthrough
                CanFallThrough, _isTryingToFallThrough
            );
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            OnJumpBegin = OnTouchGround = OnFallingBegin = null;
            OnTouchRamp = OnLeaveRamp = OnEnteringRamp = OnLeavingRamp = null;

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
            } else if (distance.Y < 0f && (IsStillJumping || JustReceiveImpulse)) {
                if (!IsJumping || _jumpStart) {
                    IsJumping = 
                        JustJumped = 
                        HasJumped = true;

                    OnGround = 
                        IsFalling = false;

                    Jumps--;
                    OnJumpBegin();
                    ClearRampState();
                } else if (JustJumped) {
                    JustJumped = false;
                }

                if (IsStillJumping) {
                    // checks if jump max distance has been reached
                    if (OnAir && Body.Position.Y <= _jumpMaxY) {
                        _canKeepCurrentJump = 
                            IsStillJumping = false;
                    }
                } else if (JustReceiveImpulse) {
                    JustReceiveImpulse = false;
                }
            }

            // platformer movement only triggers OnMove() on a horizontal movement
            if (Axis.X == 0f) {
                return;
            }

            OnMove(new Vector2(distance.X, 0f));
        }

        protected override void ImpulseEnds() {
            base.ImpulseEnds();
            IsStoppingFromImpulse = true;
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Checks if Body is actually on a ramp and the displacement in it.
        /// </summary>
        /// <param name="displacement">Movement displacement.</param>
        /// <param name="rampDisplacement">Calculated ramp displacement.</param>
        /// <returns>True, if it's on a ramp, False otherwise.</returns>
        private bool HandleRamps(Vector2 displacement, out Vector2 rampDisplacement, ref Vector2 currentVelocity) {
            bool wasOnAscendingRamp = IsOnAscendingRamp,
                 wasOnDescendingRamp = IsOnDescendingRamp;

            if (!OnGround) {
                rampDisplacement = Vector2.Zero;
                ClearRampState();
                return false;
            }

            if (Math.EqualsEstimate(displacement.X, 0f)) {
                rampDisplacement = Vector2.Zero;
                return false;
            }

            // true horizontal displacement value
            float dX = displacement.X + (float) Body.MoveBufferX;

#if !DISABLE_ASCENDING_RAMP
            if (Body.CollidesMultiple(Body.Position + new Vector2(Math.Sign(dX), 1f), CollisionTags, out CollisionList<Body> ascdCollisionList)) {

                bool shouldMoveTowardsRamp = true;
                Vector2 moveDir = new Vector2(Math.Sign(dX), 0f);
                float greaterDist = 0f;

                foreach (CollisionInfo<Body> collInfo in ascdCollisionList) {
                    foreach (Contact c in collInfo.Contacts) {
                        float proj = moveDir.Projection(c.Normal);
                        if (proj <= 0f) {
                            continue;
                        }

                        float dist = proj * c.PenetrationDepth;
                        if (dist > greaterDist) {
                            greaterDist = dist;
                        }
                    }

                    if (!shouldMoveTowardsRamp) {
                        break;
                    }
                }

                if (greaterDist > 0.01f && greaterDist < .8f) {
                    rampDisplacement = new Vector2(Math.Sign(dX) * (Math.Abs(dX) - greaterDist), 0f);
                    return true;
                }

                Rectangle bodyBounds = Body.Bounds;

                Vector2[] ascRampChecks = new Vector2[] {
                    new Vector2(0f, -1f),
                    new Vector2(1f, -1f)
                };

                if (HandleRamp(dX, ascRampChecks, ascdCollisionList, false, out rampDisplacement)) {
                    Body.MoveBufferY = 0;
                    IsOnAscendingRamp = true;
                    IsOnDescendingRamp = false;

                    if (wasOnDescendingRamp) {
                        IsLeavingRamp = true;
                        _previousRampDirection = 1;
                        OnLeaveRamp?.Invoke(1);
                        ExtraMaxVelocity = Vector2.Zero;
                    }

                    if (!wasOnAscendingRamp) {
                        IsLeavingRamp = false;
                        _isEnteringRamp = true;
                        OnTouchRamp?.Invoke(-1);
                        ExtraMaxVelocity = new Vector2(AscendingRampVelocityModifier, 0f);
                        RampAcceleration = MaxVelocity.X / (Time.MiliToSec * TimeToAchieveMaxVelocityAtAscendingRamp);
                        _rampCurrentAcceleration = 0f; // it'll be defined at Integrate step
                    }

                    return true;
                }
            }
#endif

#if !DISABLE_DESCENDING_RAMP
            if (Body.CollidesMultiple(Body.Position + new Vector2(0f, 2f), CollisionTags, out CollisionList<Body> descdCollisionList)) {
                Rectangle bodyBounds = Body.Bounds;
                Size halfBodySize = bodyBounds.Size / 2f;

                Vector2[] descRampChecks = new Vector2[] {
                    new Vector2(-2f, 2f),
                    new Vector2(-3f, 1f),
                    // one quarter to help check ramps
                    new Vector2(-(halfBodySize.Width / 2f), 0f),
                    // when leaving desc ramp
                    new Vector2(-halfBodySize.Width, 0f),
                    new Vector2(-halfBodySize.Width, 1f),
                    new Vector2(-halfBodySize.Width, 2f),
                    new Vector2(-(bodyBounds.Width + 1f), 0f)
                };

                if (HandleRamp(dX, descRampChecks, descdCollisionList, true, out rampDisplacement)) {
                    Body.MoveBufferY = 0;
                    IsOnAscendingRamp = false;
                    IsOnDescendingRamp = true;

                    if (wasOnAscendingRamp) {
                        IsLeavingRamp = true;
                        _previousRampDirection = -1;
                        OnLeaveRamp?.Invoke(-1);
                        ExtraMaxVelocity = Vector2.Zero;
                    }

                    if (!wasOnDescendingRamp) {
                        IsLeavingRamp = false;
                        _isEnteringRamp = true;
                        OnTouchRamp?.Invoke(1);
                        ExtraMaxVelocity = new Vector2(DescendingRampVelocityModifier, 0f);
                        RampAcceleration = MaxVelocity.X / (Time.MiliToSec * TimeToAchieveMaxVelocityAtDescendingRamp);
                        _rampCurrentAcceleration = 0f; // it'll be defined at Integrate step
                    }

                    return true;
                }
            }
#endif

            rampDisplacement = Vector2.Zero;
            SmoothRampVelocity(ref currentVelocity);
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

            Vector2 rampMoveNormal = Math.Sign(dX) > 0 ? _rampNormal.PerpendicularCW() : _rampNormal.PerpendicularCCW();
            float rampDisplacementDistance = rampMoveNormal.Projection(new Vector2(dX, 0f));

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
            if (collisionInfo.Subject.Shape != null) {
                if (collisionInfo.Subject.Shape is GridShape gridShape) {
                    foreach (Vector2 rampPositionCheck in positionsToCheck) {
                        (int gridColumn, int gridRow) = gridShape.ConvertPosition(collisionInfo.Subject.Position, rampPositionCheck);
                        ref GridShape.TileShape tileShape = ref gridShape.GetTileInfo(gridColumn, gridRow);

                        if (tileShape is GridShape.BoxTileShape) {
                            //boxTileShape.CreateCollisionPolygon(gridShape, collInfo.Subject.Position, gridColumn, gridRow);
                            // BoxTileShape will always be a straight wall or ground
                            // in this case it'll be 90 degree wall
                        } else if (tileShape is GridShape.PolygonTileShape polygonTileShape) {
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
                        }
                    }
                }
            }

            rampNormal = Vector2.Zero;
            return false;
        }

        private void Fall() {
            // ledge jump
            _ledgeJumpTime = 0;
            if (IsOnRamp || IsLeavingRamp) {
                _currentLedgeJumpMaxTime = RampLedgeJumpMaxTime;
            } else {
                _currentLedgeJumpMaxTime = LedgeJumpMaxTime;
            }

            ClearRampState();
            IsFalling = true;
            OnGround = IsJumping = IsStillJumping = _canKeepCurrentJump = false;
            OnFallingBegin();
        }

        private void ClearRampState() {
            if (!IsOnRamp) {
                return;
            }

            IsOnRamp = false;
            _rampNormal = Vector2.Zero;
            _internal_isGravityEnabled = true;
            _isRampAccelerationApplied = false;

            IsLeavingRamp = 
                _justLeavedRamp = true;

            if (IsOnDescendingRamp) {
                _previousRampDirection = 1;
                OnLeaveRamp?.Invoke(1);
                ExtraMaxVelocity = Vector2.Zero;
            } else {
                _previousRampDirection = -1;
                OnLeaveRamp?.Invoke(-1);
                ExtraMaxVelocity = Vector2.Zero;
            }

            IsOnDescendingRamp = IsOnAscendingRamp = false;
        }

        private void SmoothRampVelocity(ref Vector2 currentVelocity) {
            if (!IsOnRamp) {
                return;
            }

            float realXVelocity = Math.Abs(_rampNormal.X) * Math.Abs(currentVelocity.X);

            if (IsOnAscendingRamp) {
                realXVelocity *= SmoothLeavingAscendingRampVelocityCuttoff;
            } else if (IsOnDescendingRamp) {
                realXVelocity *= SmoothLeavingDescendingRampVelocityCuttoff;
            }

            currentVelocity = new Vector2(
                Math.Sign(Velocity.X) * realXVelocity,
                Velocity.Y
            );

            _rampCurrentAcceleration = Math.Distance(Math.Abs(BaseMaxVelocity.X), realXVelocity) / 0.2f;
        }

        #endregion Private Methods
    }
}
