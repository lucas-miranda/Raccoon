using System.Collections.Generic;
using Raccoon.Util;

namespace Raccoon {
    public partial class Physics {
        private bool CheckPolygonsIntersection(Polygon polygonA, Polygon polygonB, IEnumerable<Vector2> axes) {
            foreach (Vector2 axis in axes) {
                Range projectionA = polygonA.Projection(axis), projectionB = polygonB.Projection(axis);
                if (projectionA.Min >= projectionB.Max || projectionB.Min >= projectionA.Max) {
                    return false;
                }
            }

            return true;
        }

        private bool CheckPolygonsIntersection(Polygon polygonA, Polygon polygonB) {
            List<Vector2> axes = new List<Vector2>();

            // polygon A axes
            Vector2 previousVertex = polygonA[0];
            for (int i = 1; i < polygonA.VertexCount; i++) {
                axes.Add((polygonA[i] - previousVertex).Perpendicular());
                previousVertex = polygonA[i];
            }

            // polygon B axes
            previousVertex = polygonB[0];
            for (int i = 1; i < polygonB.VertexCount; i++) {
                axes.Add((polygonB[i] - previousVertex).Perpendicular());
                previousVertex = polygonB[i];
            }

            return CheckPolygonsIntersection(polygonA, polygonB, axes);
        }
    }
}
