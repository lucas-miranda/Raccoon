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

        // jump
        private bool _canJump = true, _canKeepCurrentJump = true, _requestedJump;
        private int _jumpMaxY, _ledgeJumpTime;
        private uint _lastTimeFirstRequestToJump;

        // ramp movement
        private bool _isWalkingOnRamp;

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
        /// Drag force applied when on ground
        /// </summary>
        public float GroundDragForce { get; set; }

        /// <summary>
        /// Drag force applied when on air
        /// </summary>
        public float AirDragForce { get; set; }

        #endregion Public Properties

        #region Protected Properties

        /// <summary>
        /// If jump condition is renewed.
        /// </summary>
        protected bool IsStillJumping { get; set; }

        #endregion Protected Properties

        #region Public Methods

        public override void Update(int delta) {
            base.Update(delta);
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

            if (IsFalling && _ledgeJumpTime <= LedgeJumpMaxTime) {
                _ledgeJumpTime += delta;
            }

            IsStillJumping = false;
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nForce: {Body.Force}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})\nOnGroud? {OnGround}; CanJump? {CanJump};\nIsJumping? {IsJumping}; IsFalling: {IsFalling}\nJumps: {Jumps}\nJump Height: {JumpHeight}\nIsStillJumping? {IsStillJumping}\nGravity Force: {GravityForce}\n\nnextJumpReady? {_canKeepCurrentJump}, jumpMaxY: {_jumpMaxY}\n\n- Ramps\nisWalkingOnRamp? {_isWalkingOnRamp}";
            Debug.DrawString(Camera.Current, new Vector2(Game.Instance.Width - 200f, Game.Instance.Height / 2f), info);
            Debug.DrawLine(new Vector2(Body.Position.X - 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), new Vector2(Body.Position.X + 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), Graphics.Color.Yellow);

            //Debug.DrawString(Debug.Transform(Body.Position - new Vector2(16)), $"Impulse Time: {ImpulseTime}\n(I/s: {ImpulsePerSec})");
        }

        public override void PhysicsUpdate(float dt) {
            base.PhysicsUpdate(dt);

            if (Body.Shape == null) {
                return;
            }

            if (OnGround && !_isWalkingOnRamp) {
                // checks if it's touching the ground
                if (Physics.Instance.QueryCollision(Body.Shape, Body.Position + Vector2.Down, CollisionTags, out Contact[] contacts)
                  && contacts.Length > 0) {
                    int contactIndex = System.Array.FindIndex(contacts, c => /*c.PenetrationDepth > 0f &&*/ Helper.InRangeLeftExclusive(Vector2.Dot(c.Normal, Vector2.Down), 0f, 1f));

                    if (contactIndex < 0) {
                        OnGround = false;
                    }
                } else {
                    OnGround = false;
                }
            }
        }

        public override void PhysicsLateUpdate() {
            base.PhysicsLateUpdate();
        }

        public override void OnCollide(Vector2 collisionAxes) {
            base.OnCollide(collisionAxes);

            if (TouchedBottom) { 
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
            } else if (TouchedTop) { 
                // jumping and reached a ceiling
                if (IsJumping) {
                    IsJumping = _canKeepCurrentJump = false;
                    IsFalling = true;
                    Velocity = new Vector2(Velocity.X, 0f);
                    OnFallingBegin();
                }
            }

            if (!OnGround) {
                return;
            }
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

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            if (distance.Y > 0f) {
                // if it's moving down then it's falling
                if (!IsFalling && !_isWalkingOnRamp) { 
                    IsFalling = true;
                    OnGround = IsJumping = IsStillJumping = _canKeepCurrentJump = false;
                    OnFallingBegin();
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
            if (!Physics.Instance.QueryCollision(Body.Shape, Body.Position + new Vector2(dX, 0f) + AscendingRampCollisionCheckCorrection, CollisionTags, out Contact[] ascContacts)
              || ascContacts.Length == 0) {
                return false;
            }

            int contactIndex = System.Array.FindIndex(ascContacts, c => c.PenetrationDepth > 0f && Helper.InRange(Vector2.Dot(c.Normal, Vector2.Down), .3f, 1f));

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
            /*Debug.WriteLine($"Contacts: {contact}, displacement.x = {displacement.X}, rampslope: {Vector2.Dot(contact.Normal, Vector2.Down)}");
            /*Debug.WriteLine($"  perp: {contactNormalPerp}, l: {displacementProjection}, displacement: {rampMoveDisplacement}"); //, -penVec: {-contact.PenetrationVector}");*/

            return true;
        }

        private bool CheckDescendingRamp(float dX, ref Vector2 displacement) {
            // Descending Ramp
            Vector2 descendingCheck = new Vector2(Math.Clamp(dX, -1f, 1f), Math.Max(1.7f, Math.Abs(dX)));

            if (!Physics.Instance.QueryCollision(Body.Shape, Body.Position + descendingCheck, CollisionTags, out Contact[] descContacts)
              || descContacts.Length == 0) {
                return false;
            }

            int contactIndex = System.Array.FindIndex(descContacts, c => Helper.InRangeLeftExclusive(c.PenetrationDepth, 0f, descendingCheck.Y) 
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

        #endregion Private Methods
    }
}
