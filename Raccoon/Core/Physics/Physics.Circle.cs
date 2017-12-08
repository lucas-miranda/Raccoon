using Raccoon.Components;

namespace Raccoon.Physics {
    public partial class Physics {
        #region Circle vs Circle

        private bool CheckCircleCircle(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            CircleShape shapeA = A.Shape as CircleShape,
                        shapeB = B.Shape as CircleShape;

            Vector2 translation = BPos - APos;
            float radius = shapeA.Radius + shapeB.Radius;
            if (translation.LengthSquared() > radius * radius) {
                manifold = null;
                return false;
            }

            float dist = translation.Length();
            manifold = new Manifold(A, B) {
                Contacts = new Contact[1]
            };

            if (dist == 0) {
                manifold.Contacts[0] = new Contact(APos, Vector2.Right, shapeA.Radius);
            } else {
                Vector2 normal = translation / dist;
                manifold.Contacts[0] = new Contact(normal * shapeA.Radius + APos, normal, radius - dist);
            }

            return true;
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
