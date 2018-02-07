using Raccoon.Components;
using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public interface IShape {
        Body Body { get; set; }
        int Area { get; }
        Size BoundingBox { get; }
        Vector2 Origin { get; set; }

        void DebugRender(Vector2 position, Color color);
        bool ContainsPoint(Vector2 point);
        bool Intersects(Line line);
        float ComputeMass(float density);
        Range Projection(Vector2 axis);
    }
}
