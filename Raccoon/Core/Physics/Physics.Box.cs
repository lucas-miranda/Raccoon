using System.Collections.Generic;
using Raccoon.Components;

namespace Raccoon {
    public partial class Physics {
        #region Box vs Box

        private bool CheckBoxBox(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxAColl = colliderA as BoxCollider, boxBColl = colliderB as BoxCollider;
            if (boxAColl.Rotation != 0 || boxBColl.Rotation != 0) { // SAT
                // box collider polygon
                Polygon boxACollPolygon = boxAColl.Polygon;
                boxACollPolygon.Translate(colliderAPos);

                // other collider polygon
                Polygon boxBCollPolygon = boxBColl.Polygon;
                boxBCollPolygon.Translate(colliderBPos);

                Vector2[] axes = new Vector2[] {
                                (boxACollPolygon[0] - boxACollPolygon[1]).Perpendicular(),
                                (boxACollPolygon[1] - boxACollPolygon[2]).Perpendicular(),
                                (boxBCollPolygon[0] - boxBCollPolygon[1]).Perpendicular(),
                                (boxBCollPolygon[1] - boxBCollPolygon[2]).Perpendicular()
                            };

                return CheckPolygonsIntersection(boxACollPolygon, boxBCollPolygon, axes);
            }

            return new Rectangle(colliderAPos, boxAColl.Size) & new Rectangle(colliderBPos, boxBColl.Size); // regular AABB
        }

        #endregion Box vs Box

        #region Box vs Grid

        private bool CheckBoxGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            GridCollider gridColl = colliderB as GridCollider;

            int startColumn, startRow, endColumn, endRow, row, column;
            if (boxColl.Rotation != 0) {
                // box collider polygon
                Polygon boxCollPolygon = boxColl.Polygon;
                boxCollPolygon.Translate(colliderAPos);

                Vector2[] axes = new Vector2[] {
                            new Vector2(1, 0),
                            new Vector2(0, 1),
                            (boxCollPolygon[0] - boxCollPolygon[1]).Perpendicular(),
                            (boxCollPolygon[1] - boxCollPolygon[2]).Perpendicular()
                        };

                float top = boxCollPolygon[0].Y, right = boxCollPolygon[0].X, bottom = boxCollPolygon[0].Y, left = boxCollPolygon[0].X;
                foreach (Vector2 vertex in boxCollPolygon) {
                    if (vertex.Y < top)
                        top = vertex.Y;
                    if (vertex.X > right)
                        right = vertex.X;
                    if (vertex.Y > bottom)
                        bottom = vertex.Y;
                    if (vertex.X < left)
                        left = vertex.X;
                }

                startColumn = (int) (left - colliderBPos.X) / (int) gridColl.TileSize.Width;
                startRow = (int) (top - colliderBPos.Y) / (int) gridColl.TileSize.Height;
                endColumn = (int) (right - colliderBPos.X) / (int) gridColl.TileSize.Width;
                endRow = (int) (bottom - colliderBPos.Y) / (int) gridColl.TileSize.Height;

                for (row = startRow; row <= endRow; row++) {
                    for (column = startColumn; column <= endColumn; column++) {
                        if (!gridColl.IsCollidable(column, row)) {
                            continue;
                        }

                        if (CheckPolygonsIntersection(boxCollPolygon,
                            new Polygon(new Vector2(colliderBPos.X + column * gridColl.TileSize.Width, colliderBPos.Y + row * gridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * gridColl.TileSize.Width, colliderBPos.Y + row * gridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * gridColl.TileSize.Width, colliderBPos.Y + (row + 1) * gridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + column * gridColl.TileSize.Width, colliderBPos.Y + (row + 1) * gridColl.TileSize.Height)
                            ), axes)) {
                            return true;
                        }
                    }
                }

                return false;
            }

            Rectangle boxRect = new Rectangle(colliderAPos, boxColl.Size);
            if (!(boxRect & new Rectangle(colliderBPos, gridColl.Size))) { // out of grid
                return false;
            }

            startColumn = (int) (boxRect.Left - colliderBPos.X) / (int) gridColl.TileSize.Width;
            startRow = (int) (boxRect.Top - colliderBPos.Y) / (int) gridColl.TileSize.Height;
            endColumn = (int) (boxRect.Right - colliderBPos.X - 1) / (int) gridColl.TileSize.Width;
            endRow = (int) (boxRect.Bottom - colliderBPos.Y - 1) / (int) gridColl.TileSize.Height;
            for (row = startRow; row <= endRow; row++) {
                for (column = startColumn; column <= endColumn; column++) {
                    if (gridColl.IsCollidable(column, row)) {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Box vs Grid

        #region Box vs Circle

        private bool CheckBoxCircle(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            CircleCollider circleColl = colliderB as CircleCollider;

            if (boxColl.Rect & circleColl.Position) {
                return true;
            }

            float radiusSquared = circleColl.Radius * circleColl.Radius;
            return Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X - 1, colliderAPos.Y - 1), new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y - 1)), circleColl.Position) < radiusSquared
                || Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y - 1), new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y + boxColl.Height)), circleColl.Position) < radiusSquared
                || Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X + boxColl.Width, colliderAPos.Y + boxColl.Height), new Vector2(colliderAPos.X - 1, colliderAPos.Y + boxColl.Height)), circleColl.Position) < radiusSquared
                || Util.Math.DistanceSquared(new Line(new Vector2(colliderAPos.X - 1, colliderAPos.Y + boxColl.Height), new Vector2(colliderAPos.X, colliderAPos.Y - 1)), circleColl.Position) < radiusSquared;
        }

        #endregion Box vs Circle

        #region Box vs Line

        private bool CheckBoxLine(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            LineCollider lineColl = colliderB as LineCollider;

            // box collider polygon
            Polygon boxPolygon = boxColl.Polygon;
            boxPolygon.Translate(colliderAPos);

            // line collider polygon
            Polygon linePolygon = new Polygon(lineColl.From, lineColl.To);
            linePolygon.Translate(colliderBPos);

            Vector2[] axes = new Vector2[] { 
                // box relevant axes
                (boxPolygon[0] - boxPolygon[1]).Perpendicular(),
                (boxPolygon[1] - boxPolygon[2]).Perpendicular(),

                // line axis
                (linePolygon[0] - linePolygon[1]).Perpendicular()
            };

            return CheckPolygonsIntersection(boxPolygon, linePolygon, axes);
        }

        #endregion Box vs Line

        #region Box vs Polygon

        private bool CheckBoxPolygon(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            PolygonCollider polygonColl = colliderB as PolygonCollider;

            // box collider polygon
            Polygon boxPolygon = boxColl.Polygon;
            boxPolygon.Translate(colliderAPos);

            // polygon collider polygon
            Polygon polygon = polygonColl.Polygon.Clone();
            polygon.Translate(colliderBPos);

            List<Vector2> axes = new List<Vector2> {
                // box relevant axes
                (boxPolygon[0] - boxPolygon[1]).Perpendicular(),
                (boxPolygon[1] - boxPolygon[2]).Perpendicular()
            };

            // polygon axes
            for (int i = 0; i < polygon.VertexCount; i++) {
                axes.Add((polygon[i] - polygon[(i + 1) % polygon.VertexCount]).Perpendicular());
            }

            return CheckPolygonsIntersection(boxPolygon, polygon, axes);
        }

        #endregion Box vs Polygon

        #region Box vs RichGrid

        private bool CheckBoxRichGrid(Collider colliderA, Vector2 colliderAPos, Collider colliderB, Vector2 colliderBPos) {
            BoxCollider boxColl = colliderA as BoxCollider;
            RichGridCollider richGridColl = colliderB as RichGridCollider;

            int startColumn, startRow, endColumn, endRow, row, column;
            /*if (boxColl.Rotation != 0) {
                // box collider polygon
                Polygon boxCollPolygon = boxColl.Polygon;
                boxCollPolygon.Translate(colliderAPos);

                Vector2[] axes = new Vector2[] {
                            new Vector2(1, 0),
                            new Vector2(0, 1),
                            (boxCollPolygon[0] - boxCollPolygon[1]).Perpendicular(),
                            (boxCollPolygon[1] - boxCollPolygon[2]).Perpendicular()
                        };

                float top = boxCollPolygon[0].Y, right = boxCollPolygon[0].X, bottom = boxCollPolygon[0].Y, left = boxCollPolygon[0].X;
                foreach (Vector2 vertex in boxCollPolygon) {
                    if (vertex.Y < top)
                        top = vertex.Y;
                    if (vertex.X > right)
                        right = vertex.X;
                    if (vertex.Y > bottom)
                        bottom = vertex.Y;
                    if (vertex.X < left)
                        left = vertex.X;
                }

                startColumn = (int) (left - colliderBPos.X) / (int) richGridColl.TileSize.Width;
                startRow = (int) (top - colliderBPos.Y) / (int) richGridColl.TileSize.Height;
                endColumn = (int) (right - colliderBPos.X) / (int) richGridColl.TileSize.Width;
                endRow = (int) (bottom - colliderBPos.Y) / (int) richGridColl.TileSize.Height;

                for (row = startRow; row <= endRow; row++) {
                    for (column = startColumn; column <= endColumn; column++) {
                        if (!richGridColl.IsCollidable(column, row)) {
                            continue;
                        }

                        if (CheckPolygonsIntersection(boxCollPolygon,
                            new Polygon(new Vector2(colliderBPos.X + column * richGridColl.TileSize.Width, colliderBPos.Y + row * richGridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * richGridColl.TileSize.Width, colliderBPos.Y + row * richGridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + (column + 1) * richGridColl.TileSize.Width, colliderBPos.Y + (row + 1) * richGridColl.TileSize.Height),
                                new Vector2(colliderBPos.X + column * richGridColl.TileSize.Width, colliderBPos.Y + (row + 1) * richGridColl.TileSize.Height)
                            ), axes)) {
                            return true;
                        }
                    }
                }

                return false;
            }*/

            Rectangle boxRect = new Rectangle(colliderAPos, boxColl.Size);
            if (!(boxRect & new Rectangle(colliderBPos, richGridColl.Size))) { // out of grid
                return false;
            }

            // box collider polygon
            Polygon boxPolygon = boxColl.Polygon;
            boxPolygon.Translate(colliderAPos);

            startColumn = (int) (boxRect.Left - colliderBPos.X) / (int) richGridColl.TileSize.Width;
            startRow = (int) (boxRect.Top - colliderBPos.Y) / (int) richGridColl.TileSize.Height;
            endColumn = (int) (boxRect.Right - colliderBPos.X - 1) / (int) richGridColl.TileSize.Width;
            endRow = (int) (boxRect.Bottom - colliderBPos.Y - 1) / (int) richGridColl.TileSize.Height;
            for (row = startRow; row <= endRow; row++) {
                for (column = startColumn; column <= endColumn; column++) {
                    if (!richGridColl.IsCollidable(column, row)) {
                        continue;
                    }

                    RichGridCollider.Tile tile = richGridColl.GetTileInfo(column, row);
                    if (tile is RichGridCollider.BoxTile) {
                        return true;
                    }

                    // rich grid collider tile (column, row) polygon
                    Polygon tilePolygon = (tile as RichGridCollider.PolygonTile).Polygon.Clone();

                    List<Vector2> axes = new List<Vector2>();
                    Vector2 boxAxis0 = (boxPolygon[0] - boxPolygon[1]).Perpendicular(), 
                            boxAxis1 = (boxPolygon[1] - boxPolygon[2]).Perpendicular();

                    if (tilePolygon.IsConvex) {
                        tilePolygon.Translate(colliderBPos + new Vector2(column, row) * richGridColl.TileSize);

                        // box relevant axes
                        axes.Add(boxAxis0);
                        axes.Add(boxAxis1);

                        // tile axes
                        for (int i = 0; i < tilePolygon.VertexCount; i++) {
                            axes.Add((tilePolygon[i] - tilePolygon[(i + 1) % tilePolygon.VertexCount]).Perpendicular());
                        }

                        if (CheckPolygonsIntersection(boxPolygon, tilePolygon, axes)) {
                            return true;
                        }
                    } else {
                        foreach (Polygon convexComponent in tilePolygon.GetConvexComponents()) {
                            convexComponent.Translate(colliderBPos + new Vector2(column, row) * richGridColl.TileSize);

                            axes.Clear();

                            // box relevant axes
                            axes.Add(boxAxis0);
                            axes.Add(boxAxis1);

                            // tile axes
                            for (int i = 0; i < convexComponent.VertexCount; i++) {
                                axes.Add((convexComponent[i] - convexComponent[(i + 1) % convexComponent.VertexCount]).Perpendicular());
                            }

                            if (CheckPolygonsIntersection(boxPolygon, convexComponent, axes)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion Box vs RichGrid
    }
}
