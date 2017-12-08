namespace Raccoon.Physics {
    public class CircleShape : IShape {
        public float Radius { get; set; }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }

        public Rectangle BoundingVolume() {
            throw new System.NotImplementedException();
        }
    }
}
