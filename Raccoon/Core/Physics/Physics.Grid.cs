using System.Collections.Generic;
using Raccoon.Components;

namespace Raccoon {
    public sealed partial class Physics {
        #region Grid vs Grid

        private bool CheckGridGrid(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            manifold = null;
            return false;
        }

        #endregion Grid vs Grid

        #region Grid vs Box

        private bool CheckGridBox(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return CheckBoxGrid(B, BPos, A, APos, out manifold);
        }

        #endregion Grid vs Box

        #region Grid vs Circle

        private bool CheckGridCircle(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return CheckCircleGrid(B, BPos, A, APos, out manifold);
        }

        #endregion Grid vs Circle

        #region Grid vs Polygon

        private bool CheckGridPolygon(Body A, Vector2 APos, Body B, Vector2 BPos, out Manifold manifold) {
            return CheckPolygonGrid(B, BPos, A, APos, out manifold);
        }

        #endregion Grid vs Polygon

        private Contact? TestGrid(GridShape grid, Vector2 gridPos, Rectangle otherBoundingBox, System.Func<Polygon, Contact?> SAT) {
            Contact? totalContact = null;
            Polygon boxTilePolygon = new Polygon(grid.BoxTilePolygon);
            boxTilePolygon.Translate(gridPos);

            (int column, int row) start = grid.ConvertPosition(otherBoundingBox.TopLeft),
                                  end = grid.ConvertPosition(otherBoundingBox.BottomRight);

            //List<Contact> contacts = new List<Contact>();
            List<Polygon> tilePolygons = new List<Polygon>();

            foreach ((int column, int row, GridShape.TileShape shape) in grid.Tiles(start, end)) {
                tilePolygons.Clear();
                switch (shape) {
                    case GridShape.BoxTileShape boxShape:
                        tilePolygons.Add(new Polygon(boxTilePolygon));
                        tilePolygons[0].Translate(grid.ConvertTilePosition(column, row));
                        break;

                    case GridShape.PolygonTileShape polygonShape:
                        foreach (Polygon componentPolygon in polygonShape.Polygon.ConvexComponents()) {
                            Polygon p = new Polygon(componentPolygon);
                            p.Translate(gridPos + grid.ConvertTilePosition(column, row));
                            tilePolygons.Add(p);
                        }
                        break;

                    case null:
                        continue;

                    default:
                        Debug.Assert(false, $"Unable to find shape '{shape.GetType().Name}'.");
                        continue;
                }

                foreach (Polygon tilePolygon in tilePolygons) {
                    Contact? contact = SAT(tilePolygon);
                    if (contact == null) {
                        continue;
                    }

                    totalContact = totalContact == null ? contact : Contact.Sum(contact.Value, totalContact.Value);
                }
            }

            return totalContact;
        }
    }
}
