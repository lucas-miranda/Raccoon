﻿using System.Collections.Generic;

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

            (int column, int row) start = grid.ConvertPosition(gridPos, otherBoundingBox.TopLeft),
                                  end = grid.ConvertPosition(gridPos, otherBoundingBox.BottomRight);

            //List<Contact> contacts = new List<Contact>();
            List<Polygon> tilePolygons = new List<Polygon>();

            foreach ((int column, int row, GridShape.TileShape shape) in grid.Tiles(start, end)) {
                tilePolygons.Clear();

                switch (shape) {
                    case GridShape.BoxTileShape boxShape:
                        Polygon boxPolygon = new Polygon(boxTilePolygon);
                        boxPolygon.Translate(grid.ConvertTilePosition(gridPos, column, row));
                        tilePolygons.Add(boxPolygon);
                        break;

                    case GridShape.PolygonTileShape polygonShape:
                        foreach (Vector2[] component in polygonShape.Polygon.ConvexComponents()) {
                            Polygon p = new Polygon(component);
                            p.Translate(gridPos + grid.ConvertTilePosition(gridPos, column, row));
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

                    //Debug.Info($"Collision with tile shape: {shape}");
                    //totalContact = totalContact == null ? contact : Contact.Sum(contact.Value, totalContact.Value);
                    contacts.Add(contact.Value);
                }
            }

            return contacts;
        }
    }
}
