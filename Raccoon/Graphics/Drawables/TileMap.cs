using System;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class TileMap : Graphic {
        #region Private Static Members

        private static readonly Regex GidRegex = new Regex(@"(\d+)");

        #endregion Private Static Members

        #region Private Members

#if DEBUG
        private Grid _grid;
#endif

        private int _tileSetRows, _tileSetColumns, _triangleCount;
        private uint[] _tilesIds = new uint[0];
        private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[0];
        //private Vector2 _texSpriteCount;
        private Microsoft.Xna.Framework.Matrix _lastWorldMatrix;

        #endregion Private Members

        #region Constructors

        public TileMap(Texture texture, Size tileSize) : base() {
            TileSize = tileSize;
            Texture = texture;
            Load();
        }

        public TileMap(string filename, Size tileSize) : this(new Texture(filename), tileSize) { }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; set; }
        public Size TileSize { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public uint[] Data { get { return _tilesIds; } }

#if DEBUG
        public new Vector2 Scroll {
            get {
                return base.Scroll;
            }

            set {
                base.Scroll = value;
                if (_grid != null) {
                    _grid.Scroll = value;
                }
            }
        }
#endif

        #endregion Public Properties

        #region Public Methods

        public override void Render(Vector2 position, Color color, float rotation) {
            if (_vertices.Length == 0) {
                return;
            }

            Game.Instance.Core.BasicEffect.TextureEnabled = true;
            Game.Instance.Core.BasicEffect.Texture = Texture.XNATexture;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = Opacity;
            Game.Instance.Core.BasicEffect.World = _lastWorldMatrix = Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X * Scroll.X, position.Y * Scroll.Y, 0f) * Microsoft.Xna.Framework.Matrix.CreateLookAt(new Microsoft.Xna.Framework.Vector3(0f, 0f, 1f), new Microsoft.Xna.Framework.Vector3(0f, 0f, -1f), Microsoft.Xna.Framework.Vector3.Up) * Surface.World;
            Game.Instance.Core.BasicEffect.View = Microsoft.Xna.Framework.Matrix.CreateScale(1f / Scroll.X, 1f / Scroll.Y, 1f) * Surface.View * Microsoft.Xna.Framework.Matrix.CreateScale(Scroll.X, Scroll.Y, 1f);
            Game.Instance.Core.BasicEffect.Projection = Surface.Projection;
            
            foreach (EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _triangleCount);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
            Game.Instance.Core.BasicEffect.Texture = null;
            Game.Instance.Core.BasicEffect.TextureEnabled = false;
        }

        public override void DebugRender() {
#if DEBUG
            if (_grid == null) {
                return;
            }

            _grid.Render(Position, Rotation);
#endif
        }

        public void Setup(int columns, int rows) {
            if (Columns == columns && Rows == rows) {
                return;
            }

            int oldColumns = Columns, oldRows = Rows;
            Columns = columns;
            Rows = rows;
            Size = new Size(Columns * TileSize.Width, Rows * TileSize.Height);

            VertexPositionColorTexture[] newVertices = new VertexPositionColorTexture[Columns * Rows * 6];
            uint[] newTilesIds = new uint[Columns * Rows];
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    int newTileId = (y * Columns + x) * 6;
                    if (y < oldRows && x < oldColumns) {
                        int oldTileId = (y * oldColumns + x) * 6;
                        newVertices[newTileId] = _vertices[oldTileId];
                        newVertices[newTileId + 1] = _vertices[oldTileId + 1];
                        newVertices[newTileId + 2] = _vertices[oldTileId + 2];
                        newVertices[newTileId + 3] = _vertices[oldTileId + 3];
                        newVertices[newTileId + 4] = _vertices[oldTileId + 4];
                        newVertices[newTileId + 5] = _vertices[oldTileId + 5];
                        newTilesIds[y * Columns + x] = _tilesIds[y * oldColumns + x];
                    } else {
                        newVertices[newTileId] = newVertices[newTileId + 1] = newVertices[newTileId + 2] = newVertices[newTileId + 3] = newVertices[newTileId + 4] = newVertices[newTileId + 5] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White, Vector2.Zero);
                        newTilesIds[y * Columns + x] = 0;
                    }
                }
            }

            _vertices = newVertices;
            _triangleCount = Columns * Rows * 2;
            _tilesIds = newTilesIds;
#if DEBUG
            if (_grid == null) {
                _grid = new Grid(TileSize) {
                    Scroll = Scroll
                };
            }

            _grid.Setup(Columns, Rows);
#endif
        }

        public void SetData(uint[][] data) {
            int greaterRowSize = 0;
            for (int row = 0; row < data.Length; row++) {
                if (data[row].Length > greaterRowSize) {
                    greaterRowSize = data[row].Length;
                }
            }

            Setup(greaterRowSize, data.Length);

            Microsoft.Xna.Framework.Vector3 displacement = new Microsoft.Xna.Framework.Vector3(X, Y, 0);
            Vector2 texSpriteCount = new Vector2((int) (Texture.Width / TileSize.Width), (int) (Texture.Height / TileSize.Height));
            for (int y = 0; y < data.Length; y++) {
                for (int x = 0; x < data[y].Length; x++) {
                    SetTile(x, y, data[y][x]);
                }
            }
        }

        public void SetData(uint[,] data) {
            Setup(data.GetLength(1), data.GetLength(0));

            Microsoft.Xna.Framework.Vector3 displacement = new Microsoft.Xna.Framework.Vector3(X, Y, 0);
            Vector2 texSpriteCount = new Vector2((int) (Texture.Width / TileSize.Width), (int) (Texture.Height / TileSize.Height));
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    SetTile(x, y, data[y, x]);
                }
            }
        }

        public void SetData(string csv, int columns, int rows) {
            uint[][] newData = new uint[rows][];
            for (int row = 0; row < newData.Length; row++) {
                newData[row] = new uint[columns];
            }

            int x = 0, y = 0;
            foreach (Match m in GidRegex.Matches(csv)) {
                newData[y][x] = uint.Parse(m.Value);
                x++;
                if (x == columns) {
                    x = 0;
                    y++;
                    if (y == rows) {
                        break;
                    }
                }
            }

            SetData(newData);
        }

        public uint GetTile(int x, int y) {
            if (!ExistsTile(x, y)) throw new ArgumentException($"x ({x}) or y ({y}) out of bounds [0 0 {Columns} {Rows}]");
            return _tilesIds[y * Columns + x];
        }

        public uint[] GetTiles(Rectangle area) {
            if (area.Left < 0 || area.Right > Columns || area.Top < 0 || area.Bottom > Rows) throw new ArgumentException($"area {area} out of bounds [0 0 {Columns} {Rows}]");

            int i = 0;
            uint[] tiles = new uint[(uint) area.Area];
            for (int y = (int) area.Top; y < area.Bottom; y++) {
                for (int x = (int) area.Left; x < area.Right; x++) {
                    tiles[i] = GetTile(x, y);
                    i++;
                }
            }

            return tiles;
        }

        public void SetTile(int x, int y, uint gid) {
            if (!ExistsTile(x, y)) throw new ArgumentException($"x or y out of bounds [0 0 {Columns} {Rows}]");

            int tileId = (y * Columns + x) * 6;
            _tilesIds[y * Columns + x] = gid;

            // empty tile
            if (gid == 0) {
                _vertices[tileId] = _vertices[tileId + 1] = _vertices[tileId + 2] = _vertices[tileId + 3] = _vertices[tileId + 4] = _vertices[tileId + 5] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White, Vector2.Zero);
                return;
            }

            bool isAntiDiagonally = (gid & Tiled.TiledTile.FlippedDiagonallyFlag) != 0;

            ImageFlip flip = ImageFlip.None;
            if ((gid & Tiled.TiledTile.FlippedHorizontallyFlag) != 0) {
                flip |= ImageFlip.Horizontal;
            }

            if ((gid & Tiled.TiledTile.FlippedVerticallyFlag) != 0) {
                flip |= ImageFlip.Vertical;
            }

            int id = (int) (gid & ~(Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag)) - 1; // clear flags
            float texLeft = (id % _tileSetColumns) * TileSize.Width, texTop = (id / _tileSetColumns) * TileSize.Height;

            _vertices[tileId] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, y * TileSize.Height, 0), Color.White, new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, texTop / Texture.Height));
            _vertices[tileId + 1] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, y * TileSize.Height, 0), Color.White, new Microsoft.Xna.Framework.Vector2((texLeft + TileSize.Width) / Texture.Width, texTop / Texture.Height));
            _vertices[tileId + 2] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, (y + 1) * TileSize.Height, 0), Color.White, new Microsoft.Xna.Framework.Vector2((texLeft + TileSize.Width) / Texture.Width, (texTop + TileSize.Height) / Texture.Height));

            _vertices[tileId + 3] = _vertices[tileId + 2];
            _vertices[tileId + 4] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, (y + 1) * TileSize.Height, 0), Color.White, new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, (texTop + TileSize.Height) / Texture.Height));
            _vertices[tileId + 5] = _vertices[tileId];

            if (isAntiDiagonally) {
                VertexPositionColorTexture tmp;
                _vertices[tileId + 1].Position += new Microsoft.Xna.Framework.Vector3(-TileSize.Width, TileSize.Height, 0);
                _vertices[tileId + 4].Position += new Microsoft.Xna.Framework.Vector3(TileSize.Width, -TileSize.Height, 0);

                tmp = _vertices[tileId + 4];
                _vertices[tileId + 4] = _vertices[tileId + 1];
                _vertices[tileId + 1] = tmp;
            }

            if (flip.HasFlag(ImageFlip.Horizontal)) {
                VertexPositionColorTexture tmp;

                _vertices[tileId].Position += new Microsoft.Xna.Framework.Vector3(TileSize.Width, 0, 0);
                _vertices[tileId + 1].Position += new Microsoft.Xna.Framework.Vector3(-TileSize.Width, 0, 0);

                tmp = _vertices[tileId + 1];
                _vertices[tileId + 1] = _vertices[tileId];
                _vertices[tileId + 5] = _vertices[tileId] = tmp;

                _vertices[tileId + 2].Position += new Microsoft.Xna.Framework.Vector3(-TileSize.Width, 0, 0);
                _vertices[tileId + 4].Position += new Microsoft.Xna.Framework.Vector3(TileSize.Width, 0, 0);

                tmp = _vertices[tileId + 4];
                _vertices[tileId + 4] = _vertices[tileId + 2];
                _vertices[tileId + 3] = _vertices[tileId + 2] = tmp;
            }

            if (flip.HasFlag(ImageFlip.Vertical)) {
                VertexPositionColorTexture tmp;

                _vertices[tileId].Position += new Microsoft.Xna.Framework.Vector3(0, TileSize.Height, 0);
                _vertices[tileId + 4].Position += new Microsoft.Xna.Framework.Vector3(0, -TileSize.Height, 0);

                tmp = _vertices[tileId + 4];
                _vertices[tileId + 4] = _vertices[tileId];
                _vertices[tileId + 5] = _vertices[tileId] = tmp;

                _vertices[tileId + 1].Position += new Microsoft.Xna.Framework.Vector3(0, TileSize.Height, 0);
                _vertices[tileId + 2].Position += new Microsoft.Xna.Framework.Vector3(0, -TileSize.Height, 0);

                tmp = _vertices[tileId + 2];
                _vertices[tileId + 3] = _vertices[tileId + 2] = _vertices[tileId + 1];
                _vertices[tileId + 1] = tmp;
            }
        }

        public void SetTile(int x, int y, uint id, ImageFlip flipped, bool flippedDiagonally) {
            if (flippedDiagonally) {
                id |= Tiled.TiledTile.FlippedDiagonallyFlag;
            }

            if (flipped.HasFlag(ImageFlip.Horizontal)) {
                id |= Tiled.TiledTile.FlippedHorizontallyFlag;
            }

            if (flipped.HasFlag(ImageFlip.Vertical)) {
                id |= Tiled.TiledTile.FlippedVerticallyFlag;
            }

            SetTile(x, y, id);
        }

        public void SetTiles(Rectangle area, uint gid) {
            for (int y = (int) area.Top; y < area.Bottom && y >= 0 && y < Rows; y++) {
                for (int x = (int) area.Left; x < area.Right && x >= 0 && x < Columns; x++) {
                    SetTile(x, y, gid);
                }
            }
        }

        public void SetTiles(Rectangle area, uint[] gids) {
            if (gids.Length != area.Area) throw new ArgumentException($"Inconsistent gids data size, expected {area.Area} gids, got {gids.Length}", "gids");

            for (int y = 0; y < area.Height && area.Top + y < Rows; y++) {
                for (int x = 0; x < area.Width && area.Left + x < Columns; x++) {
                    int id = y * (int) area.Width + x;
                    SetTile((int) area.Left + x, (int) area.Top + y, gids[id]);
                }
            }
        }

        public bool ExistsTile(int x, int y) {
            return !(x < 0 || x >= Columns || y < 0 || y >= Rows);
        }

        public void Refresh() {
            for (int i = 0; i < _tilesIds.Length; i++) {
                SetTile(i % Columns, i / Columns, _tilesIds[i]);
            }
        }

        public override void Dispose() {
            if (Texture != null) {
                Texture.Dispose();
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            _tileSetColumns = Texture.Width / (int) TileSize.Width;
            _tileSetRows = Texture.Height / (int) TileSize.Height;
        }

        #endregion Protected Methods
    }
}
