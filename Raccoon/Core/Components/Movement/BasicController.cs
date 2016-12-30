using Raccoon.Util;

namespace Raccoon.Components {
    public class BasicMovement : Movement {
        public event System.Action OnMove;

        public BasicMovement(Vector2 maxSpeed, Vector2 acceleration, Collider collider = null) : base(maxSpeed, acceleration, collider) { }

        public override void OnCollide(Vector2 moveDirection) { }

        public override void OnMoveUpdate(float dt) {
            int x = (int) Entity.X, y = (int) Entity.Y;
            float speedX = Speed.X, speedY = Speed.Y;

            // determine TargetSpeed
            Vector2 oldTargetSpeed = TargetSpeed;
            TargetSpeed = Axis * MaxSpeed;
            if (HorizontalAxisSnap && System.Math.Sign(oldTargetSpeed.X) != System.Math.Sign(TargetSpeed.X)) {
                speedX = 0;
                MoveHorizontalBuffer = 0;
            } else if (VerticalAxisSnap && System.Math.Sign(oldTargetSpeed.Y) != System.Math.Sign(TargetSpeed.Y)) {
                speedY = 0;
                MoveVerticalBuffer = 0;
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
            speedY = Math.Approach(speedY, TargetSpeed.Y, (Axis.Y != 0 ? Acceleration.Y : Acceleration.Y * DragForce) * dt);

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
                            MoveVerticalBuffer = 0;
                            break;
                        } else {
                            y += vDir;
                            MoveVerticalBuffer = Math.Approach(MoveVerticalBuffer, 0, 1);
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
    }
}
