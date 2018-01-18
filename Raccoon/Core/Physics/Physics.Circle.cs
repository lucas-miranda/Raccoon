using System.Collections.Generic;
using Raccoon.Components;

namespace Raccoon {
    public sealed partial class Physics {
        #region Circle vs Circle

        private bool CheckCircleCircle(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            CircleShape circleA = A.Shape as CircleShape,
                        circleB = B.Shape as CircleShape;

            Vector2 translation = BPos - APos;
            float radius = circleA.Radius + circleB.Radius;
            if (translation.LengthSquared() > radius * radius) {
                manifold = null;
                return false;
            }

            float dist = translation.Length();
            manifold = new Manifold(A, B) {
                Contacts = new Contact[1]
            };

            if (dist == 0) {
                manifold.Contacts[0] = new Contact(APos, Vector2.Right, circleA.Radius);
            } else {
                Vector2 normal = translation / dist;
                manifold.Contacts[0] = new Contact(APos + normal * circleA.Radius, normal, radius - dist);
            }

            return true;
        }

        #endregion Circle vs Circle

        #region Circle vs Box

        private bool CheckCircleBox(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return CheckBoxCircle(B, BPos, A, APos, out manifold);
        }

        #endregion Circle vs Box

        #region Circle vs Polygon

        private bool CheckCirclePolygon(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            CircleShape circleA = A.Shape as CircleShape;
            PolygonShape polyB = B.Shape as PolygonShape;

            Polygon polygonB = new Polygon(polyB.Shape);
            polygonB.Translate(BPos);

            Vector2 closestPoint = polygonB.ClosestPoint(APos);
            Vector2 d = closestPoint - APos;
            if (Vector2.Dot(d, d) > circleA.Radius * circleA.Radius) {
                manifold = null;
                return false;
            }

            if (TestSAT(circleA, polyB, polygonB.Normals, out Contact? contact)) {
                manifold = new Manifold(A, B) {
                    Contacts = new Contact[] {
                        new Contact(closestPoint, contact.Value.Normal, contact.Value.PenetrationDepth)
                    }
                };

                return true;
            }

            manifold = null;
            return false;
        }

        #endregion Circle vs Polygon
        
        #region Circle vs Grid

        private bool CheckCircleGrid(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            CircleShape circleA = A.Shape as CircleShape;
            GridShape gridB = B.Shape as GridShape;

            // test grid bounds
            Vector2 closestPoint = new Rectangle(BPos, gridB.Size).ClosestPoint(APos);
            Vector2 diff = closestPoint - APos;
            if (Vector2.Dot(diff, diff) > circleA.Radius * circleA.Radius) {
                manifold = null;
                return false;
            }

            Contact? contact = TestGrid(gridB, BPos, new Rectangle(APos - circleA.Radius, APos + circleA.Radius), 
                (Polygon tilePolygon) => {
                    TestSAT(circleA, tilePolygon, out Contact? tileContact);
                    return new Contact(closestPoint, tileContact.Value.Normal, tileContact.Value.PenetrationDepth);
                }
            );

            if (contact != null) {
                manifold = new Manifold(A, B) {
                    Contacts = new Contact[] {
                        contact.Value
                    }
                };
                return true;
            }

            manifold = null;
            return false;
        }

        #endregion Circle vs Grid
    }
}
