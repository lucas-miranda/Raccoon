using Raccoon.Util;

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
                horizontalVelocity = System.Math.Abs(horizontalVelocity) < Math.Epsilon ? 0f : horizontalVelocity * DragForce;
            } else if (SnapHorizontalAxis && horizontalVelocity != 0f && System.Math.Sign(Axis.X) != System.Math.Sign(horizontalVelocity)) {
                horizontalVelocity = 0f;
            } else if (MaxVelocity.X > 0f) {
                horizontalVelocity = Math.Approach(horizontalVelocity, TargetVelocity.X, Acceleration.X * dt);
            } else {
                horizontalVelocity += System.Math.Sign(Axis.X) * Acceleration.X * dt;
            }

            float verticalVelocity = velocity.Y;
            if (Axis.Y == 0f) {
                verticalVelocity = System.Math.Abs(verticalVelocity) < Math.Epsilon ? 0f : verticalVelocity * DragForce;
            } else if (SnapVerticalAxis && verticalVelocity != 0f && System.Math.Sign(Axis.Y) != System.Math.Sign(verticalVelocity)) {
                verticalVelocity = 0f;
            } else if (MaxVelocity.Y > 0f) {
                verticalVelocity = Math.Approach(verticalVelocity, TargetVelocity.Y, Acceleration.Y * dt);
            } else {
                verticalVelocity += System.Math.Sign(Axis.Y) * Acceleration.Y * dt;
            }

            return new Vector2(horizontalVelocity, verticalVelocity);
        }

        public override void OnMoving(Vector2 distance) {
            OnMove();
        }

        public override void DebugRender() {
            base.DebugRender();
            string info = $"Axis: {Axis} (Last: {LastAxis})\nVelocity: {Velocity}\nMaxVelocity: {MaxVelocity}\nTargetVelocity: {TargetVelocity}\nAcceleration: {Acceleration}\nEnabled? {Enabled}; CanMove? {CanMove};\nAxes Snap: (H: {SnapHorizontalAxis}, V: {SnapVerticalAxis})";
            Debug.DrawString(Camera.Current, new Vector2(16, Game.Instance.ScreenHeight / 2f), info);
        }
    }
}
