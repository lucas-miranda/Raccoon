using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public class TileMap : PrimitiveGraphic {
        #region Public Members

        public const uint TileFlippedHorizontallyFlag = 0x80000000,
                          TileFlippedVerticallyFlag = 0x40000000,
                          TileFlippedDiagonallyFlag = 0x20000000,
                          TileFlippedAllFlags = TileFlippedHorizontallyFlag | TileFlippedVerticallyFlag | TileFlippedDiagonallyFlag;

        #endregion Public Members

        #region Private Static Members

        private static readonly Regex GidRegex = new Regex(@"(\d+)");

        #endregion Private Static Members

        #region Private Members

        private Texture _texture;
        private int _tileSetRows, _tileSetColumns, _triangleCount;
        private VertexPositionColorTexture[] _vertices;
        private int[] _indices;

        #endregion Private Members

        #region Constructors

        public TileMap() : base() {
        }

        public TileMap(Size tileSize) : base() {
            TileSize = tileSize;
        }

        public TileMap(Texture texture, Size tileSize) : base() {
            TileSize = tileSize;
            Texture = texture;
            Load();
        }

        public TileMap(string filename, Size tileSize) : this(new Texture(filename), tileSize) {
        }

        #endregion Constructors

        #region Public Properties

        public Size TileSize { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public uint[] Data { get; private set; } = new uint[0];
        public Rectangle TileBounds { get { return new Rectangle(0, 0, Columns, Rows); } }

        public Texture Texture {
            get {
                return _texture;
            }

            set {
                _texture = value;

                if (_texture != null) {
                    Load();
                    NeedsReload = true;
                }
            }
        }

        public int TileSetColumns { get { return _tileSetColumns; } }
        public int TileSetRows { get { return _tileSetRows; } }

#if DEBUG
        public Grid Grid { get; private set; }
#endif

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
#if DEBUG
            if (Grid == null) {
                return;
            }

            Grid.Render(Position + position - Origin, Rotation + rotation, Scale * scale, Flipped ^ flip, Color.White, Scroll + scroll);
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

            // prepare vertex buffer
            VertexPositionColorTexture[] previousVertices = null,
                                         newVertices = new VertexPositionColorTexture[columns * rows * 4];

            if (_vertices != null) {
                previousVertices = _vertices;
            }

            // prepare index buffer
            int[] newIndices = new int[columns * rows * 6];

            uint[] newTilesIds = new uint[Columns * Rows];
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    int newTileId = (y * Columns) + x,
                        newTileVertexStartId = newTileId * 4,
                        newTileIndexStartId = newTileId * 6;

                    if (y < oldRows && x < oldColumns) {
                        // copy from old data
                        int oldTileId = (y * oldColumns) + x,
                            oldTileVertexStartId = oldTileId * 4//,
                            /*oldTileIndexStartId = oldTileId * 6*/;

                        System.Array.Copy(previousVertices, oldTileVertexStartId, newVertices, newTileVertexStartId, 4);

                        newTilesIds[newTileId] = Data[oldTileId];
                    } else {
                        newVertices[newTileVertexStartId] =
                            newVertices[newTileVertexStartId + 1] =
                            newVertices[newTileVertexStartId + 2] =
                            newVertices[newTileVertexStartId + 3] = new VertexPositionColorTexture(Microsoft.Xna.Framework.Vector3.Zero, Color.White, Vector2.Zero);

                        newTilesIds[newTileId] = 0;
                    }

                    newIndices[newTileIndexStartId] = newTileVertexStartId;
                    newIndices[newTileIndexStartId + 1] = newTileVertexStartId + 1;
                    newIndices[newTileIndexStartId + 2] = newTileVertexStartId + 2;
                    newIndices[newTileIndexStartId + 3] = newTileVertexStartId + 2;
                    newIndices[newTileIndexStartId + 4] = newTileVertexStartId + 1;
                    newIndices[newTileIndexStartId + 5] = newTileVertexStartId + 3;
                }
            }

            _vertices = newVertices;
            _indices = newIndices;
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

            for (int y = 0; y < data.Length; y++) {
                for (int x = 0; x < data[y].Length; x++) {
                    SetTile(x, y, data[y][x]);
                }
            }
        }

        public void SetData(uint[,] data) {
            Setup(data.GetLength(1), data.GetLength(0));

            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    SetTile(x, y, data[y, x]);
                }
            }
        }

        public void SetData(uint[] data, int columns, int rows) {
            Setup(columns, rows);

            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    SetTile(x, y, data[y * columns + x]);
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
            if (Texture == null) {
                throw new System.InvalidOperationException($"Can't set a tile. Texture is not set.");
            } else if (_tileSetColumns == 0) {
                throw new System.InvalidOperationException($"Can't set a tile. Tileset columns is zero.");
            } else if (_tileSetRows == 0) {
                throw new System.InvalidOperationException($"Can't set a tile. Tileset rows is zero.");
            }

            if (!ExistsTileAt(x, y)) {
                throw new System.ArgumentException($"x or y out of bounds [0 0 {Columns} {Rows}]");
            }

            int tileId = (y * Columns) + x,
                vertexTileId = tileId * 4;

            Data[tileId] = gid;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];

            // empty tile
            if (gid == 0) {
                vertices.CopyTo(_vertices, vertexTileId);
                return;
            }

            bool isAntiDiagonally = (gid & TileFlippedDiagonallyFlag) != 0;

            ImageFlip flip = ImageFlip.None;
            if ((gid & TileFlippedHorizontallyFlag) != 0) {
                flip |= ImageFlip.Horizontal;
            }

            if ((gid & TileFlippedVerticallyFlag) != 0) {
                flip |= ImageFlip.Vertical;
            }

            int id = (int) (gid & ~TileFlippedAllFlags) - 1; // clear flags
            float texLeft = (id % _tileSetColumns) * TileSize.Width,
                  texTop = (id / _tileSetColumns) * TileSize.Height;

            //
            // Vertices layout:
            //
            //  1--3/5--7
            //  |\  | \ |
            //  | \ |  \|
            //  0--2/4--6
            //

            vertices[0] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, (y + 1) * TileSize.Height, 0f),
                Color.White,
                new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, (texTop + TileSize.Height) / Texture.Height)
            );

            vertices[1] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, y * TileSize.Height, 0f),
                Color.White,
                new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, texTop / Texture.Height)
            );

            vertices[2] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, (y + 1) * TileSize.Height, 0f),
                Color.White,
                new Microsoft.Xna.Framework.Vector2((texLeft + TileSize.Width) / Texture.Width, (texTop + TileSize.Height) / Texture.Height)
            );

            vertices[3] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, y * TileSize.Height, 0f),
                Color.White,
                new Microsoft.Xna.Framework.Vector2((texLeft + TileSize.Width) / Texture.Width, texTop / Texture.Height)
            );

            //
            // Vertices layout:
            //
            //  1--3/5--7
            //  |\  | \ |
            //  | \ |  \|
            //  0--2/4--6
            //

            if (isAntiDiagonally) {
                Microsoft.Xna.Framework.Vector2 texTmp;

                texTmp = vertices[0].TextureCoordinate;
                vertices[0].TextureCoordinate = vertices[3].TextureCoordinate;
                vertices[3].TextureCoordinate = texTmp;
            }

            if (flip.HasFlag(ImageFlip.Horizontal)) {
                Microsoft.Xna.Framework.Vector2 texTmp;

                texTmp = vertices[0].TextureCoordinate;
                vertices[0].TextureCoordinate = vertices[2].TextureCoordinate;
                vertices[2].TextureCoordinate = texTmp;

                texTmp = vertices[1].TextureCoordinate;
                vertices[1].TextureCoordinate = vertices[3].TextureCoordinate;
                vertices[3].TextureCoordinate = texTmp;
            }

            if (flip.HasFlag(ImageFlip.Vertical)) {
                Microsoft.Xna.Framework.Vector2 texTmp;

                texTmp = vertices[0].TextureCoordinate;
                vertices[0].TextureCoordinate = vertices[1].TextureCoordinate;
                vertices[1].TextureCoordinate = texTmp;

                texTmp = vertices[2].TextureCoordinate;
                vertices[2].TextureCoordinate = vertices[3].TextureCoordinate;
                vertices[3].TextureCoordinate = texTmp;
            }

            vertices.CopyTo(_vertices, vertexTileId);
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
            if (IsDisposed) {
                return;
            }

            _texture = null;

            if (Grid != null) {
                Grid.Dispose();
                Grid = null;
            }

            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            if ((int) TileSize.Width == 0 || (int) TileSize.Height == 0) {
                throw new System.InvalidOperationException("TileMap needs a TileSize, with width and height, greater than zero.");
            }

            if (Texture.Width == 0 || Texture.Height == 0) {
                throw new System.InvalidOperationException("Invalid texture size.");
            }

            _tileSetColumns = Texture.Width / (int) TileSize.Width;
            _tileSetRows = Texture.Height / (int) TileSize.Height;
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (_vertices == null
             || _vertices.Length == 0
             || _indices == null
             || _indices.Length == 0
             || Columns * Rows == 0
             || Texture == null
            ) {
                return;
            }

            Renderer.DrawVertices(
                texture:            Texture,
                vertexData:         _vertices,
                minVertexIndex:     0,
                verticesLength:     _vertices.Length,
                indices:            _indices,
                minIndex:           0,
                primitivesCount:    _triangleCount,
                isHollow:           false,
                position:           position,
                rotation:           rotation,
                scale:              scale,
                color:              new Color(color, (color.A / 255f) * Opacity),
                origin:             origin,
                scroll:             scroll,
                shader:             shader,
                shaderParameters:   shaderParameters,
                layerDepth:         layerDepth
            );
        }

        #endregion Protected Methods
    }
}
