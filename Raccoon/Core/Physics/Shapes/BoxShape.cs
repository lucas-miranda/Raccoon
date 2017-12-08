namespace Raccoon {
    public class BoxShape : IShape {
        public Rectangle BoundingBox => throw new System.NotImplementedException();

        public void DebugRender(Vector2 position) {
            throw new System.NotImplementedException();
        }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }
    }
}
