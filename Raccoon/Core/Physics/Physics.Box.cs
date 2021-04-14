using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon {
    public sealed partial class Physics {
        #region Box vs Box

        private bool CheckBoxBox(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            BoxShape boxA = A as BoxShape,
                     shapeB = B as BoxShape;

            // AABB x AABB
            if (System.Math.Abs(boxA.Rotation) < Math.Epsilon && System.Math.Abs(shapeB.Rotation) < Math.Epsilon) {
                Vector2 translation = BPos - APos;
                Vector2 overlap = (boxA.HalwidthExtents + shapeB.HalwidthExtents) - Math.Abs(translation);

                if (overlap.X < 0f || overlap.Y < 0f) {
                    contacts = null;
                    return false;
                }

                contacts = new Contact[1];

                if (overlap.X < overlap.Y) {
                    float sign = Math.Sign(translation.X);
                    contacts[0] = new Contact {
                        Position = new Vector2(APos.X + boxA.HalwidthExtents.X * sign, BPos.Y),
                        Normal = new Vector2(sign, 0f),
                        PenetrationDepth = Math.Abs(overlap.X)
                    };
                } else {
                    float sign = Math.Sign(translation.Y);
                    contacts[0] = new Contact {
                        Position = new Vector2(BPos.X, APos.Y + boxA.HalwidthExtents.Y * sign),
                        Normal = new Vector2(0f, sign),
                        PenetrationDepth = Math.Abs(overlap.Y)
                    };
                }

                return true;
            }

            // OBB x OBB
            Polygon polygonA = new Polygon(boxA.Shape), polygonB = new Polygon(shapeB.Shape);
            polygonA.Translate(APos);
            polygonB.Translate(BPos);

            if (SAT.Test(polygonA, polygonB, out Contact? contact)) {
                contacts = new Contact[] {
                    contact.Value
                };

                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Box vs Box

        #region Box vs Circle

        private bool CheckBoxCircle(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            BoxShape boxA = A as BoxShape;
            CircleShape circleB = B as CircleShape;

            Vector2 closestPoint = boxA.ClosestPoint(APos, BPos);
            Vector2 diff = closestPoint - BPos;
            if (Vector2.Dot(diff, diff) > circleB.Radius * circleB.Radius) {
                contacts = null;
                return false;
            }

            if (SAT.Test(boxA, APos, circleB, BPos, boxA.Axes, out Contact? contact)) {
                if (contact == null) {
                    contacts = new Contact[0];
                } else {
                    contacts = new Contact[] {
                        new Contact(closestPoint, contact.Value.Normal, contact.Value.PenetrationDepth)
                    };
                }

                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Box vs Circle

        #region Box vs Polygon

        private bool CheckBoxPolygon(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            BoxShape boxA = A as BoxShape;
            PolygonShape polyB = B as PolygonShape;

            Polygon polygonA = new Polygon(boxA.Shape), 
                    polygonB = new Polygon(polyB.Shape);

            polygonA.Translate(APos);
            polygonB.Translate(BPos);

            if (SAT.Test(polygonA, polygonB, out Contact? contact)) {
                contacts = new Contact[] {
                    contact.Value
                };

                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Box vs Polygon

        #region Box vs Grid

        private bool CheckBoxGrid(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            BoxShape boxA = A as BoxShape;
            GridShape gridB = B as GridShape;

            Rectangle boxBoundingBox = boxA.BoundingBox + APos,
                      gridBoundingBox = gridB.BoundingBox + BPos;

            // test grid bounds
            if (!gridBoundingBox.Intersects(boxBoundingBox)) {
                contacts = null;
                return false;
            }

            Polygon polygonA = new Polygon(boxA.Shape);
            polygonA.Translate(APos);

            List<Contact> gridContacts = TestGrid(gridB, BPos, boxBoundingBox,
                (Polygon tilePolygon) => {
                    SAT.Test(polygonA, tilePolygon, out Contact? tileContact);
                    return tileContact;
                }
            );

            if (gridContacts.Count > 0) {
                contacts = gridContacts.ToArray();
                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Box vs Grid
    }
}
