using Raccoon.Util;

namespace Raccoon.Components {
    public class BasicMovement : Movement {
        #region Constructors

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 acceleration) : base(acceleration) {
            DragForce = .9f;
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 maxVelocity, Vector2 acceleration) : base(maxVelocity, acceleration) {
            DragForce = .9f;
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="timeToAchieveMaxSpeed">Time (in miliseconds) to reach max velocity.</param>
        public BasicMovement(Vector2 maxVelocity, int timeToAchieveMaxSpeed) : base(maxVelocity, timeToAchieveMaxSpeed) {
            DragForce = .9f;
            SnapAxes = true;
        }

        #endregion Constructors

        #region Public Methods

        public override Vector2 Integrate(float dt) {
            float horizontalVelocity = Velocity.X;
            if (Axis.X == 0f) { // stopping from movement, drag force applies
                horizontalVelocity = Math.EqualsEstimate(horizontalVelocity, 0f) ? 0f : horizontalVelocity * DragForce;
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && Math.Sign(Axis.X) != Math.Sign(horizontalVelocity)) { // snapping horizontal axis clears velocity
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) { // velocity increasing until MaxVelocity.X limit
                horizontalVelocity = Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else { // velocity increasing without a limit
                horizontalVelocity += System.Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            float verticalVelocity = Velocity.Y;
            if (Axis.Y == 0f) { // stopping from movement, drag force applies
                verticalVelocity = Math.EqualsEstimate(verticalVelocity, 0f) ? 0f : verticalVelocity * DragForce;
            } else if (SnapVerticalAxis && verticalVelocity != 0f && System.Math.Sign(Axis.Y) != System.Math.Sign(verticalVelocity)) { // snapping horizontal axis clears velocity
                verticalVelocity = 0f;
            } else if (MaxVelocity.Y > 0f) { // velocity increasing until MaxVelocity.Y limit
                verticalVelocity = Math.Approach(verticalVelocity, TargetVelocity.Y, Acceleration.Y * dt);
            } else { // velocity increasing without a limit
                verticalVelocity += System.Math.Sign(Axis.Y) * Acceleration.Y * dt;
            }

            Velocity = new Vector2(horizontalVelocity, verticalVelocity);

            return (Velocity + Body.Force) * dt;
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})";
            Debug.DrawString(Camera.Current, new Vector2(16, Game.Instance.Height / 2f), info);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnMoving(Vector2 distance) {
            OnMove(distance);
        }

        #endregion Protected Methods
    }
}
