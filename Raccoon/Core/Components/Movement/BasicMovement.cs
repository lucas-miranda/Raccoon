using Raccoon.Util;

namespace Raccoon.Components {
    public class BasicMovement : Movement {
        #region Constructors

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 acceleration) : base(acceleration) {
            DragForce = CalculateDragForceNormalized(.9f);
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 maxVelocity, Vector2 acceleration) : base(maxVelocity, acceleration) {
            DragForce = CalculateDragForceNormalized(.9f);
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="timeToAchieveMaxSpeed">Time (in miliseconds) to reach max velocity.</param>
        public BasicMovement(Vector2 maxVelocity, int timeToAchieveMaxSpeed) : base(maxVelocity, timeToAchieveMaxSpeed) {
            DragForce = CalculateDragForceNormalized(.9f);
            SnapAxes = true;
        }

        #endregion Constructors

        #region Public Methods

        public override Vector2 Integrate(float dt) {
            bool combinedAxis = false;

            Vector2 axis = Axis;

            Vector2 targetRemaining = Vector2.Zero,
                    moveTargetAxis = Vector2.Zero;

            if (IsMovingToTargetPosition) {
                // estimate interations of drag force to full stop

                targetRemaining = MoveTargetPosition.Value - Body.Position;
                moveTargetAxis = targetRemaining.Normalized();


                // horizontal

                int i = 0;
                float v = Velocity.X, 
                      d = 0f;

                if (UseSmoothStopWithMovingToTarget && !Math.EqualsEstimate(DragForce * MaxVelocity.X * dt, 0f, 1f)) {
                    while (v != 0f) { // && d < MoveDistanceRemaining.X) {
                        v = Math.Approach(v, 0f, DragForce * MaxVelocity.X * dt);
                        d += v * dt;
                        i += 1;
                    }
                }

                if (Math.Abs(d) < Math.Abs(targetRemaining.X)) {
                    if (Math.EqualsEstimate(axis.X, 0f)) {
                        axis.X = moveTargetAxis.X;
                    } else {
                        axis.X = axis.X + moveTargetAxis.X;
                        combinedAxis = true;
                    }
                }

                // vertical

                i = 0;
                v = Velocity.Y;
                d = 0f;

                if (UseSmoothStopWithMovingToTarget && !Math.EqualsEstimate(DragForce * MaxVelocity.Y * dt, 0f, 1f)) {
                    while (v != 0f) { // && d < MoveDistanceRemaining.Y) {
                        v = Math.Approach(v, 0f, DragForce * MaxVelocity.Y * dt);
                        d += v * dt;
                        i += 1;
                    }
                }

                if (Math.Abs(d) < Math.Abs(targetRemaining.Y)) {
                    if (Math.EqualsEstimate(axis.Y, 0f)) {
                        axis.Y = moveTargetAxis.Y;
                    } else {
                        axis.Y = axis.Y + moveTargetAxis.Y;
                        combinedAxis = true;
                    }
                }

                //

                if (combinedAxis) {
                    axis = axis.Normalized();
                }

                Axis = axis;
            }


            // horizontal velocity

            float horizontalVelocity = Velocity.X;
            if (axis.X == 0f) { // stopping from movement, drag force applies
                horizontalVelocity = Math.Approach(horizontalVelocity, 0f, DragForce * MaxVelocity.X * dt);
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && Math.Sign(axis.X) != Math.Sign(horizontalVelocity)) { // snapping horizontal axis clears velocity
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) { // velocity increasing until MaxVelocity.X limit
                horizontalVelocity = Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else { // velocity increasing without a limit
                horizontalVelocity += System.Math.Sign(axis.X) * Acceleration.X * dt;
            }

            // vertical velocity

            float verticalVelocity = Velocity.Y;
            if (axis.Y == 0f) { // stopping from movement, drag force applies
                verticalVelocity = Math.Approach(verticalVelocity, 0f, DragForce * MaxVelocity.Y * dt);
            } else if (SnapVerticalAxis && verticalVelocity != 0f && System.Math.Sign(axis.Y) != System.Math.Sign(verticalVelocity)) { // snapping horizontal axis clears velocity
                verticalVelocity = 0f;
            } else if (MaxVelocity.Y > 0f) { // velocity increasing until MaxVelocity.Y limit
                verticalVelocity = Math.Approach(verticalVelocity, TargetVelocity.Y, Acceleration.Y * dt);
            } else { // velocity increasing without a limit
                verticalVelocity += System.Math.Sign(axis.Y) * Acceleration.Y * dt;
            }

            Velocity = new Vector2(horizontalVelocity, verticalVelocity);
            Vector2 velocityDisplacement = Velocity * dt;

            if (IsMovingToTargetPosition) {
                float bufferDistance = moveTargetAxis.Projection(new Vector2(Body.MoveBufferX, Body.MoveBufferY)),
                      distance = moveTargetAxis.Projection(velocityDisplacement),
                      remainingDistance = targetRemaining.Length();

                if (remainingDistance <= distance + bufferDistance) {
                    velocityDisplacement = targetRemaining;
                    Body.MoveBufferX = Body.MoveBufferY = 0;
                    MoveTargetPosition = null;
                }
            }

            return velocityDisplacement + Body.Force * dt;
        }

        public override void DebugRender() {
            base.DebugRender();
            Debug.DrawString(Camera.Current, new Vector2(16, Game.Instance.Height / 2f), ToStringDetailed());
        }

        public string ToStringDetailed() {
            return $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})";
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            OnMove(distance);
        }

        #endregion Protected Methods
    }
}
