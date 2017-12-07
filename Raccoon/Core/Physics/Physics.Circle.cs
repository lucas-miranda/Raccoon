using Raccoon.Components;

namespace Raccoon {
    public partial class Physics {
        #region Circle vs Circle

        private bool CheckCircleCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            CircleCollider circleA = colliderA as CircleCollider, circleB = colliderB as CircleCollider;
            Vector2 centerDiff = (colliderBPos + circleB.Radius) - (colliderAPos + circleA.Radius);
            return centerDiff.X * centerDiff.X + centerDiff.Y * centerDiff.Y <= (circleA.Radius + circleB.Radius) * (circleA.Radius + circleB.Radius);
        }

        #endregion Circle vs Circle

        #region Circle vs Box

        private bool CheckCircleBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return CheckBoxCircle(colliderB, colliderBPos, colliderA, colliderAPos);
        }

        #endregion Circle vs Box

        #region Circle vs Grid

        private bool CheckCircleGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Circle vs Grid

        #region Circle vs Line

        private bool CheckCircleLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            CircleCollider circleColl = colliderA as CircleCollider;
            LineCollider lineColl = colliderB as LineCollider;

            if (Util.Math.DistanceSquared(new Line(colliderBPos + lineColl.From, colliderBPos + lineColl.To), circleColl.Position) < circleColl.Radius * circleColl.Radius) {
                return true;
            }

            return false;
        }

        #endregion Circle vs Line

        #region Circle vs Polygon

        private bool CheckCirclePolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            CircleCollider circleColl = colliderA as CircleCollider;
            PolygonCollider polygonColl = colliderB as PolygonCollider;

            float radiusSquared = circleColl.Radius * circleColl.Radius;
            Polygon polygon = polygonColl.Polygon.Clone();
            polygon.Translate(colliderBPos);
            for (int i = 0; i < polygon.VertexCount; i++) {
                if (Util.Math.DistanceSquared(new Line(polygon[i], polygon[(i + 1) % polygon.VertexCount]), circleColl.Position) < radiusSquared) {
                    return true;
                }
            }

            return false;
        }

        #endregion Circle vs Polygon

        #region Circle vs RichGrid

        private bool CheckCircleRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            return false;
        }

        #endregion Circle vs RichGrid
    }
}
