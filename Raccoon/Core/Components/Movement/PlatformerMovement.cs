using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = 100; // in miliseconds

        public event System.Action OnJumpBegin = delegate { },
                                   OnTouchGround = delegate { },
                                   OnFallingBegin = delegate { };

        // jump
        private bool _canJump = true, _nextJumpReady = true, _requestedJump;
        private int _jumpMaxY, _ledgeJumpTime;

        // ramp movement
        private bool _lookingForRamp, _walkingOnRamp, _waitingForNextRampCollision, _canApplyRampCorrection;
        private int _searchRampX, _searchRampY;

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

        public bool OnGround { get; protected set; }
        public bool OnAir { get { return !OnGround; } }
        public bool CanJump { get { return _canJump && Jumps > 0; } set { _canJump = value; } }
        public bool IsJumping { get; protected set; }
        public bool IsFalling { get; protected set; } = true;
        public bool CanContinuousJump { get; set; } = false;
        public bool JustJumped { get; private set; }
        public int MaxJumps { get; set; } = 1;
        public int Jumps { get; protected set; }
        public float JumpHeight { get; private set; }
        public float JumpExplosionRate { get; set; } = .5f;
        public float GravityScale { get; set; } = 1f;

        protected bool IsStillJumping { get; set; }

        public override void OnRemoved() {
            base.OnRemoved();
            OnJumpBegin = OnTouchGround = OnFallingBegin = null;
        }

        public override void Update(int delta) {
            base.Update(delta);
            if (OnGround) {
                if (!CanContinuousJump) {
                    // continuous jump lock (must release and press jump button to jump again)
                    if (!_nextJumpReady && !_requestedJump && (Jumps > 0 || OnGround)) {
                        _nextJumpReady = true;
                    }

                    _requestedJump = false;
                }
            } else if (IsFalling && _ledgeJumpTime <= LedgeJumpMaxTime) {
                _ledgeJumpTime += delta;
            }

            IsStillJumping = false;
        }

        public override void FixedUpdate(float dt) {
            base.FixedUpdate(dt);
            if (_lookingForRamp && System.Math.Sign(Axis.X) != _searchRampX) {
                // reset ramp movement, if looking for a ramp and Axis.X differs from seach ramp X
                ResetRampMovement();
            }
        }

        public override void FixedLateUpdate(float dt) {
            if (_canApplyRampCorrection) {
                // not successfully made a expected horizontal move and a correction must be done
                if (_searchRampY < 0) {
                    Velocity = Vector2.Zero;
                    Body.Position = new Vector2(Body.Position.X, Body.Position.Y + 2);
                }

                ResetRampMovement();
                _canApplyRampCorrection = false;
            }

            if (IsStillJumping && !OnAir) {
                Velocity = new Vector2(Velocity.X, 0f);
            }
        }

        public override Vector2 HandleVelocity(Vector2 velocity, float dt) {
            float horizontalVelocity = velocity.X;
            if (Axis.X == 0f) { // stopping from movement, drag force applies
                horizontalVelocity = System.Math.Abs(horizontalVelocity) < Math.Epsilon ? 0f : horizontalVelocity * DragForce;
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && System.Math.Sign(Axis.X) != System.Math.Sign(horizontalVelocity)) { // snapping horizontal axis clears velocity
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) { // velocity increasing until MaxVelocity.X limit
                horizontalVelocity = _lookingForRamp && _searchRampY < 0 ? TargetVelocity.X : Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else { // velocity increasing without a limit
                horizontalVelocity += System.Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            float verticalVelocity = velocity.Y;

            if (_lookingForRamp && _searchRampY < 0) { // up ramp
                // wait until a horizontal collision happens to move body up again
                if (!_waitingForNextRampCollision) {
                    Body.Position = new Vector2(Body.Position.X, Body.Position.Y - 2);
                    _waitingForNextRampCollision = _canApplyRampCorrection = true;
                    _walkingOnRamp = false;
                }
            } else {
                // apply gravity force
                verticalVelocity += GravityScale * GravityForce.Y * dt;

                if (IsStillJumping) {
                    // apply jumping acceleration if it's jumping
                    verticalVelocity -= Acceleration.Y * dt;
                }

                if (_lookingForRamp && _searchRampY > 0) { // down ramp
                    // make a strong down force to make body gently walks on ramp
                    verticalVelocity = 8 / dt;
                    _waitingForNextRampCollision = _canApplyRampCorrection = true;
                    _walkingOnRamp = false;
                }
            }

            return new Vector2(horizontalVelocity, verticalVelocity);
        }

        public override void OnCollide(Vector2 collisionAxes) {
            base.OnCollide(collisionAxes);

            if (TouchedBottom) { 
                // falling and reach the ground
                if (!OnGround) {
                    OnGround = true;
                    IsStillJumping = IsJumping = IsFalling = false;
                    Jumps = MaxJumps;
                    OnTouchGround();
                }

                // down ramp collision check
                if (_lookingForRamp && _searchRampY > 0) {
                    _waitingForNextRampCollision = false;
                }

                Velocity = new Vector2(Velocity.X, 0f);
            } else if (TouchedTop) { 
                // jumping and reached a ceiling
                if (IsJumping) {
                    IsJumping = false;
                    IsFalling = true;
                    _nextJumpReady = false;
                    Velocity = new Vector2(Velocity.X, 0f);
                    OnFallingBegin();
                }
            }

            if (collisionAxes.X != 0f && _searchRampY < 1) { // do nothing if it's on a down ramp
                if (OnGround && !IsStillJumping) {
                    // start checking if it's on a up ramp
                    if (!_lookingForRamp) {
                        _lookingForRamp = true;
                        _walkingOnRamp = false;
                        _searchRampX = (int) collisionAxes.X;
                        _searchRampY = -1;
                    } else { 
                        // up ramp collision check
                        _waitingForNextRampCollision = false;
                    }
                } else {
                    // commom behavior, clears Velocity.X if touching horizontal walls on air
                    Velocity = new Vector2(0f, Velocity.Y);
                }
            }
        }

        public override void OnMoving(Vector2 distance) {
            if (_lookingForRamp && _searchRampY < 0) {
                // up ramp final cycle checking
                // checks for a valid horizontal movement
                // if it doesn't occurs then a correction will be made on the late update 
                if (System.Math.Sign(distance.X) == _searchRampX) {
                    ResetRampMovement();
                }
            } else if (OnGround && !IsStillJumping && _searchRampY < 1 && distance.X != 0) {
                // start checking if it's on a down ramp
                _lookingForRamp = true;
                _walkingOnRamp = false;
                _searchRampX = System.Math.Sign(distance.X);
                _searchRampY = 1;
            } else {
                if (distance.Y > 0) {
                    bool forceFallingState = false; // when down ramp correction must be applied
                    if (_canApplyRampCorrection && _searchRampY > 0 && _waitingForNextRampCollision) {
                        // down ramp final cycle checking (cade A)
                        // checks for a valid down ramp collision and immediately apply a correction
                        Velocity = new Vector2(Velocity.X, 0f);
                        Body.Position = new Vector2(Body.Position.X, Body.Position.Y - distance.Y);
                        //ResetRampMovement(); // doesn't need to reset ramp movement here
                        forceFallingState = true;
                    }

                    // standard checking for falling movement
                    // checks if it's moving down, so it's falling
                    if ((_searchRampY == 0 && !IsFalling) || forceFallingState) { 
                        ResetRampMovement();
                        IsFalling = true;
                        OnGround = IsJumping = false;
                        OnFallingBegin();
                    }
                } else if (distance.Y < 0) {
                    if (IsStillJumping) {
                        if (JustJumped) {
                            JustJumped = false;
                        }

                        if (!IsJumping) {
                            // reset ramp movement
                            ResetRampMovement();

                            IsJumping = JustJumped = true;
                            OnGround = IsFalling = false;
                            Jumps--;
                            OnJumpBegin();
                        }

                        // checks if jump max distance has been reached
                        if (OnAir && Body.Position.Y <= _jumpMaxY) {
                            _nextJumpReady = false;
                        }
                    }
                } else if (_searchRampY > 0 && !_waitingForNextRampCollision) {
                    // down ramp final cycle checking (case B)
                    // if tries to identify a down ramp and move distance is zero, already reached ground
                    ResetRampMovement();
                }
            }

            // platformer movement only triggers OnMove() on a horizontal movement
            if (Axis.X == 0f) {
                return;
            }

            OnMove();
        }

        public void Jump() {
            // continuous jump lock (must release and press jump button to jump again)
            _requestedJump = true;
            if (!_nextJumpReady) {
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

            // reset ramp movement
            ResetRampMovement();

            IsStillJumping = true;
            _jumpMaxY = (int) (Body.Position.Y - JumpHeight);
            Velocity = new Vector2(Velocity.X, -(Acceleration.Y * JumpExplosionRate));
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nForce: {Body.Force}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})\nOnGroud? {OnGround}; CanJump? {CanJump};\nIsJumping? {IsJumping}; IsFalling: {IsFalling}\nJumps: {Jumps}\nJump Height: {JumpHeight}\nIsStillJumping? {IsStillJumping}\nGravity Force: {GravityForce}\n\nnextJumpReady? {_nextJumpReady}, jumpMaxY: {_jumpMaxY}\nlookingForRamp? {_lookingForRamp}, walkingOnRamp: {_walkingOnRamp}, ramp X: {_searchRampX}, Y: {_searchRampY}\ncanApplyRampCorreciton? {_canApplyRampCorrection}";
            Debug.DrawString(Camera.Current, new Vector2(Game.Instance.ScreenWidth - 200f, Game.Instance.ScreenHeight / 2f), info);
            Debug.DrawLine(new Vector2(Body.Position.X - 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), new Vector2(Body.Position.X + 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), Graphics.Color.Yellow);
        }

        private void ResetRampMovement() {
            _lookingForRamp = _walkingOnRamp = _waitingForNextRampCollision = _canApplyRampCorrection = false;
            _searchRampX = _searchRampY = 0;
        }
    }
}
