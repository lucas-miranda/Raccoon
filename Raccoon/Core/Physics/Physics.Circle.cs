using Raccoon.Components;

namespace Raccoon {
    public sealed partial class Physics {
        #region Circle vs Circle

        private bool CheckCircleCircle(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            CircleShape circleA = A as CircleShape,
                        circleB = B as CircleShape;

            Vector2 translation = BPos - APos;
            float radius = circleA.Radius + circleB.Radius;
            if (translation.LengthSquared() > radius * radius) {
                contacts = null;
                return false;
            }

            float dist = translation.Length();
            if (dist == 0) {
                contacts = new Contact[] { new Contact(APos, Vector2.Right, circleA.Radius) };
            } else {
                Vector2 normal = translation / dist;
                contacts = new Contact[] { new Contact(APos + normal * circleA.Radius, normal, radius - dist) };
            }

            return true;
        }

        #endregion Circle vs Circle

        #region Circle vs Box

        private bool CheckCircleBox(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return CheckBoxCircle(B, BPos, A, APos, out contacts);
        }

        #endregion Circle vs Box

        #region Circle vs Polygon

        private bool CheckCirclePolygon(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            CircleShape circleA = A as CircleShape;
            PolygonShape polyB = B as PolygonShape;

            Polygon polygonB = new Polygon(polyB.Shape);
            polygonB.Translate(BPos);

            Vector2 closestPoint = polygonB.ClosestPoint(APos);
            Vector2 d = closestPoint - APos;
            if (Vector2.Dot(d, d) > circleA.Radius * circleA.Radius) {
                contacts = null;
                return false;
            }

            if (TestSAT(circleA, polyB, polygonB.Normals, out Contact? contact)) {
                contacts = new Contact[] {
                    new Contact(closestPoint, contact.Value.Normal, contact.Value.PenetrationDepth)
                };

                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Circle vs Polygon
        
        #region Circle vs Grid

        private bool CheckCircleGrid(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            CircleShape circleA = A as CircleShape;
            GridShape gridB = B as GridShape;

            // test grid bounds
            Vector2 closestPoint = new Rectangle(BPos, gridB.Size).ClosestPoint(APos);
            Vector2 diff = closestPoint - APos;
            if (Vector2.Dot(diff, diff) > circleA.Radius * circleA.Radius) {
                contacts = null;
                return false;
            }

            Contact? contact = TestGrid(gridB, BPos, new Rectangle(APos - circleA.Radius, APos + circleA.Radius), 
                (Polygon tilePolygon) => {
                    TestSAT(circleA, tilePolygon, out Contact? tileContact);
                    return new Contact(closestPoint, tileContact.Value.Normal, tileContact.Value.PenetrationDepth);
                }
            );

            if (contact != null) {
                contacts = new Contact[] {
                    contact.Value
                };
                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Circle vs Grid
    }
}
