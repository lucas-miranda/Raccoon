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
        /// Preferred to be values in [0, 90] range.
        /// </summary>
        public static Range AllowedRampElevation = new Range(0, 60); // in degrees (preferred to stay 

        public event System.Action OnJumpBegin = delegate { },
                                   OnTouchGround = delegate { },
                                   OnFallingBegin = delegate { };

        public delegate void RampEvent(int climbDirection);
        public event RampEvent OnTouchRamp, OnLeaveRamp;

        #endregion Public Members

        #region Private Members

        private static readonly Vector2 AscendingRampCollisionCheckCorrection = new Vector2(0f, -.25f);

        private static readonly Range AllowedSlopeFactorRange = Range.From(Math.Cos(AllowedRampElevation.Min), Math.Cos(AllowedRampElevation.Max));

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
  On Ramp? {22}
  Ascd? {23} Descd? {24}
  internal isGravityEnabled {25}

Fall Through
  Can Fall Through? {26}

  is trying to fall through? {27}
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
        private static readonly Vector2[] AscendingRampChecks = new Vector2[] {
            new Vector2(1f, -1f),
            new Vector2(0f, -1f)
        };

        private static readonly Vector2[] DescendingRampChecks = new Vector2[] {
            new Vector2(-2f, 2f),
            new Vector2(-3f, 1f),
            new Vector2(-4f, 0f)
        };

        private Vector2 _rampNormal;

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

        /// <summary>
        /// Is Body on a descending ramp.
        /// </summary>
        public bool IsOnDescendingRamp { get; private set; }

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
            string info = string.Format(
                DebugText,
                Axis, LastAxis, SnapHorizontalAxis, SnapVerticalAxis,
                Velocity, MaxVelocity, TargetVelocity, Acceleration,
                Body.Force, GravityForce * GravityScale,
                Enabled, CanMove,
                OnGround, IsFalling,
                Jumps, CanJump, IsJumping, JustJumped, IsStillJumping, JumpHeight, _canKeepCurrentJump, _jumpMaxY,
                IsOnRamp, IsOnAscendingRamp, IsOnDescendingRamp, _internal_isGravityEnabled,
                CanFallThrough, _isTryingToFallThrough
            );

            Debug.DrawString(null, new Vector2(10f, 10f), info);
            Debug.DrawLine(
                new Vector2(Body.Position.X - 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f),
                new Vector2(Body.Position.X + 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f),
                Graphics.Color.Yellow
            );

            //Debug.DrawString(Debug.Transform(Body.Position - new Vector2(16)), $"Impulse Time: {ImpulseTime}\n(I/s: {ImpulsePerSec})");

            /*
            int direction = Math.Sign(LastAxis.X);
            Vector2 realign = direction < 0 ? new Vector2(-1f, 0f) : Vector2.Zero;
            Rectangle boundingBox = Body.Shape.BoundingBox;

            foreach (Vector2 currentRampCheck in AscendingRampChecks) {
                Vector2 pos = Body.Position + new Vector2(direction * (boundingBox.Width / 2f + currentRampCheck.X), boundingBox.Height / 2f + currentRampCheck.Y) + realign;
                Debug.DrawRectangle(new Rectangle(pos, Size.Unit), Graphics.Color.Red);
            }

            foreach (Vector2 currentRampCheck in DescendingRampChecks) {
                Vector2 pos = Body.Position + new Vector2(direction * (boundingBox.Width / 2f + currentRampCheck.X), boundingBox.Height / 2f + currentRampCheck.Y) + realign;
                Debug.DrawRectangle(new Rectangle(pos, Size.Unit), Graphics.Color.Blue);
            }
            */
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

            bool isAboveSomething = Body.CollidesMultiple(Body.Position + Vector2.Down, CollisionTags, out CollisionList<Body> contactsBelow);
            bool isBelowSomething = Body.CollidesMultiple(Body.Position + Vector2.Up, CollisionTags, out CollisionList<Body> contactsAbove);

            if (isAboveSomething) {
                foreach (CollisionInfo<Body> collisionInfo in contactsBelow) {
                    foreach (Contact contact in collisionInfo.Contacts) {
                        if (Vector2.Dot(contact.Normal, Vector2.Down) <= .6f) {
                            continue;
                        }

                        if (IsOnRamp) {
                            if (Helper.InRange(contact.PenetrationDepth, 0f, 1f)) {
                                _touchedBottom = true;
                                break;
                            }
                        } else {
                            if (Helper.InRangeLeftExclusive(contact.PenetrationDepth, 0f, 1f)) {
                                _touchedBottom = true;
                                break;
                            }
                        }
                    }

                    if (_touchedBottom) {
                        break;
                    }
                }
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
                    } else if (!CanContinuousJump && _requestedJump && Body.Entity.Timer - _lastTimeFirstRequestToJump <= JumpInputBufferTime) {
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
            if (OnGround && !_touchedBottom) {
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

            if (GravityEnabled && _internal_isGravityEnabled) {
                // apply gravity force
                velocity.Y += GravityScale * GravityForce.Y * dt;
            }

            if (IsStillJumping) {
                // apply jumping acceleration if it's jumping
                velocity.Y -= Acceleration.Y * dt;
            }

            velocity.Y += Body.Force.Y * dt;
            displacement.Y += velocity.Y * dt;

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

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            if (distance.Y > 0f) {
                // if it's moving down then it's falling
                if (IsJumping && !IsFalling) {
                    Fall();
                }
            } else if (distance.Y < 0f) {
                if (IsStillJumping) {
                    if (!IsJumping) {
                        IsJumping = JustJumped = true;
                        OnGround = IsFalling = false;
                        Jumps--;
                        OnJumpBegin();
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

            OnMove();
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

            if (!OnGround || Math.EqualsEstimate(displacement.X, 0f)) {
                rampDisplacement = Vector2.Zero;
                ClearRampState();
                return false;
            }

            // true horizontal displacement value
            float dX = displacement.X + (float) Body.MoveBufferX;
            Body.MoveBufferY = 0;

#if !DISABLE_ASCENDING_RAMP
            if (Body.CollidesMultiple(Body.Position + new Vector2(Math.Sign(dX), 0f), CollisionTags, out CollisionList<Body> ascdCollisionList)) {
                if (HandleRamp(dX, AscendingRampChecks, ascdCollisionList, directionSameAsNormal: false, out rampDisplacement)) {
                    IsOnAscendingRamp = true;
                    IsOnDescendingRamp = false;

                    if (wasOnDescendingRamp) {
                        OnLeaveRamp?.Invoke(1);
                    }

                    if (!wasOnAscendingRamp) {
                        OnTouchRamp?.Invoke(-1);
                    }

                    return true;
                }
            }
#endif

#if !DISABLE_DESCENDING_RAMP
            if (Body.CollidesMultiple(Body.Position + new Vector2(Math.Sign(dX) * .5f, 2f), CollisionTags, out CollisionList<Body> descdCollisionList)) {
                if (HandleRamp(dX, DescendingRampChecks, descdCollisionList, directionSameAsNormal: true, out rampDisplacement)) {
                    IsOnAscendingRamp = false;
                    IsOnDescendingRamp = true;

                    if (wasOnAscendingRamp) {
                        OnLeaveRamp?.Invoke(-1);
                    }

                    if (!wasOnDescendingRamp) {
                        OnTouchRamp?.Invoke(1);
                    }

                    return true;
                }
            }
#endif

            rampDisplacement = Vector2.Zero;
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
            float rampDisplacementDistance = Vector2.Dot(new Vector2(dX, 0f), rampMoveNormal);
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

            IsOnRamp = false;
            _rampNormal = Vector2.Zero;
            _internal_isGravityEnabled = true;
            OnLeaveRamp?.Invoke(IsOnDescendingRamp ? 1 : -1);
            IsOnDescendingRamp = IsOnAscendingRamp = false;
        }

        #endregion Private Methods
    }
}
