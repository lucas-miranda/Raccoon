using Raccoon.Components;
using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class CircleShape : IShape {
        public CircleShape(int radius) {
            Radius = radius;
            BoundingBox = new Size(Radius + Radius);
            Area = (int) (Math.PI * Radius * Radius);
        }

        public int Radius { get; }
        public int Area { get; }
        public Vector2 Origin { get; set; }
        public Size BoundingBox { get; }

        public void DebugRender(Vector2 position, Color color) {
            // boundingBox
            Debug.DrawRectangle(new Rectangle(position - Origin - BoundingBox / 2f, Debug.Transform(BoundingBox)), Color.Indigo);

            float gameWorldRadius = Debug.Transform(Radius);
            Debug.DrawCircle(position - Origin, gameWorldRadius, (int) (gameWorldRadius * gameWorldRadius), color);
        }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }

        public float ComputeMass(float density) {
            //float mass = (float) (Math.PI * Radius * Radius * density);
            //float inertia = mass * Radius * Radius;
            return density;
        }

        public Range Projection(Vector2 position, Vector2 axis) {
            float p = Vector2.Dot(position, axis);
            return new Range(p - Radius, p + Radius);
        }
    }
}
