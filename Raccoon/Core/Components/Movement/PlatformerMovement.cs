﻿using Raccoon.Util;

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

        #endregion Public Properties

        #region Protected Properties

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
        }

        public override void PhysicsLateUpdate() {
            base.PhysicsLateUpdate();

            if (IsStillJumping && !OnAir) {
                Velocity = new Vector2(Velocity.X, 0f);
            }
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

                Velocity = new Vector2(Velocity.X, 0f);
            } else if (TouchedTop) { 
                // jumping and reached a ceiling
                if (IsJumping) {
                    IsJumping = false;
                    IsFalling = true;
                    _canKeepCurrentJump = false;
                    Velocity = new Vector2(Velocity.X, 0f);
                    OnFallingBegin();
                }
            }
        }

        public override Vector2 Integrate(float dt) {
            float horizontalVelocity = Velocity.X;
            if (Axis.X == 0f) { // stopping from movement, drag force applies
                horizontalVelocity = System.Math.Abs(horizontalVelocity) < Math.Epsilon ? 0f : horizontalVelocity * DragForce;
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && System.Math.Sign(Axis.X) != System.Math.Sign(horizontalVelocity)) { // snapping horizontal axis clears velocity
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) { // velocity increasing until MaxVelocity.X limit
                horizontalVelocity = Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else { // velocity increasing without a limit
                horizontalVelocity += System.Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            float verticalVelocity = Velocity.Y;

            // apply gravity force
            verticalVelocity += GravityScale * GravityForce.Y * dt;

            if (IsStillJumping) {
                // apply jumping acceleration if it's jumping
                verticalVelocity -= Acceleration.Y * dt;
            }

            Velocity = Body.Force * dt + new Vector2(horizontalVelocity, verticalVelocity);

            return Velocity * dt;
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
            if (distance.Y > 0) {
                // checks if it's moving down, so it's falling
                if (!IsFalling) { 
                    IsFalling = true;
                    OnGround = IsJumping = false;
                    OnFallingBegin();
                }
            } else if (distance.Y < 0) {
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
