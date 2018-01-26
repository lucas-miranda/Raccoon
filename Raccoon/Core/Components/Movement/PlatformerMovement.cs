using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformerMovement : Movement {
        public static Vector2 GravityForce;
        public static int LedgeJumpMaxTime = (int) (12f / 60f * 1000f);

        public event System.Action OnJumpBegin = delegate { },
                                   OnTouchGround = delegate { },
                                   OnFallingBegin = delegate { };

        private bool _canJump = true, _nextJumpReady = true, _requestedJump;
        private int _jumpMaxY, _ledgeJumpTime;

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
        public int MaxJumps { get; set; } = 1;
        public int Jumps { get; protected set; }
        public float JumpHeight { get; private set; }
        public float JumpExplosionRate { get; set; } = .5f;
        public float GravityScale { get; set; } = 1f;

        protected bool IsStillJumping { get; set; }

        public override void Update(int delta) {
            base.Update(delta);
            if (!CanContinuousJump && OnGround) {
                // continuous jump lock (must release and press jump button to jump again)
                if (!_nextJumpReady && !_requestedJump) {
                    _nextJumpReady = true;
                }

                _requestedJump = false;
            }

            IsStillJumping = false;
        }

        public override void FixedUpdate(float dt) {
            base.FixedUpdate(dt);
        }

        public override Vector2 HandleVelocity(Vector2 velocity, float dt) {
            float horizontalVelocity = velocity.X;
            if (Axis.X == 0f) {
                horizontalVelocity = System.Math.Abs(horizontalVelocity) < Math.Epsilon ? 0f : horizontalVelocity * DragForce;
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && System.Math.Sign(Axis.X) != System.Math.Sign(horizontalVelocity)) {
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) {
                horizontalVelocity = Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else {
                horizontalVelocity += System.Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            float verticalVelocity = velocity.Y;

            // apply gravity
            verticalVelocity += GravityScale * GravityForce.Y * dt;

            if (IsStillJumping) {
                // apply jumping acceleration (sure, if it's jumping)
                verticalVelocity -= Acceleration.Y * dt;
            }

            return new Vector2(horizontalVelocity, verticalVelocity);
        }

        public override void OnMoving(Vector2 distance) {
            if (distance.Y != 0) {
                // check if it's moving down, so it's falling
                if (!IsFalling && distance.Y > 0f) {
                    IsFalling = true;
                    OnGround = IsJumping = _nextJumpReady = false;
                    OnFallingBegin();
                }

                // checks if jump max distance has been reached
                if (IsStillJumping && OnAir) {
                    if (Body.Position.Y <= _jumpMaxY) {
                        _nextJumpReady = false;
                    }
                }
            }

            if (Axis.X == 0f) {
                return;
            }

            OnMove();
        }

        public override void OnCollide(Vector2 collisionAxes) {
            base.OnCollide(collisionAxes);
            if (collisionAxes.X != 0f) {
                Velocity = new Vector2(0f, Velocity.Y);
            }

            if (collisionAxes.Y > 0f) { // falling and reach the ground
                if (!OnGround) {
                    OnGround = true;
                    IsStillJumping = IsJumping = IsFalling = false;
                    Jumps = MaxJumps;
                    OnTouchGround();
                }

                Velocity = new Vector2(Velocity.X, 0f);
            } else if (collisionAxes.Y < 0f) { // jumping and reach a ceiling
                IsStillJumping = IsJumping = false;
                IsFalling = true;
                OnFallingBegin();
                Velocity = new Vector2(Velocity.X, 0f);
            }
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

            if (!CanJump || (!OnGround && _ledgeJumpTime > LedgeJumpMaxTime)) {
                return;
            }
            
            IsStillJumping = IsJumping = true;
            OnGround = IsFalling = false;
            Jumps--;
            _jumpMaxY = (int) (Body.Position.Y - JumpHeight);
            float jumpForce = Acceleration.Y * JumpExplosionRate;
            Velocity = new Vector2(Velocity.X, -jumpForce);
            OnJumpBegin();
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nForce: {Body.Force}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})\nOnGroud? {OnGround}; CanJump? {CanJump};\nIsJumping? {IsJumping}; IsFalling: {IsFalling}\nJumps: {Jumps}\nJump Height: {JumpHeight}\nIsStillJumping? {IsStillJumping}\nGravity Force: {GravityForce}\n\nnextJumpReady? {_nextJumpReady}, jumpMaxY: {_jumpMaxY}";
            Debug.DrawString(Camera.Current, new Vector2(16, Game.Instance.ScreenHeight / 2f), info);
            Debug.DrawLine(new Vector2(Body.Position.X - 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), new Vector2(Body.Position.X + 32, _jumpMaxY + Body.Shape.BoundingBox.Height / 2f), Graphics.Color.Yellow);
        }
    }
}
