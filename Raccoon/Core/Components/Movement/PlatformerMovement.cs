using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        #region Public Members

        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = 200; // in miliseconds
        public static uint JumpInputBufferTime = 200; // milliseconds

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

        // jump
        private bool _canJump = true, _canKeepCurrentJump = true, _requestedJump;
        private int _jumpMaxY, _ledgeJumpTime;
        private uint _lastTimeFirstRequestToJump;

        // ramp movement
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

            if (Body.Shape == null) {
                return;
            }

            if (OnGround && !_isWalkingOnRamp && !IsJumping) {
                // checks if it's touching the ground
                if (!Physics.Instance.QueryCollision(Body.Shape, Body.Position + Vector2.Down, CollisionTags, out ContactList contacts)
                  || !contacts.Contains(c => c.PenetrationDepth > 0f && Helper.InRangeLeftExclusive(Vector2.Down.Projection(c.Normal), .3f, 1f))) {
                    Fall();
                }
            }

            _isTryingToFallThrough = true;
            _applyFall = _isAboveSomething = false;
        }

        public override void PhysicsLateUpdate() {
            Body.CollidesMultiple(Body.Position + Vector2.Up, CollisionTags, out CollisionList<Body> topCollisionList);
            Body.CollidesMultiple(Body.Position + Vector2.Down, CollisionTags, out CollisionList<Body> bottomCollisionList);

            bool touchedBottom = false,
                 touchedTop = false;

            if (topCollisionList != null) {
                touchedBottom = bottomCollisionList.Contains(ci => ci.Contacts.Contains(c => c.PenetrationDepth > 0f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .3f, 1f)));
            }

            if (bottomCollisionList != null) {
                touchedTop = topCollisionList.Contains(ci => ci.Contacts.Contains(c => c.PenetrationDepth > 0f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Up), .3f, 1f)));
            }

            if (_isTryingToFallThrough && _applyFall) {
                Fall();
            }

            if (touchedBottom) { 
                if (!_isTryingToFallThrough) {
                    // falling and reached the ground
                    if (!OnGround) {
                        OnGround = true;
                        IsStillJumping = IsJumping = IsFalling = false;
                        Jumps = MaxJumps;

                        if (!CanContinuousJump && _requestedJump && Body.Entity.Timer - _lastTimeFirstRequestToJump <= JumpInputBufferTime) {
                            _canKeepCurrentJump = true;
                        }

                        OnTouchGround();
                    }

                    Velocity = new Vector2(Velocity.X, 0f);
                }
            } else if (touchedTop) { 
                // moving up and reached a ceiling
                if (Velocity.Y < 0 && !_isTryingToFallThrough) {
                    IsJumping = _canKeepCurrentJump = false;
                    Velocity = new Vector2(Velocity.X, 0f);

                    if (!Physics.Instance.QueryCollision(Body.Shape, Body.Position + Vector2.Down, CollisionTags, out ContactList contacts)
                      || contacts.Contains(c => c.PenetrationDepth < 1f && Helper.InRangeLeftExclusive(Vector2.Dot(c.Normal, Vector2.Down), 0f, 1f))) {
                        IsFalling = true;
                        OnFallingBegin();
                    }
                }
            }

            base.PhysicsLateUpdate();
        }

        public override bool CanCollideWith(Vector2 collisionAxes, CollisionInfo<Body> collisionInfo) {
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

            return true;
        }

        public override void BodyCollided(Body otherBody, Vector2 collisionAxes, CollisionInfo<Body> hCollisionInfo, CollisionInfo<Body> vCollisionInfo) {
            base.BodyCollided(otherBody, collisionAxes, hCollisionInfo, vCollisionInfo);

            if (!_isTryingToFallThrough) {
                return;
            }

            if (!otherBody.Tags.HasAny(FallThroughTags)) {
                if (vCollisionInfo != null) {
                    _isTryingToFallThrough = false;
                }

                _applyFall = false;
                return;
            }

            if (vCollisionInfo == null) {
                vCollisionInfo = hCollisionInfo;
            }

            if (collisionAxes.Y > 0 && vCollisionInfo.Contacts.Contains(c => c.PenetrationDepth == 1f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .4f, 1f))) {
                if (CanFallThrough) {
                    _applyFall = true;
                } else if (_isTryingToFallThrough) {
                    _isTryingToFallThrough = false;
                }
            } else if (collisionAxes.Y >= 0) {
                if (vCollisionInfo.Contacts.Contains(c => c.PenetrationDepth == 0f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .4f, 1f))) {
                    _isAboveSomething = true;
                    _applyFall = false;
                }

                if (!_isAboveSomething && (otherBody.Shape is GridShape || Body.Bottom > otherBody.Top)) {
                    _applyFall = true;
                } 
            }
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

            _isWalkingOnRamp = CheckRamps(displacement.X, ref displacement);

            if (!_isWalkingOnRamp) {
                if (!Math.EqualsEstimate(ImpulseTime, 0f) && ImpulsePerSec.Y != 0f) {
                    // handling impulse
                    velocity.Y += ImpulsePerSec.Y * dt;
                }

                if (!OnGround) {
                    // apply gravity force
                    velocity.Y += GravityScale * GravityForce.Y * dt;
                }

                if (IsStillJumping) {
                    // apply jumping acceleration if it's jumping
                    velocity.Y -= Acceleration.Y * dt;
                }

                velocity.Y += Body.Force.Y * dt;
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
                if (IsJumping && !IsFalling && !_isWalkingOnRamp) {
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

        private bool CheckRamps(float displacementX, ref Vector2 displacement) {
            if (!OnGround || Math.EqualsEstimate(displacementX, 0f)) {
                return false;
            }

            float dX = displacementX + (float) Body.MoveBufferX;

            if (CheckAscendingRamp(dX, ref displacement)) {
                return true;
            }

            if (CheckDescendingRamp(dX, ref displacement)) {
                return true;
            }

            return false;
        }

        private Vector2 CalculateRampDisplacement(float dX, Contact contact) {
            int hSign = Math.Sign(dX);

            // movement direction normal
            Vector2 contactNormalPerp = hSign > 0 ? contact.Normal.PerpendicularCCW() : contact.Normal.PerpendicularCW();

            // projection of displacement, in horizontal axis, onto movement direction normal
            float displacementProjection = Vector2.Dot(new Vector2(dX, 0f), contactNormalPerp);

            // ramp movement displacement
            Vector2 rampMoveDisplacement = contactNormalPerp * displacementProjection;

            return rampMoveDisplacement;
        }

        private bool CheckAscendingRamp(float dX, ref Vector2 displacement) {
            // Ascending Ramp
            if (!Physics.Instance.QueryCollision(Body.Shape, Body.Position + new Vector2(dX, 0f) + AscendingRampCollisionCheckCorrection, CollisionTags, out ContactList ascContacts)
              || ascContacts.Count == 0) {
                return false;
            }

            int contactIndex = ascContacts.FindIndex(c => c.PenetrationDepth > 0f && Helper.InRange(Vector2.Down.Projection(c.Normal), .3f, 1f));

            // check if it's on a valid ascending ramp
            if (contactIndex <= -1) {
                return false;
            }

            Contact contact = ascContacts[contactIndex];
            Vector2 rampMoveDisplacement = CalculateRampDisplacement(dX, contact);

            // ascending ramp can't move downwards
            if (Math.Sign(rampMoveDisplacement.Y) > 0f) {
                return false;
            }
            
            // total displacement (added initial collision check vertical correction)
            displacement = rampMoveDisplacement;

            Body.MoveBufferX = 0;
            Debug.WriteLine($"Contacts: {contact}, displacement.x = {displacement.X}, rampslope: {Vector2.Dot(contact.Normal, Vector2.Down)}");
            /*Debug.WriteLine($"  perp: {contactNormalPerp}, l: {displacementProjection}, displacement: {rampMoveDisplacement}"); //, -penVec: {-contact.PenetrationVector}");*/

            return true;
        }

        private bool CheckDescendingRamp(float dX, ref Vector2 displacement) {
            // Descending Ramp
            Vector2 descendingCheck = new Vector2(Math.Clamp(dX, -1f, 1f), Math.Max(1.7f, Math.Abs(dX)));

            if (!Physics.Instance.QueryCollision(Body.Shape, Body.Position + descendingCheck, CollisionTags, out ContactList descContacts)
              || descContacts.Count == 0) {
                return false;
            }

            int contactIndex = descContacts.FindIndex(c => Helper.InRangeLeftExclusive(c.PenetrationDepth, 0f, descendingCheck.Y) 
                                                        && Helper.InRangeLeftExclusive(Vector2.Dot(c.Normal, Vector2.Down), 0f, 1f));

            // check if it's on a valid descending ramp
            if (contactIndex <= -1) {
                return false;
            }

            Contact contact = descContacts[contactIndex];
            float rampFactor = Vector2.Dot(contact.Normal, Vector2.Down);
            Vector2 rampMoveDisplacement = CalculateRampDisplacement(dX, contact);

            //Debug.WriteLine($"Contacts: {contact}, displacement.x = {displacement.X}, rampFactor: {rampFactor}\nrampMoveDisplacement: {rampMoveDisplacement}");

            // only handles rampFactor = 1f when already walking on a ramp
            if (!_isWalkingOnRamp && rampFactor == 1f) {
                return false;
            }

            // descending ramp can't move upwards
            if (Math.Sign(rampMoveDisplacement.Y) < 0) {
                return false;
            }

            bool ret = true;

            // HACK: special case when about to leave ramp and reach a flat ground (ramp factor = 1f)
            if (Math.EqualsEstimate(rampFactor, 1f)) {
                rampMoveDisplacement = new Vector2(Math.Sign(rampMoveDisplacement.X), 1f);
                ret = false;
            }

            // total displacement (added initial collision check vertical correction)
            displacement = new Vector2(rampMoveDisplacement.X, Math.Max(1f, rampMoveDisplacement.Y)); // force to go at least 1 pixel down (less than that and movement sticks out of tile)

            Body.MoveBufferX = 0;
            //Debug.WriteLine($"  perp: {contactNormalPerp}, l: {displacementProjection}, displacement: {rampMoveDisplacement}"); //, -penVec: {-contact.PenetrationVector}"); */
            return ret;
        }

        private void Fall() {
            IsFalling = true;
            OnGround = IsJumping = IsStillJumping = _canKeepCurrentJump = false;
            OnFallingBegin();
        }

        #endregion Private Methods
    }
}
