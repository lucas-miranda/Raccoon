using System.Collections.Generic;
using Raccoon.Components;

namespace Raccoon {
    public sealed partial class Physics {
        #region Polygon vs Polygon

        private bool CheckPolygonPolygon(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            PolygonShape shapeA = A.Shape as PolygonShape,
                         shapeB = B.Shape as PolygonShape;

            Polygon polygonA = new Polygon(shapeA.Shape), polygonB = new Polygon(shapeB.Shape);
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

        #endregion Polygon vs Polygon

        #region Polygon vs Box

        private bool CheckPolygonBox(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return CheckBoxPolygon(B, BPos, A, APos, out manifold);
        }

        #endregion Polygon vs Box

        #region Polygon vs Circle

        private bool CheckPolygonCircle(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return CheckCirclePolygon(B, BPos, A, APos, out manifold);
        }

        #endregion Polygon vs Circle

        #region Polygon vs Grid

        private bool CheckPolygonGrid(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            PolygonShape polygonShapeA = A.Shape as PolygonShape;
            GridShape gridB = B.Shape as GridShape;

            Rectangle polygonBoundingBox = new Rectangle(APos - polygonShapeA.BoundingBox / 2f, polygonShapeA.BoundingBox),
                      gridBoundingBox = new Rectangle(BPos, gridB.BoundingBox);

            // test grid bounds
            if (!gridBoundingBox.Intersects(polygonBoundingBox)) {
                manifold = null;
                return false;
            }

            Polygon polygonA = new Polygon(polygonShapeA.Shape);
            polygonA.Translate(APos);

            Contact? contact = TestGrid(gridB, BPos, polygonBoundingBox, 
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

        #endregion Polygon vs Grid
    }
}
