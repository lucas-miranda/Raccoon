using System.Collections.Generic;
using Raccoon.Components;

namespace Raccoon.Physics {
    public partial class Physics {
        #region Line vs Line

        private bool CheckLineLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            LineCollider lineA = colliderA as LineCollider, lineB = colliderB as LineCollider;
            Vector2 lineAFrom = lineA.From + colliderAPos, lineATo = lineA.To + colliderAPos,
                    lineBFrom = lineB.From + colliderBPos, lineBTo = lineB.To + colliderBPos;

            Vector2 lineALength = lineA.To - lineA.From, lineBLength = lineB.To - lineB.From;

            float lengthsCross = Vector2.Cross(lineALength, lineBLength);
            float fromDiffCrossLengthA = Vector2.Cross(lineBFrom - lineAFrom, lineALength);

            if (lengthsCross == 0) {
                if (fromDiffCrossLengthA == 0) { // collinear
                    float t0 = Vector2.Dot(lineBFrom - lineAFrom, lineALength / Vector2.Dot(lineALength, lineALength));
                    float t1 = t0 + Vector2.Dot(lineBLength, lineALength / Vector2.Dot(lineALength, lineALength));
                    return Vector2.Dot(lineBLength, lineALength) < 0 ? !(t1 > 1 || t0 < 0) : !(t0 > 1 || t1 < 0);
                }

                return false;
            }

            float t = Vector2.Cross(lineBFrom - lineAFrom, lineBLength) / lengthsCross, u = fromDiffCrossLengthA / lengthsCross;
            return !((t < 0 || t > 1) || (u < 0 || u > 1));
        }

        #endregion Line vs Line

        #region Line vs Box

        private bool CheckLineBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxLine(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Line vs Box

        private bool CheckLineGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #region Line vs Circle

        private bool CheckLineCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckCircleLine(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Line vs Circle

        #region Line vs Polygon

        private bool CheckLinePolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            LineCollider lineColl = colliderA as LineCollider;
            PolygonCollider polygonColl = colliderB as PolygonCollider;

            // line collider polygon
            Polygon linePolygon = new Polygon(new Vector2(lineColl.From), new Vector2(lineColl.To));
            linePolygon.Translate(colliderAPos);

            // polygon collider polygon
            Polygon polygon = polygonColl.Polygon.Clone();
            polygon.Translate(colliderBPos);

            List<Vector2> axes = new List<Vector2> {
                // line axis
                (linePolygon[0] - linePolygon[1]).Perpendicular()
            };

            // polygon axes
            for (int i = 0; i < polygon.VertexCount; i++) {
                axes.Add((polygon[i] - polygon[(i + 1) % polygon.VertexCount]).Perpendicular());
            }

            return CheckPolygonsIntersection(linePolygon, polygon, axes);
        }

        #endregion Line vs Polygon

        #region Line vs RichGrid

        private bool CheckLineRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Line vs RichGrid
    }
}
