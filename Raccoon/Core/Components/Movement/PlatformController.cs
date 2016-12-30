using Raccoon.Util;

namespace Raccoon.Components {
    public class PlatformMovement : Movement {
        public event System.Action OnMove, OnJumpStart, OnJumpEnd;

        private bool _canJump = true, _nextJumpReady = true, _requestedJump;
        private int _jumpDistanceBuffer;

        public PlatformMovement(Vector2 maxSpeed, Vector2 acceleration, Collider collider = null) : base(maxSpeed, acceleration, collider) {
            GravityForce = new Vector2(0, 450);
        }

        public bool OnGround { get; protected set; }
        public bool OnAir { get { return !OnGround; } }
        public bool CanJump { get { return _canJump && Jumps > 0; } set { _canJump = value; } }
        public bool IsJumping { get; protected set; }
        public bool IsFalling { get; protected set; } = true;
        public int MaxJumps { get; set; } = 1;
        public int Jumps { get; set; }
        public int JumpHeight { get; set; } = 19;
        public float JumpStartExplosionRate { get; set; } = 0.6f;
        public Vector2 GravityForce { get; set; }

        protected bool IsStillJumping { get; set; }

        public override void OnCollide(Vector2 moveDirection) {
            if (moveDirection.Y > 0) { // falling and reach the ground
                OnGround = true;
                IsStillJumping = IsJumping = IsFalling = false;
                Jumps = MaxJumps;
                _jumpDistanceBuffer = 0;
                OnJumpEnd?.Invoke();
            } else if (moveDirection.Y < 0) { // jumping and reach a ceiling
                IsStillJumping = IsJumping = false;
                IsFalling = true;
            }
        }

        public override void OnMoveUpdate(float dt) {
            int x = (int) Entity.X, y = (int) Entity.Y;
            float speedX = Speed.X, speedY = Speed.Y;
            Vector2 moveAxis = Axis == Vector2.Zero ? LastAxis : Axis;

            // determine TargetSpeed
            Vector2 oldTargetSpeed = TargetSpeed;
            TargetSpeed = new Vector2(Axis.X * MaxSpeed.X, MaxSpeed.Y);
            if (HorizontalAxisSnap && System.Math.Sign(oldTargetSpeed.X) != System.Math.Sign(TargetSpeed.X)) {
                speedX = 0;
            }

            // horizontal move
            speedX = Math.Approach(speedX, TargetSpeed.X, (Axis.X != 0 ? Acceleration.X : Acceleration.X * DragForce) * dt);

            if ((int) speedX != 0) {
                MoveHorizontalBuffer += speedX * dt;
                int hDir = System.Math.Sign(MoveHorizontalBuffer);
                if (Collider == null) {
                    int dist = (int) System.Math.Floor(System.Math.Abs(MoveHorizontalBuffer));
                    x += dist * hDir;
                    MoveHorizontalBuffer = Math.Approach(MoveHorizontalBuffer, 0, dist);
                } else {
                    while (System.Math.Abs(MoveHorizontalBuffer) >= 1) {
                        if (Collider.Collides(new Vector2(x + hDir, y), CollisionTags)) {
                            OnCollide(new Vector2(hDir, 0));
                            MoveHorizontalBuffer = 0;
                            break;
                        } else {
                            x += hDir;
                            MoveHorizontalBuffer = Math.Approach(MoveHorizontalBuffer, 0, 1);
                        }
                    }
                }
            }

            // vertical move
            float yAxis = 0; // vertical axis value
            if (IsStillJumping) {
                yAxis = -1;
                speedY = Math.Approach(Speed.Y, yAxis * TargetSpeed.Y, Acceleration.Y * dt);
                IsStillJumping = false;
            } else {
                if (OnGround) { 
                    // checks the ground existence
                    if (!Collider.Collides(new Vector2(x, y + 1), CollisionTags)) {
                        IsFalling = true;
                        OnGround = false;
                    }
                } else {
                    // falling & jump brake speed update
                    yAxis = 1;
                    speedY = Math.Approach(Speed.Y, yAxis * TargetSpeed.Y, GravityForce.Y * dt);

                    // reached jump max height
                    if (IsJumping && speedY >= 0) {
                        IsJumping = false;
                        IsFalling = true;
                        Speed = new Vector2(Speed.X, 0);
                        _nextJumpReady = false;
                    }
                }

                // continuous jump lock (must release and press jump button to jump again)
                if (!_nextJumpReady && !_requestedJump) {
                    _nextJumpReady = true;
                }

                _requestedJump = false;
            }

            if ((int) speedY != 0) {
                MoveVerticalBuffer += speedY * dt;
                int vDir = System.Math.Sign(MoveVerticalBuffer);
                if (Collider == null) {
                    int dist = (int) System.Math.Floor(System.Math.Abs(MoveVerticalBuffer));
                    y += dist * vDir;
                    MoveVerticalBuffer = Math.Approach(MoveVerticalBuffer, 0, dist);
                } else {
                    while (System.Math.Abs(MoveVerticalBuffer) >= 1) {
                        if (Collider.Collides(new Vector2(x, y + vDir), CollisionTags)) {
                            OnCollide(new Vector2(0, vDir));
                            speedY = MoveVerticalBuffer = 0;
                            break;
                        } else {
                            y += vDir;
                            MoveVerticalBuffer = Math.Approach(MoveVerticalBuffer, 0, 1);

                            // register jump distance to buffer
                            if (IsJumping) {
                                _jumpDistanceBuffer++;
                            }
                        }
                    }
                }
            }

            // update entity values
            Speed = new Vector2(speedX, speedY);
            Vector2 oldPosition = Entity.Position;
            Entity.Position = new Vector2(x, y);
            if (x != oldPosition.X || y != oldPosition.Y) {
                OnMove?.Invoke();
            }
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

            if (!CanJump) {
                return;
            }

            IsStillJumping = IsJumping = true;
            OnGround = IsFalling = false;
            Jumps--;
            _jumpDistanceBuffer = 0;
            Speed = new Vector2(Speed.X, -JumpStartExplosionRate * MaxSpeed.Y);
            OnJumpStart?.Invoke();
        }

        /*public override void DebugRender() {
            base.DebugRender();
            Debug.DrawString(false, new Vector2(5, 30), @"- Platform Movement -
Axis: {0}
Speed: {1}
OnGround: {2}, OnAir: {3}
CanJump: {4}
IsJumping: {5}, IsFalling: {6}
Jumps: {7}, MaxJumps: {8}
JumpHeight: {9}
JumpDistanceBuffer: {10}
NextJumpReady: {11}, RequestedJump: {12}", Axis, Speed, OnGround, OnAir, CanJump, IsJumping, IsFalling, Jumps, MaxJumps, JumpHeight, _jumpDistanceBuffer, _nextJumpReady, _requestedJump);
        }*/
    }
}
