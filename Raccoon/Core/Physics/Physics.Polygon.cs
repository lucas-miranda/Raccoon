using Raccoon.Components;

namespace Raccoon {
    public sealed partial class Physics {
        #region Polygon vs Polygon

        private bool CheckPolygonPolygon(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            PolygonShape shapeA = A as PolygonShape,
                         shapeB = B as PolygonShape;

            Polygon polygonA = new Polygon(shapeA.Shape), polygonB = new Polygon(shapeB.Shape);
            polygonA.Translate(APos);
            polygonB.Translate(BPos);

            if (TestSAT(polygonA, polygonB, out Contact? contact)) {
                contacts = new Contact[] {
                    contact.Value
                };

                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Polygon vs Polygon

        #region Polygon vs Box

        private bool CheckPolygonBox(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return CheckBoxPolygon(B, BPos, A, APos, out contacts);
        }

        #endregion Polygon vs Box

        #region Polygon vs Circle

        private bool CheckPolygonCircle(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return CheckCirclePolygon(B, BPos, A, APos, out contacts);
        }

        #endregion Polygon vs Circle

        #region Polygon vs Grid

        private bool CheckPolygonGrid(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            PolygonShape polygonShapeA = A as PolygonShape;
            GridShape gridB = B as GridShape;

            Rectangle polygonBoundingBox = new Rectangle(APos - polygonShapeA.BoundingBox / 2f, polygonShapeA.BoundingBox),
                      gridBoundingBox = new Rectangle(BPos, gridB.BoundingBox);

            // test grid bounds
            if (!gridBoundingBox.Intersects(polygonBoundingBox)) {
                contacts = null;
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
                contacts = new Contact[] {
                    contact.Value
                };
                return true;
            }

            contacts = null;
            return false;
        }

        #endregion Polygon vs Grid
    }
}
