using System;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;

namespace Raccoon.Components {
    public class RichGridCollider : Collider {
        private Size _tileSize;
        private Dictionary<uint, Tile> _collisionTiles = new Dictionary<uint, Tile>();
        private Tile[,] _data = new Tile[0,0];
        private bool _graphicNeedUpdate;

        public RichGridCollider(Size tileSize, int columns, int rows, params string[] tags) : base(tags) {
            Initialize(tileSize, columns, rows);
        }

        public RichGridCollider(Size tileSize, int columns, int rows, params Enum[] tags) : base(tags) {
            Initialize(tileSize, columns, rows);
        }

        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public Size TileSize {
            get {
                return _tileSize;
            }

            set {
                _tileSize = value;
                Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);
            }
        }

        public override void DebugRender() {
            Size graphicSize = new Size((float) Math.Floor(TileSize.Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1), (float) Math.Floor(TileSize.Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1));
            if (_graphicNeedUpdate || Graphic.Size != graphicSize) {
                (Graphic as RectanglePrimitive).Size = graphicSize;
                _graphicNeedUpdate = false;
            }

            for (int y = 0; y < _data.GetLength(0); y++) {
                for (int x = 0; x < _data.GetLength(1); x++) {
                    Tile tile = _data[y, x];
                    if (tile == null) {
                        continue;
                    }

                    if (tile is PolygonTile) {
                        PolygonTile polygonTile = tile as PolygonTile;
                        polygonTile.Graphic.Color = Color;
                        polygonTile.Graphic.Render(Position + new Vector2(x, y) * TileSize);
                    } else {
                        Graphic.Color = Color;
                        Graphic.Render(Position * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + new Vector2(x, y) * TileSize * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom);
                    }
                }
            }
        }

        public void Setup(int columns, int rows) {
            Columns = columns;
            Rows = rows;
            _data = new Tile[Rows, Columns];
            Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);
        }

        public void LoadTilesData(uint[] data) {
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    SetCollidable(x, y, data[y * Columns + x]);
                }
            }
        }

        public bool IsCollidable(int x, int y) {
            return !(x < 0 || x >= Columns || y < 0 || y >= Rows) && _data[y, x] != null;
        }
        
        public void SetCollidable(int x, int y, uint gid) {
            if (x < 0 || x >= Columns || y < 0 || y >= Rows) {
                return;
            }

            Tile tile = null;

            if (gid > 0 && !_collisionTiles.TryGetValue(gid, out tile)) {
                uint id = gid & ~(Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag);
                if (!_collisionTiles.TryGetValue(id, out tile)) {
                    return;
                }

                // polygon tiles need an extra treatment
                if (tile is PolygonTile) {
                    // create missing rotated polygon from original polygon
                    Polygon poly = (tile as PolygonTile).Polygon.Clone();
                    uint flags = gid & (Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag);
                    switch (flags) {
                        case Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag:
                            poly.RotateAround(270, TileSize.ToVector2() / 2);
                            poly.ReflectHorizontal(TileSize.Width / 2);
                            break;

                        case Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag:
                            poly.RotateAround(270, TileSize.ToVector2() / 2);
                            break;

                        case Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag:
                            poly.RotateAround(90, TileSize.ToVector2() / 2);
                            break;

                        case Tiled.TiledTile.FlippedDiagonallyFlag:
                            poly.RotateAround(270, TileSize.ToVector2() / 2);
                            poly.ReflectHorizontal(TileSize.Width / 2);
                            break;

                        case Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag:
                            poly.RotateAround(180, TileSize.ToVector2() / 2);
                            break;

                        case Tiled.TiledTile.FlippedVerticallyFlag:
                            poly.ReflectVertical(TileSize.Height / 2);
                            break;

                        case Tiled.TiledTile.FlippedHorizontallyFlag:
                            poly.ReflectHorizontal(TileSize.Width / 2);
                            break;

                        default:
                            break;
                    }

                    tile = new PolygonTile(gid, poly);
                    _collisionTiles[gid] = tile;
                }
            }

            _data[y, x] = tile;
        }

        public Tile GetTileInfo(int x, int y) {
            return _data[y, x];
        }

        public void RegisterTile(Tile tile) {
            _collisionTiles.Add(tile.Gid, tile);
        }

        public void UnregisterTile(uint gid) {
            _collisionTiles.Remove(gid);
        }

        public void UnregisterTile(Tile tile) {
            UnregisterTile(tile.Gid);
        }

        private void Initialize(Size tileSize, int columns, int rows) {
            Setup(columns, rows);
            TileSize = tileSize;
            Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);

#if DEBUG
            Graphic = new RectanglePrimitive(TileSize.Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1, TileSize.Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1, Color, false) {
                Surface = Game.Instance.Core.DebugSurface
            };

            _graphicNeedUpdate = false;
#endif
        }

        #region Tile Class

        public abstract class Tile {
            public Tile(uint gid) {
                Gid = gid;
            }

            public uint Gid { get; private set; }
        }

        #endregion Tile Class

        #region BoxTile Class

        public class BoxTile : Tile {
            public BoxTile(uint gid) : base(gid) { }
        }

        #endregion BoxTile Class

        #region PolygonTile Class

        public class PolygonTile : Tile {
            public PolygonTile(uint gid, Polygon polygon) : base(gid) {
                Polygon = polygon;

#if DEBUG
                Graphic = new Graphics.Primitives.PolygonPrimitive(Polygon, Color.Red);
#endif
            }

            public Polygon Polygon { get; private set; }

#if DEBUG
            public Graphics.Primitives.PolygonPrimitive Graphic { get; protected set; }
#endif
        }

        #endregion PolygonTile Class
    }
}
