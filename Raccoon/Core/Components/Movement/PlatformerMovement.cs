namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = (int) (12f / 60f * 1000f);

        public event System.Action OnJumpBegin = delegate { },
                                   OnJumpEnd = delegate { },
                                   OnFallingBegin = delegate { };

        private bool _canJump = true, _nextJumpReady = true, _requestedJump;
        private int _jumpDistanceBuffer, _ledgeJumpTime;

        /*/// <summary>
        /// A component that handles platformer movement.
        /// </summary>
        /// <param name="horizontalAcceleration">Horizontal speed increase.</param>
        public PlatformerMovement(float horizontalAcceleration, float jumpWidth, float jumpHeight) : base(new Vector2(horizontalAcceleration, 0)) {
            JumpWidth = jumpWidth;
            JumpHeight = jumpHeight;
            JumpHorizontalDistanceToPeak = JumpWidth / 2f;
            OnFallingBegin += () => {
                _ledgeJumpTime = 0;
            };
        }*/

        /// <summary>
        /// A component that handles platformer movement.
        /// </summary>
        /// <param name="maxHorizontalVelocity">Max horizontal velocity.</param>
        /// <param name="horizontalAcceleration">Horizontal speed increase.</param>
        public PlatformerMovement(float maxHorizontalVelocity, float horizontalAcceleration, float jumpWidth, float jumpHeight) : base(new Vector2(maxHorizontalVelocity, 0), new Vector2(horizontalAcceleration, 0)) {
            SnapHorizontalAxis = true;
            JumpWidth = jumpWidth;
            JumpHeight = jumpHeight;
            JumpHorizontalDistanceToPeak = JumpWidth / 2f;
            OnFallingBegin += () => {
                _ledgeJumpTime = 0;
            };
        }

        /// <summary>
        /// A component that handles platformer movement.
        /// </summary>
        /// <param name="maxHorizontalVelocity">Max horizontal velocity.</param>
        /// <param name="timeToAchieveMaxVelocity">Time (in miliseconds) to reach max velocity.</param>
        public PlatformerMovement(float maxHorizontalVelocity, int timeToAchieveMaxVelocity, float jumpWidth, float jumpHeight) : base(new Vector2(maxHorizontalVelocity, 0), timeToAchieveMaxVelocity) {
            SnapHorizontalAxis = true;
            JumpWidth = jumpWidth;
            JumpHeight = jumpHeight;
            JumpHorizontalDistanceToPeak = JumpWidth / 2f;
            OnFallingBegin += () => {
                _ledgeJumpTime = 0;
            };
        }

        public bool OnGround { get; protected set; }
        public bool OnAir { get { return !OnGround; } }
        public bool CanJump { get { return _canJump && Jumps > 0; } set { _canJump = value; } }
        public bool IsJumping { get; protected set; }
        public bool IsFalling { get; protected set; } = true;
        public int MaxJumps { get; set; } = 1;
        public int Jumps { get; protected set; } = 1;
        public float JumpWidth { get; private set; }
        public float JumpHeight { get; private set; }
        public float JumpHorizontalDistanceToPeak { get; private set; }
        //public float JumpStartExplosionRate { get; set; } = 0.6f;
        public float GravityScale { get; set; } = 1f;

        protected bool IsStillJumping { get; set; }

        public override void Update(int delta) {
            base.Update(delta);
            IsStillJumping = false;

            float verticalVelocity = Body.Velocity.Y;
            if (!IsStillJumping) {
                //verticalMoveDirection = -1;
                //speedY = Math.Approach(CurrentSpeed.Y, verticalMoveDirection * TargetSpeed.Y, Acceleration.Y * dt);
                //Body.ApplyImpulse(new Vector2(0, (-2 * JumpHeight * MaxVelocity.X) / JumpHorizontalDistanceToPeak)); // (2hVx) / (Xh)

                Body.ApplyForce(GravityForce);
                if (OnGround) {
                    // checks the ground existence
                    if (verticalVelocity > 0) {
                        IsFalling = true;
                        OnGround = false;
                        OnFallingBegin.Invoke();
                    }
                } else {
                    // falling & jump brake speed update
                    //verticalMoveDirection = 1;
                    //speedY = Math.Approach(CurrentSpeed.Y, verticalMoveDirection * TargetSpeed.Y, GravityForce.Y * dt);
                    //Body.ApplyImpulse(new Vector2(0, (-2 * JumpHeight * MaxVelocity.X) / JumpHorizontalDistanceToPeak)); // (2hVx) / (Xh)
                    //_ledgeJumpTime += (int) (dt * 1000);

                    // reached jump max height
                    if (IsJumping && verticalVelocity >= 0) {
                        IsJumping = false;
                        IsFalling = true;
                        //? Speed = new Vector2(CurrentSpeed.X, 0);
                        _nextJumpReady = false;
                        OnFallingBegin.Invoke();
                    }
                }

                // continuous jump lock (must release and press jump button to jump again)
                if (!_nextJumpReady && !_requestedJump) {
                    _nextJumpReady = true;
                }

                _requestedJump = false;
            }
        }

        public override Vector2 HandleVelocity(Vector2 velocity, float dt) {
            float horizontalVelocity = velocity.X;
            if (Axis.X == 0f) {
                horizontalVelocity *= DragForce;
            } else if (SnapHorizontalAxis && System.Math.Sign(Axis.X) != System.Math.Sign(velocity.X)) {
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) {
                horizontalVelocity = Util.Math.Clamp(horizontalVelocity, -MaxVelocity.X, MaxVelocity.X);
            }

            float verticalVelocity = velocity.Y;

            // vertical move
            //float verticalMoveDirection = 0; // vertical axis direction
            //Body.ApplyForce(GravityForce);

            return new Vector2(horizontalVelocity, verticalVelocity);
        }

        public override void OnCollide() {
            base.OnCollide();
            Vector2 velocity = Body.Velocity;
            if (velocity.Y > 0) { // falling and reach the ground
                OnGround = true;
                IsStillJumping = IsJumping = IsFalling = false;
                Jumps = MaxJumps;
                _jumpDistanceBuffer = 0;
                OnJumpEnd.Invoke();
            } else if (velocity.Y < 0) { // jumping and reach a ceiling
                IsStillJumping = IsJumping = false;
                IsFalling = true;
                OnFallingBegin.Invoke();
            } else {
                OnGround = true;
                IsStillJumping = IsJumping = IsFalling = false;
                Jumps = MaxJumps;
                _jumpDistanceBuffer = 0;
            }
        }

        public override void Move(Vector2 axis) {
            base.Move(axis);
            if (NextAxis == Vector2.Zero) {
                return;
            }

            Body.ApplyImpulse(NextAxis * Acceleration);
        }

        public void Jump() {
            // continuous jump lock (must release and press jump button to jump again)
            _requestedJump = true;
            if (!_nextJumpReady) {
                return;
            }

            // keep going up, if you not reach the max jump height
            if (IsJumping && _jumpDistanceBuffer < JumpHeight) {
                IsStillJumping = true;
                return;
            }

            if (!CanJump || (!OnGround && _ledgeJumpTime > LedgeJumpMaxTime)) {
                return;
            }
            
            IsStillJumping = IsJumping = true;
            OnGround = IsFalling = false;
            Jumps--;
            _jumpDistanceBuffer = 0;
            //var jumpValue = (-2 * JumpHeight * MaxVelocity.X) / JumpHorizontalDistanceToPeak;
            float jumpValue = 100 * JumpHeight;
            //Speed = new Vector2(CurrentSpeed.X, -JumpStartExplosionRate * MaxSpeed.Y);
            Body.ApplyImpulse(new Vector2(0, -jumpValue)); // v0 = (2hVx) / (Xh)
            //Body.LastPosition = new Vector2(Body.LastPosition.X, Body.Position.Y);
            //Body.Position = new Vector2(Body.Position.X, Body.Position.Y + jumpValue);
            OnJumpBegin.Invoke();
            Debug.WriteLine("jump = " + jumpValue);
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})\nOnGroud? {OnGround}; CanJump? {CanJump}; IsJumping? {IsJumping}\nJumps: {Jumps}\nJump (W: {JumpWidth}, H: {JumpHeight}, DtP: {JumpHorizontalDistanceToPeak})\nIsStillJumping? {IsStillJumping}\nGravity Force: {GravityForce}\n\nnextJumpReady? {_nextJumpReady}, justDistanceBuffer: {_jumpDistanceBuffer}";
            Debug.DrawString(Camera.Current, new Vector2(16, Game.Instance.ScreenHeight / 2f), info);
        }
    }
}
