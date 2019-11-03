using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class GridShape : IShape, System.IDisposable {
#if DEBUG
        public static readonly Color BackgroundGridColor = new Color(0x9B999AFF);
        public static bool DebugRenderDetailed = true;
#endif

        private Dictionary<uint, TileShape> _collisionShapes = new Dictionary<uint, TileShape>();
        private TileShape[,] _tilesData;

        private Vector2 _origin;

        public GridShape(Size tileSize, int columns, int rows) {
            TileSize = tileSize;
            Columns = columns;
            Rows = rows;
            TileBounds = new Rectangle(Vector2.Zero, new Size(Columns, Rows));
            BoundingBox = new Rectangle(Vector2.Zero, TileBounds.Size * TileSize);
            _tilesData = new TileShape[Rows, Columns];
            BoxTilePolygon = new Polygon(Vector2.Zero, new Vector2(TileSize.Width, 0), TileSize.ToVector2(), new Vector2(0, TileSize.Height));
        }

        public float Width { get { return BoundingBox.Width; } }
        public float Height { get { return BoundingBox.Height; } }
        public Size Size { get { return BoundingBox.Size; } }
        public Size TileSize { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int Area { get; }
        public Rectangle BoundingBox { get; private set; }
        public Rectangle TileBounds { get; private set; }
        public Polygon BoxTilePolygon { get; private set; }
        public bool IsDisposed { get; private set; }

        public Vector2 Origin {
            get {
                return _origin;
            }

            set {
                _origin = value;
                BoundingBox = new Rectangle(-Origin, TileBounds.Size * TileSize);
            }
        }

#if DEBUG
        public Color CollisionTilesColor { get; set; } = Color.Red;
#endif

        public void DebugRender(Vector2 position, Color color) {
#if DEBUG
            // background grid
            //Debug.DrawGrid(TileSize, Columns, Rows, position, BackgroundGridColor);

            // collision tiles
            foreach ((int column, int row, TileShape tile) in Tiles()) {
                if (tile == null) {
                    continue;
                }

                Vector2 tilePos = position - Origin + new Vector2(column * TileSize.Width, row * TileSize.Height);
                if (tile is BoxTileShape boxTile) {
                    Debug.DrawRectangle(new Rectangle(tilePos, TileSize), CollisionTilesColor);
                } else if (tile is PolygonTileShape polygonTile) {
                    /*Polygon p = new Polygon(polygonTile.Polygon);
                    p.Translate(tilePos);
                    Debug.DrawPolygon(p, Color.Cyan);*/

                    List<Vector2[]> components = polygonTile.Polygon.ConvexComponents();
                    Vector2[] points;
                    foreach (Vector2[] component in components) {
                        points = new Vector2[component.Length];
                        for (int i = 0; i < component.Length; i++) {
                            points[i] = tilePos + component[i];
                        }

                        Debug.DrawLines(points, CollisionTilesColor);
                    }

                    if (DebugRenderDetailed) {
                        Debug.DrawString(tilePos + TileSize / 2f, $"{components.Count}");
                    }
                }
            }

            // bounding box
            //Debug.DrawRectangle(new Rectangle(position, Debug.Transform(BoundingBox)), Color.Indigo);
#endif
        }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }

        public float ComputeMass(float density) {
            return 0f;
        }

        public Range Projection(Vector2 position, Vector2 axis) {
            return Range.Empty;
        }

        public Vector2[] CalculateAxes() {
            return new Vector2[] { };
        }

        public (Vector2 MaxProjectionVertex, Line Edge) FindBestClippingEdge(Vector2 shapePosition, Vector2 normal) {
            return (Vector2.Zero, new Line(Vector2.Zero, Vector2.Zero));
        }

        public ref TileShape GetTileInfo(int x, int y) {
            Debug.Assert(x >= 0 && x < Columns && y >= 0 && y <= Rows, $"[{x}, {y}] is out of grid bounds [{TileBounds}].");
            return ref _tilesData[y, x];
        }

        public void RegisterTileShape(TileShape tileShape) {
            _collisionShapes.Add(tileShape.Gid, tileShape);
        }

        public bool IsCollidable(int x, int y) {
            return GetTileInfo(x, y) != null;
        }

        public bool IsCollidable(Vector2 tile) {
            return IsCollidable((int) tile.X, (int) tile.Y);
        }

        public void SetCollidable(int x, int y, uint gid) {
            Debug.Assert(gid >= 0, "Gid must be greater or equals zero.");
            ref TileShape gridTile = ref GetTileInfo(x, y);

            if (_collisionShapes.TryGetValue(gid, out TileShape cacheTileShape)) {
                gridTile = cacheTileShape;
                return;
            }

            uint id = gid & ~(Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag),
                 flags = gid & (Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag);

            if (!_collisionShapes.TryGetValue(id, out TileShape originalTileShape)) {
                gridTile = null;
                return;
            }

            // extra processing
            switch (originalTileShape) {
                case BoxTileShape _:
                    gridTile = _collisionShapes[id];
                    break;

                case PolygonTileShape polygonTileShape:
                    Polygon polygon = new Polygon(polygonTileShape.Polygon);
                    switch (flags) {
                        case Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag:
                            polygon.RotateAround(270, TileSize.ToVector2() / 2f);
                            polygon.ReflectHorizontal(TileSize.Width / 2f);
                            break;

                        case Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag:
                            polygon.RotateAround(270, TileSize.ToVector2() / 2f);
                            break;

                        case Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag:
                            polygon.RotateAround(90, TileSize.ToVector2() / 2f);
                            break;

                        case Tiled.TiledTile.FlippedDiagonallyFlag:
                            polygon.RotateAround(90, TileSize.ToVector2() / 2f);
                            polygon.ReflectHorizontal(TileSize.Width / 2f);
                            break;

                        case Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag:
                            polygon.RotateAround(180, TileSize.ToVector2() / 2f);
                            break;

                        case Tiled.TiledTile.FlippedVerticallyFlag:
                            polygon.ReflectVertical(TileSize.Height / 2f);
                            break;

                        case Tiled.TiledTile.FlippedHorizontallyFlag:
                            polygon.ReflectHorizontal(TileSize.Width / 2f);
                            break;

                        default:
                            break;
                    }

                    gridTile = new PolygonTileShape(gid, polygon);
                    _collisionShapes[gid] = gridTile;
                    break;

                default:
                    break;
            }
        }

        public void LoadTilesCollisionData(uint[] data) {
            for (int row = 0; row < Rows; row++) {
                for (int column = 0; column < Columns; column++) {
                    SetCollidable(column, row, data[row * Columns + column]);
                }
            }
        }

        public (int column, int row) ConvertPosition(Vector2 shapePosition, Vector2 position) {
            position -= shapePosition;
            return ((int) (position.X / TileSize.Width), (int) (position.Y / TileSize.Height));
        }

        public Vector2 ConvertTilePosition(Vector2 shapePosition, int column, int row) {
            return shapePosition + new Vector2(column * TileSize.Width, row * TileSize.Height);
        }

        public IEnumerable<(int column, int row, TileShape shape)> Tiles(int startColumn, int startRow, int endColumn, int endRow) {
            startColumn = Math.Max(0, startColumn);
            startRow = Math.Max(0, startRow);
            endColumn = Math.Min(Columns - 1, endColumn);
            endRow = Math.Min(Rows - 1, endRow);
            for (int row = startRow; row <= endRow; row++) {
                for (int column = startColumn; column <= endColumn; column++) {
                    yield return (column, row, _tilesData[row, column]);
                }
            }
        }

        public IEnumerable<(int column, int row, TileShape shape)> Tiles((int column, int row) start, (int column, int row) end) {
            return Tiles(start.column, start.row, end.column, end.row);
        }

        public IEnumerable<(int column, int row, TileShape shape)> Tiles() {
            return Tiles(0, 0, Columns - 1, Rows - 1);
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            _tilesData = null;
            _collisionShapes = null;
            BoxTilePolygon = null;

            IsDisposed = true;
        }

        #region TileShape Class

        public abstract class TileShape {
            public TileShape(uint gid) {
                Gid = gid;
            }

            public uint Gid { get; private set; }
            public uint Id { get { return Gid & ~(Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag); } }

            public override string ToString() {
                return $"[TileShape | Gid: {Gid}, Id: {Id}]";
            }
        }

        #endregion TileShape Class

        #region BoxTileShape Class

        public class BoxTileShape : TileShape {
            public BoxTileShape(uint gid) : base(gid) { }

            public Polygon CreateCollisionPolygon(GridShape grid, Vector2 gridPos, int column, int row) {
                Polygon boxPolygon = new Polygon(grid.BoxTilePolygon);
                boxPolygon.Translate(grid.ConvertTilePosition(gridPos, column, row));
                return boxPolygon;
            }

            public override string ToString() {
                return $"[BoxTileShape | Gid: {Gid}, Id: {Id}]";
            }
        }

        #endregion BoxTileShape Class

        #region PolygonTileShape Class

        public class PolygonTileShape : TileShape {
            public PolygonTileShape(uint gid, Polygon polygon) : base(gid) {
                Polygon = polygon;
            }

            public Polygon Polygon { get; private set; }

            public Polygon[] CreateCollisionPolygons(GridShape grid, Vector2 gridPos, int column, int row) {
                List<Vector2[]> convexComponents = Polygon.ConvexComponents();
                Polygon[] polygons = new Polygon[convexComponents.Count];
                for (int i = 0; i < convexComponents.Count; i++) {
                    Polygon p = new Polygon(convexComponents[i]);
                    p.Translate(grid.ConvertTilePosition(gridPos, column, row));
                    polygons[i] = p;
                }

                return polygons;
            }

            public Polygon CreateCollisionPolygon(GridShape grid, Vector2 gridPos, int column, int row) {
                Polygon polygon = new Polygon(Polygon);
                polygon.Translate(grid.ConvertTilePosition(gridPos, column, row));
                return polygon;
            }

            public override string ToString() {
                return $"[PolygonTileShape | Gid: {Gid}, Id: {Id}, Polygon: {Polygon}]";
            }
        }

        #endregion PolygonTileShape Class
    }
}
