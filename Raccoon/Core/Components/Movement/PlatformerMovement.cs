using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        #region Public Members

        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = 100; // in miliseconds

        public event System.Action OnJumpBegin = delegate { },
                                   OnTouchGround = delegate { },
                                   OnFallingBegin = delegate { };

        #endregion Public Members

        #region Private Members

        // jump
        private bool _canJump = true, _canKeepCurrentJump = true, _requestedJump;
        private int _jumpMaxY, _ledgeJumpTime;

        // ramp movement
        /*private bool _lookingForRamp, _walkingOnRamp, _waitingForNextRampCollision, _canApplyRampCorrection;
        private int _searchRampX, _searchRampY;*/

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
            if (OnGround) {
                if (!CanContinuousJump) {
                    // continuous jump lock (must release and press jump button to jump again)
                    if (!_canKeepCurrentJump && !_requestedJump && (Jumps > 0 || OnGround)) {
                        _canKeepCurrentJump = true;
                    }

                    _requestedJump = false;
                }
            } else if (IsFalling && _ledgeJumpTime <= LedgeJumpMaxTime) {
                _ledgeJumpTime += delta;
            }

            IsStillJumping = false;
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nForce: {Body.Force}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})\nOnGroud? {OnGround}; CanJump? {CanJump};\nIsJumping? {IsJumping}; IsFalling: {IsFalling}\nJumps: {Jumps}\nJump Height: {JumpHeight}\nIsStillJumping? {IsStillJumping}\nGravity Force: {GravityForce}\n\nnextJumpReady? {_canKeepCurrentJump}, jumpMaxY: {_jumpMaxY}"; //\nlookingForRamp? {_lookingForRamp}, walkingOnRamp: {_walkingOnRamp}, ramp X: {_searchRampX}, Y: {_searchRampY}\ncanApplyRampCorreciton? {_canApplyRampCorrection}";
            Debug.DrawString(Camera.Current, new Vector2(Game.Instance.ScreenWidth - 200f, Game.Instance.ScreenHeight / 2f), info);
            Debug.DrawLine(new Vector2(Body.Position.X - 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), new Vector2(Body.Position.X + 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), Graphics.Color.Yellow);
        }

        public override void PhysicsUpdate(float dt) {
            base.PhysicsUpdate(dt);

            if (Body.Shape == null) {
                return;
            }

            if (OnGround) {
                // checks if it's touching the ground
                if (Physics.Instance.QueryCollision(Body.Shape, Body.Position + Vector2.Down, CollisionTags, out Contact[] contacts)) {
                    foreach (Contact contact in contacts) {
                        if (Vector2.Dot(contact.Normal, Vector2.Down) <= 0f && contact.PenetrationDepth <= 1f) {
                            OnGround = false;
                        }
                    }
                } else {
                    OnGround = false;
                }
            }
        }

        public override void PhysicsLateUpdate() {
            base.PhysicsLateUpdate();

            /*if (IsStillJumping && !OnAir) {
                Velocity = new Vector2(Velocity.X, 0f);
            }*/
        }

        public override void OnCollide(Vector2 collisionAxes) {
            base.OnCollide(collisionAxes);

            if (TouchedBottom) { 
                // falling and reached the ground
                if (!OnGround) {
                    OnGround = true;
                    IsStillJumping = IsJumping = IsFalling = false;
                    Jumps = MaxJumps;
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

            // ramps
            /*if (TouchedRight) {
                // 1 - look for a ascending ramp
                if (Physics.Instance.QueryCollision(Body.Shape, Body.Position + new Vector2(2f * Math.Sign(Velocity.X), 0f), CollisionTags, out Contact[] contacts)) {
                    foreach (Contact contact in contacts) {
                        if (Vector2.Dot(contact.Normal, new Vector2(Math.Sign(Velocity.X), 1f)) <= 0f || contact.PenetrationDepth > 1f) {
                            OnGround = false;
                        }
                    }
                }
            }*/
        }

        public override Vector2 Integrate(float dt) {
            Vector2 displacement = Vector2.Zero;

            float horizontalVelocity = Velocity.X;
            if (Axis.X == 0f) { // stopping from movement, drag force applies
                horizontalVelocity = Math.EqualsEstimate(horizontalVelocity, 0f) ? 0f : horizontalVelocity * DragForce;
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && Math.Sign(Axis.X) != Math.Sign(horizontalVelocity)) { // snapping horizontal axis clears velocity
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) { // velocity increasing until MaxVelocity.X limit
                horizontalVelocity = Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else { // velocity increasing without a limit
                horizontalVelocity += Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            horizontalVelocity += Body.Force.X * dt;
            displacement.X = horizontalVelocity * dt;

            bool canCheckForRamp = true;
            float verticalVelocity = Velocity.Y;

            if (!OnGround) {
                // apply gravity force
                verticalVelocity += GravityScale * GravityForce.Y * dt;
                canCheckForRamp = false;
            }

            if (IsStillJumping) {
                // apply jumping acceleration if it's jumping
                verticalVelocity -= Acceleration.Y * dt;
                canCheckForRamp = false;
            }

            if (!canCheckForRamp) {
                verticalVelocity += Body.Force.Y * dt;
                displacement.Y = verticalVelocity * dt;
            } else if (!Math.EqualsEstimate(displacement.X, 0f)
              && Physics.Instance.QueryCollision(Body.Shape, Body.Position + new Vector2(displacement.X, 0f), CollisionTags, out Contact[] contacts)
              && contacts.Length > 0) {
                Contact contact = contacts[0];
                Debug.WriteLine($"Contacts: {contact}, displacement.x = {displacement.X}");

                // check if it's on a valid ascending ramp
                float rampSlope = Vector2.Dot(contact.Normal, Vector2.Down);
                if (contact.PenetrationDepth > 0f
                  && Helper.InRange(rampSlope, .35f, 1f)) {
                    float rampAngle = -Math.Angle(new Vector2(contact.Normal.X, -contact.Normal.Y));
                    Vector2 rampMoveDisplacement = Math.Rotate(new Vector2(displacement.X, 0f), displacement.X > 0f ? -rampAngle : (-rampAngle - 180));

                    // hack to ensure a smooth movement when going exclusively upwards
                    if (Math.EqualsEstimate(rampMoveDisplacement.X, 0f)) {
                        rampMoveDisplacement.Y = Math.Clamp(rampMoveDisplacement.Y, -1f, 1f);
                    }

                    displacement = rampMoveDisplacement;
                    Debug.WriteLine($"  angle: {rampAngle}, displacement: {rampMoveDisplacement}");
                }
            }

            Velocity = new Vector2(horizontalVelocity, verticalVelocity);
            return displacement;
        }

        public void Jump() {
            // continuous jump lock (must release and press jump button to jump again)
            _requestedJump = true;
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

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            if (distance.Y > 0f) {
                // if it's moving down then it's falling
                if (!IsFalling) { 
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

        /*private void ResetRampMovement() {
            _lookingForRamp = _walkingOnRamp = _waitingForNextRampCollision = _canApplyRampCorrection = false;
            _searchRampX = _searchRampY = 0;
        }*/

        #endregion Private Methods
    }
}
