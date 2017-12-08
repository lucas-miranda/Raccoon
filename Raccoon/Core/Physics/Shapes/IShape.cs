namespace Raccoon.Physics {
    public interface IShape {
        bool ContainsPoint(Vector2 point);
        bool Intersects(Line line);
        Rectangle BoundingVolume();
    }
}
