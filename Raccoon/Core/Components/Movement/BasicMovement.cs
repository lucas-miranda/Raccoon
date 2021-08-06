using Raccoon.Util;

namespace Raccoon.Components {
    public class BasicMovement : Movement {
        #region Private Members

        private Vector2 _currentAcceleration;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 acceleration) : base(acceleration) {
            DragForce = .1f;
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 maxVelocity, Vector2 acceleration) : base(maxVelocity, acceleration) {
            DragForce = .1f;
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="timeToAchieveMaxSpeed">Time (in miliseconds) to reach max velocity.</param>
        public BasicMovement(Vector2 maxVelocity, uint timeToAchieveMaxSpeed) : base(maxVelocity, timeToAchieveMaxSpeed) {
            DragForce = .1f;
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

                if (UseSmoothStopWithMovingToTarget && !Math.EqualsEstimate((DragForce / dt) * MaxVelocity.X * dt, 0f, 1f)) {
                    while (v != 0f) { // && d < MoveDistanceRemaining.X) {
                        v = Math.Approach(v, 0f, (DragForce / dt) * MaxVelocity.X * dt);
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

                if (UseSmoothStopWithMovingToTarget && !Math.EqualsEstimate((DragForce / dt) * MaxVelocity.Y * dt, 0f, 1f)) {
                    while (v != 0f) { // && d < MoveDistanceRemaining.Y) {
                        v = Math.Approach(v, 0f, (DragForce / dt) * MaxVelocity.Y * dt);
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

            Vector2 displacement = Vector2.Zero,
                    velocity = Velocity,
                    currentAcceleration = Vector2.Zero;

            //
            // horizontal velocity
            //

            if (axis.X == 0f) { // stopping from movement, drag force applies
                currentAcceleration.X += CalculateAcceleration(
                    velocity.X, 
                    0f, 
                    dt, 
                    (DragForce / dt) * MaxVelocity.X
                );
            } else if (SnapHorizontalAxis && velocity.X != 0f && Math.Sign(axis.X) != Math.Sign(velocity.X)) { // snapping horizontal axis clears velocity
                velocity.X = 0f;
            } else if (MaxVelocity.X > 0f) { // velocity increasing until MaxVelocity.X limit
                currentAcceleration.X += CalculateAcceleration(
                    velocity.X, 
                    TargetVelocity.X, 
                    dt, 
                    Acceleration.X
                );
            } else { // velocity increasing without a limit
                currentAcceleration.X += System.Math.Sign(axis.X) * Acceleration.X;
            }

            if (currentAcceleration.X != 0f 
             && velocity.X != 0f 
             && Math.Sign(currentAcceleration.X) != Math.Sign(velocity.X) 
             && Math.Abs(currentAcceleration.X * dt) >= Math.Abs(velocity.X)
            ) {
                velocity.X = 0f;
            } else {
                velocity.X += currentAcceleration.X * dt;
            }

            displacement.X += (Velocity.X + Body.Force.X) * dt + .5f * _currentAcceleration.X * dt * dt;

            //
            // vertical velocity
            // 

            if (axis.Y == 0f) { // stopping from movement, drag force applies
                currentAcceleration.Y += CalculateAcceleration(
                    velocity.Y, 
                    0f, 
                    dt, 
                    (DragForce / dt) * MaxVelocity.Y
                );
            } else if (SnapVerticalAxis && velocity.Y != 0f && System.Math.Sign(axis.Y) != System.Math.Sign(velocity.Y)) { // snapping horizontal axis clears velocity
                velocity.Y = 0f;
            } else if (MaxVelocity.Y > 0f) { // velocity increasing until MaxVelocity.Y limit
                currentAcceleration.Y += CalculateAcceleration(
                    velocity.Y, 
                    TargetVelocity.Y, 
                    dt, 
                    Acceleration.Y
                );
            } else { // velocity increasing without a limit
                currentAcceleration.Y += System.Math.Sign(axis.Y) * Acceleration.Y;
            }

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

            if (currentAcceleration.Y != 0f 
             && velocity.Y != 0f 
             && Math.Sign(currentAcceleration.Y) != Math.Sign(velocity.Y) 
             && Math.Abs(currentAcceleration.Y * dt) >= Math.Abs(velocity.Y)
            ) {
                velocity.Y = 0f;
            } else {
                velocity.Y += currentAcceleration.Y * dt;
            }

            displacement.Y += velocityDisplacement.Y + Body.Force.Y * dt + .5f * _currentAcceleration.Y * dt * dt;

            //

            Velocity = velocity;
            _currentAcceleration = currentAcceleration;

            return displacement;
        }

        public override void DebugRender() {
            base.DebugRender();
            Debug.DrawString(new Vector2(16, Game.Instance.Height / 2f), ToStringDetailed());
        }

        public string ToStringDetailed() {
            return $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})";
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            OnMove?.Invoke(distance);
        }

        #endregion Protected Methods
    }
}
