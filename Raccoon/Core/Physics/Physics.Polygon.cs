using System.Collections.Generic;
using Raccoon.Components;

namespace Raccoon {
    public partial class Physics {
        #region Polygon vs Polygon

        private bool CheckPolygonPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            PolygonCollider polygonAColl = colliderA as PolygonCollider, polygonBColl = colliderB as PolygonCollider;

            // polygon A collider polygon
            Polygon polygonA = polygonAColl.Polygon.Clone();
            polygonA.Translate(colliderAPos);

            // polygon B collider polygon
            Polygon polygonB = polygonBColl.Polygon.Clone();
            polygonB.Translate(colliderBPos);

            List<Vector2> axes = new List<Vector2>();

            // polygon A axes
            for (int i = 0; i < polygonA.VertexCount; i++) {
                axes.Add((polygonA[i] - polygonA[(i + 1) % polygonA.VertexCount]).Perpendicular());
            }

            // polygon B axes
            for (int i = 0; i < polygonB.VertexCount; i++) {
                axes.Add((polygonB[i] - polygonB[(i + 1) % polygonB.VertexCount]).Perpendicular());
            }

            return CheckPolygonsIntersection(polygonA, polygonB, axes);
        }

        #endregion Polygon vs Polygon

        #region Polygon vs Box

        private bool CheckPolygonBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxPolygon(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Polygon vs Box

        private bool CheckPolygonGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #region Polygon vs Circle

        private bool CheckPolygonCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCirclePolygon(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Polygon vs Circle

        #region Polygon vs Line

        private bool CheckPolygonLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckLinePolygon(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Polygon vs Line

        #region Polygon vs RichGrid

        private bool CheckPolygonRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Polygon vs RichGrid
    }
}
