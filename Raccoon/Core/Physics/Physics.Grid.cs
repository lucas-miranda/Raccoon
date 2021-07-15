using System.Collections.Generic;

namespace Raccoon {
    public sealed partial class Physics {
        #region Grid vs Grid

        private bool CheckGridGrid(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            contacts = null;
            return false;
        }

        #endregion Grid vs Grid

        #region Grid vs Box

        private bool CheckGridBox(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return CheckBoxGrid(B, BPos, A, APos, out contacts);
        }

        #endregion Grid vs Box

        #region Grid vs Circle

        private bool CheckGridCircle(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return CheckCircleGrid(B, BPos, A, APos, out contacts);
        }

        #endregion Grid vs Circle

        #region Grid vs Polygon

        private bool CheckGridPolygon(IShape A, Vector2 APos, IShape B, Vector2 BPos, out Contact[] contacts) {
            return CheckPolygonGrid(B, BPos, A, APos, out contacts);
        }

        #endregion Grid vs Polygon

        private List<Contact> TestGrid(GridShape grid, Vector2 gridPos, Rectangle otherBoundingBox, System.Func<Polygon, Contact?> SAT) {
            List<Contact> contacts = new List<Contact>();
            Polygon boxTilePolygon = new Polygon(grid.BoxTilePolygon);
            boxTilePolygon.Translate(gridPos);

            (int column, int row) start = grid.ConvertPosition(gridPos, otherBoundingBox.TopLeft - Vector2.One),
                                  end = grid.ConvertPosition(gridPos, otherBoundingBox.BottomRight + Vector2.One);

            List<Polygon> tilePolygons = new List<Polygon>();

            foreach ((int Column, int Row, GridShape.TileShape Shape) tile in grid.Tiles(start, end)) {
                tilePolygons.Clear();

                if (tile.Shape != null) {
                    if (tile.Shape is GridShape.BoxTileShape boxShape) {
                        tilePolygons.Add(boxShape.CreateCollisionPolygon(grid, gridPos, tile.Column, tile.Row));
                    } else if (tile.Shape is GridShape.PolygonTileShape polygonShape) {
                        tilePolygons.AddRange(polygonShape.CreateCollisionPolygons(grid, gridPos, tile.Column, tile.Row));
                    } else {
                        throw new System.InvalidOperationException($"Unable to find shape '{tile.Shape.GetType().Name}'.");
                    }
                }

                Location cell = new Location(tile.Column, tile.Row);
                foreach (Polygon tilePolygon in tilePolygons) {
                    Contact? contact = SAT(tilePolygon);
                    if (contact == null) {
                        continue;
                    }

                    contacts.Add(new Contact(contact.Value.Position, contact.Value.Normal, contact.Value.PenetrationDepth, cell));
                }
            }

            return contacts;
        }
    }
}
