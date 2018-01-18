namespace Raccoon.Components {
    public class BasicMovement : Movement {
        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 acceleration) : base(acceleration) {
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="acceleration">Speed increase.</param>
        public BasicMovement(Vector2 maxVelocity, Vector2 acceleration) : base(maxVelocity, acceleration) {
            SnapAxes = true;
        }

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxVelocity">Max horizontal and vertical velocity.</param>
        /// <param name="timeToAchieveMaxSpeed">Time (in miliseconds) to reach max velocity.</param>
        public BasicMovement(Vector2 maxVelocity, int timeToAchieveMaxSpeed) : base(maxVelocity, timeToAchieveMaxSpeed) {
            SnapAxes = true;
        }

        public override Vector2 HandleVelocity(Vector2 velocity, float dt) {
            float horizontalVelocity = velocity.X;
            if (Axis.X == 0f) {
                horizontalVelocity = velocity.X * DragForce;
            } else if (SnapHorizontalAxis && System.Math.Sign(Axis.X) != System.Math.Sign(velocity.X)) {
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) {
                horizontalVelocity = Util.Math.Clamp(velocity.X, -MaxVelocity.X, MaxVelocity.X);
            }

            float verticalVelocity = velocity.Y;
            if (Axis.Y == 0f) {
                verticalVelocity = velocity.Y * DragForce;
            } else if (SnapVerticalAxis && System.Math.Sign(Axis.Y) != System.Math.Sign(velocity.Y)) {
                verticalVelocity = 0f;
            } else if (MaxVelocity.Y > 0f) {
                verticalVelocity = Util.Math.Clamp(velocity.Y, -MaxVelocity.Y, MaxVelocity.Y);
            }

            return new Vector2(horizontalVelocity, verticalVelocity);
        }

        public override void Move(Vector2 axis) {
            base.Move(axis);
            if (NextAxis == Vector2.Zero) {
                return;
            }

            Body.ApplyImpulse(NextAxis * Acceleration);
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})";
            Debug.DrawString(Camera.Current, new Vector2(16, Game.Instance.ScreenHeight / 2f), info);
        }
    }
}
