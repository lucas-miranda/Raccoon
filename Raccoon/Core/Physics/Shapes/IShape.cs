namespace Raccoon {
    public interface IShape {
        Rectangle BoundingBox { get; }

        void DebugRender(Vector2 position);
        bool ContainsPoint(Vector2 point);
        bool Intersects(Line line);
    }
}
