﻿using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public interface IShape {
        int Area { get; }
        Rectangle BoundingBox { get; }
        Vector2 Origin { get; set; }

        void DebugRender(Vector2 position, Color color);
        bool ContainsPoint(Vector2 point);
        bool Intersects(Line line);
        float ComputeMass(float density);
        Range Projection(Vector2 shapePosition, Vector2 axis);
        Vector2[] CalculateAxes();
        (Vector2 MaxProjectionVertex, Line Edge) FindBestClippingEdge(Vector2 shapePosition, Vector2 normal);
    }
}
