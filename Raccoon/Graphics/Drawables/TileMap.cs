using System.Text.RegularExpressions;

using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public class TileMap : Graphic {
        #region Private Static Members

        private static readonly Regex GidRegex = new Regex(@"(\d+)");

        #endregion Private Static Members

        #region Private Members

        private int _tileSetRows, _tileSetColumns, _triangleCount;
        private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[0];

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
        public uint[] Data { get; private set; } = new uint[0];
        public Rectangle TileBounds { get { return new Rectangle(0, 0, Columns, Rows); } }

#if DEBUG
        public Grid Grid { get; private set; }
#endif
        #endregion Public Properties

        #region Public Methods

        public override void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
            if (_vertices.Length == 0) {
                return;
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X * scale.X, Scale.Y * scale.Y, 1f) 
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0f) 
                * Microsoft.Xna.Framework.Matrix.CreateRotationZ(Math.ToRadians(Rotation + rotation))
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f) 
                * Renderer.World;

            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.DiffuseColor = color * Color;
            bs.Alpha = Opacity;

            // texture
            bs.TextureEnabled = true;
            bs.Texture = Texture;
            
            foreach (var pass in bs) {
                Game.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _triangleCount);
            }

            bs.ResetParameters();
        }

        public override void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
#if DEBUG
            if (Grid == null) {
                return;
            }

            Grid.Render(Position + position, Rotation + rotation, Scale * scale, Flipped ^ flip, Color.White, Scroll + scroll);
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
                        newTilesIds[y * Columns + x] = Data[y * oldColumns + x];
                    } else {
                        newVertices[newTileId] = newVertices[newTileId + 1] = newVertices[newTileId + 2] = newVertices[newTileId + 3] = newVertices[newTileId + 4] = newVertices[newTileId + 5] = new VertexPositionColorTexture(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White, Vector2.Zero);
                        newTilesIds[y * Columns + x] = 0;
                    }
                }
            }

            _vertices = newVertices;
            _triangleCount = Columns * Rows * 2;
            Data = newTilesIds;
#if DEBUG
            if (Grid == null) {
                Grid = new Grid(TileSize) {
                    BorderColor = Color.White
                };
            }

            Grid.Setup(Columns, Rows);
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
            if (!ExistsTileAt(x, y)) {
                throw new System.ArgumentException($"x ({x}) or y ({y}) out of bounds [0 0 {Columns} {Rows}]");
            }

            return Data[y * Columns + x];
        }

        public uint[] GetTiles(Rectangle area) {
            area = Math.Clamp(area, new Rectangle(0, 0, Columns, Rows));

            int i = 0;
            uint[] tiles = new uint[(int) area.Area];
            for (int y = (int) area.Top; y < area.Bottom; y++) {
                for (int x = (int) area.Left; x < area.Right; x++) {
                    tiles[i] = GetTile(x, y);
                    i++;
                }
            }

            return tiles;
        }

        public void SetTile(int x, int y, uint gid) {
            if (!ExistsTileAt(x, y)) {
                throw new System.ArgumentException($"x or y out of bounds [0 0 {Columns} {Rows}]");
            }

            int tileId = (y * Columns + x) * 6;
            Data[y * Columns + x] = gid;

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
            area = Math.Clamp(area, new Rectangle(0, 0, Columns, Rows));

            for (int y = (int) area.Top; y < area.Bottom; y++) {
                for (int x = (int) area.Left; x < area.Right; x++) {
                    SetTile(x, y, gid);
                }
            }
        }

        public void SetTiles(Rectangle area, uint[] gids) {
            area = Math.Clamp(area, new Rectangle(0, 0, Columns, Rows));

            for (int y = 0; y < area.Height; y++) {
                for (int x = 0; x < area.Width; x++) {
                    int id = y * (int) area.Width + x;
                    SetTile((int) area.Left + x, (int) area.Top + y, gids[id]);
                }
            }
        }

        public bool ExistsTileAt(int x, int y) {
            return !(x < 0 || x >= Columns || y < 0 || y >= Rows);
        }

        public void Refresh() {
            for (int i = 0; i < Data.Length; i++) {
                SetTile(i % Columns, i / Columns, Data[i]);
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
