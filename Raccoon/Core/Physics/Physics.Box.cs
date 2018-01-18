using System.Collections.Generic;
using Raccoon.Components;
using Raccoon.Util;

namespace Raccoon {
    public sealed partial class Physics {
        #region Box vs Box

        private bool CheckBoxBox(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            BoxShape boxA = A.Shape as BoxShape,
                     shapeB = B.Shape as BoxShape;

            // AABB x AABB
            if (System.Math.Abs(boxA.Rotation) < Math.Epsilon && System.Math.Abs(shapeB.Rotation) < Math.Epsilon) {
                Vector2 translation = BPos - APos;
                Vector2 overlap = boxA.HalwidthExtents + shapeB.HalwidthExtents - Math.Abs(translation);

                if (overlap.X < 0f || overlap.Y < 0f) {
                    manifold = null;
                    return false;
                }

                manifold = new Manifold(A, B) {
                    Contacts = new Contact[2]
                };

                Contact contact1 = new Contact {
                    Position = Math.Max(APos - boxA.HalwidthExtents, BPos - shapeB.HalwidthExtents)
                }, 
                        contact2 = new Contact {
                    Position = Math.Min(APos + boxA.HalwidthExtents, BPos + shapeB.HalwidthExtents)
                };

                if (overlap.X < overlap.Y) {
                    contact1.Normal = contact2.Normal = translation.X < 0 ? Vector2.Left : Vector2.Right;
                    contact1.PenetrationDepth = contact2.PenetrationDepth = overlap.X;
                } else {
                    contact1.Normal = contact2.Normal = translation.Y < 0 ? Vector2.Up : Vector2.Down;
                    contact1.PenetrationDepth = contact2.PenetrationDepth = overlap.Y;
                }

                manifold.Contacts[0] = contact1;
                manifold.Contacts[1] = contact2;
                return true;
            }

            // OBB x OBB
            Polygon polygonA = new Polygon(boxA.Shape), polygonB = new Polygon(shapeB.Shape);
            polygonA.Translate(APos);
            polygonB.Translate(BPos);

            if (TestSAT(polygonA, polygonB, out Contact? contact)) {
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

        #endregion Box vs Box

        #region Box vs Circle

        private bool CheckBoxCircle(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            BoxShape boxA = A.Shape as BoxShape;
            CircleShape circleB = B.Shape as CircleShape;

            Vector2 closestPoint = boxA.ClosestPoint(BPos);
            Vector2 diff = closestPoint - BPos;
            if (Vector2.Dot(diff, diff) > circleB.Radius * circleB.Radius) {
                manifold = null;
                return false;
            }

            if (TestSAT(boxA, circleB, boxA.Axes, out Contact? contact)) {
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

        #endregion Box vs Circle

        #region Box vs Polygon

        private bool CheckBoxPolygon(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            BoxShape boxA = A.Shape as BoxShape;
            PolygonShape polyB = B.Shape as PolygonShape;

            Polygon polygonA = new Polygon(boxA.Shape), polygonB = new Polygon(polyB.Shape);
            polygonA.Translate(APos);
            polygonB.Translate(BPos);

            if (TestSAT(polygonA, polygonB, out Contact? contact)) {
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

        #endregion Box vs Polygon

        #region Box vs Grid

        private bool CheckBoxGrid(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            BoxShape boxA = A.Shape as BoxShape;
            GridShape gridB = B.Shape as GridShape;

            Rectangle boxBoundingBox = new Rectangle(APos - boxA.BoundingBox / 2f, boxA.BoundingBox),
                      gridBoundingBox = new Rectangle(BPos, gridB.BoundingBox);

            // test grid bounds
            if (!gridBoundingBox.Intersects(boxBoundingBox)) {
                manifold = null;
                return false;
            }

            Polygon polygonA = new Polygon(boxA.Shape);
            polygonA.Translate(APos);

            Contact? contact = TestGrid(gridB, BPos, boxBoundingBox, 
                (Polygon tilePolygon) => {
                    TestSAT(polygonA, tilePolygon, out Contact? tileContact);
                    return tileContact;
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

        #endregion Box vs Grid
    }
}
