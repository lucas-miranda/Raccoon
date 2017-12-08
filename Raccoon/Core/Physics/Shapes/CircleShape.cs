namespace Raccoon {
    public class CircleShape : IShape {
        public CircleShape(float radius) {
            Radius = radius;
            BoundingBox = new Rectangle(new Size(Radius + Radius));
        }

        public float Radius { get; }
        public Rectangle BoundingBox { get; }

        public void DebugRender(Vector2 position) {
            float gameWorldRadius = Debug.Transform(Radius);
            Debug.DrawCircle(position, gameWorldRadius, (int) (gameWorldRadius * gameWorldRadius), Graphics.Color.White);
        }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }
    }
}
