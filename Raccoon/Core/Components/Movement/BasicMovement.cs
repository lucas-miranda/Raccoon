namespace Raccoon.Components {
    public class BasicMovement : Movement {
        public event System.Action OnMove;

        /// <summary>
        /// A component that handles simple top-down movement.
        /// </summary>
        /// <param name="maxSpeed">Max horizontal and vertical speed. (in pixels/sec)</param>
        /// <param name="acceleration">Speed increase. (in pixels/sec)</param>
        /// <param name="collider">Collider used to detect end of movement.</param>
        public BasicMovement(Vector2 maxSpeed, Vector2 acceleration, Collider collider) : base(maxSpeed, acceleration, collider) {
        }

        public BasicMovement(Vector2 maxSpeed, float timeToMaxSpeed, Collider collider) : base(maxSpeed, maxSpeed / timeToMaxSpeed, collider) {
        }

        public override void OnMoveUpdate(float dt) {
            /*
                verlet integration
                ----------------------
                speed = pos - lastPos
                nextPos = pos + speed + acc * timestep
                lastPos = pos
                pos = nextPos
            */

            Vector2 speed = CurrentSpeed;
            float speedX = speed.X, speedY = speed.Y;

            // apply drag force, if it's necessary
            speedX = Axis.X == 0f ? speedX * DragForce : speedX;
            speedY = Axis.Y == 0f ? speedY * DragForce : speedY;

            // axes snap 
            // ignore speed if it's too low
            if (System.Math.Abs(speedX) < 0.001f || (HorizontalAxisSnap && System.Math.Sign(speed.X) != System.Math.Sign(Axis.X))) {
                speedX = 0;
            }

            if (System.Math.Abs(speedY) < 0.001f || (VerticalAxisSnap && System.Math.Sign(speed.Y) != System.Math.Sign(Axis.Y))) {
                speedY = 0;
            }

            speed = new Vector2(speedX, speedY);

            Vector2 nextPos = Entity.Position + speed + AccumulatedForce * dt * dt;
            LastPosition = Entity.Position;
            Entity.Position = nextPos;
            AccumulatedForce = Vector2.Zero;

            if (speed.LengthSquared() != 0) {
                OnMove?.Invoke();
            }
        }

        public override void DebugRender() {
            base.DebugRender();
            string message = $"Position: {Entity.Position}\nLastPosition: {LastPosition}]\nAxis: {Axis} [Last: {LastAxis}]\nCurrentSpeed: {CurrentSpeed} [Max: {MaxSpeed}]\nAcceleration: {Acceleration}\nAccumulatedForce: {AccumulatedForce}\nDragForce: {DragForce}\nH-Snap? {HorizontalAxisSnap}, V-Snap? {VerticalAxisSnap}\nEnabled? {Enabled}, CanMove? {CanMove}, Sleeping? {Sleeping}\nBuffer: [{MoveHorizontalBuffer}, {MoveVerticalBuffer}]";
            Vector2 pos = new Vector2(Game.Instance.WindowWidth - 350, Game.Instance.WindowHeight / 2);

            if (Camera.Current != null) {
                Debug.DrawString(Camera.Current, pos, message);
                return;
            }

            Debug.DrawString(pos, message);
        }

        public override void Move(Vector2 axis) {
            base.Move(axis);
            if (NextAxis == Vector2.Zero) {
                return;
            }

            ApplyForce(NextAxis * Acceleration);
        }
    }
}
