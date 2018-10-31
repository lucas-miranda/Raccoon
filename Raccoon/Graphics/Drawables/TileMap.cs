using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public class TileMap : PrimitiveGraphic {
        #region Private Static Members

        private static readonly Regex GidRegex = new Regex(@"(\d+)");

        #endregion Private Static Members

        #region Private Members

        private int _tileSetRows, _tileSetColumns, _triangleCount;
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;

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

            VertexPositionColorTexture[] previousVertices = null,
                                         newVertices = new VertexPositionColorTexture[Columns * Rows * 4];

            // prepare vertex buffer
            if (_vertexBuffer == null || newVertices.Length > _vertexBuffer.VertexCount) {
                if (_vertexBuffer != null) {
                    _vertexBuffer.GetData(previousVertices, 0, _vertexBuffer.VertexCount);
                }

                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, newVertices.Length, BufferUsage.WriteOnly);
            } else {
                _vertexBuffer.GetData(previousVertices, 0, _vertexBuffer.VertexCount);
            }

            int[] previousIndices = null,
                  newIndices = new int[Columns * Rows * 6];

            // prepare index buffer
            if (_indexBuffer == null || newIndices.Length > _indexBuffer.IndexCount) {
                if (_indexBuffer != null) {
                    _indexBuffer.GetData(previousIndices, 0, _indexBuffer.IndexCount);
                }

                _indexBuffer = new DynamicIndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, newIndices.Length, BufferUsage.WriteOnly);
            } else {
                _indexBuffer.GetData(previousIndices, 0, _indexBuffer.IndexCount);
            }

            uint[] newTilesIds = new uint[Columns * Rows];
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    int newTileId = (y * Columns) + x,
                        newTileVertexStartId = newTileId * 4,
                        newTileIndexStartId = newTileId * 6;

                    if (y < oldRows && x < oldColumns) {
                        // copy from old data
                        int oldTileId = (y * oldColumns) + x,
                            oldTileVertexStartId = oldTileId * 4,
                            oldTileIndexStartId = oldTileId * 6;

                        System.Array.Copy(previousVertices, oldTileVertexStartId, newVertices, newTileVertexStartId, 4);
                        System.Array.Copy(previousIndices, oldTileIndexStartId, newIndices, newTileIndexStartId, 6);

                        newTilesIds[newTileId] = Data[oldTileId];
                    } else {
                        newVertices[newTileVertexStartId] = 
                            newVertices[newTileVertexStartId + 1] = 
                            newVertices[newTileVertexStartId + 2] = 
                            newVertices[newTileVertexStartId + 3] = new VertexPositionColorTexture(Microsoft.Xna.Framework.Vector3.Zero, Color.White, Vector2.Zero);

                        newIndices[newTileIndexStartId] = newTileVertexStartId;
                        newIndices[newTileIndexStartId + 1] = newTileVertexStartId + 1;
                        newIndices[newTileIndexStartId + 2] = newTileVertexStartId + 2;
                        newIndices[newTileIndexStartId + 3] = newTileVertexStartId + 2;
                        newIndices[newTileIndexStartId + 4] = newTileVertexStartId + 1;
                        newIndices[newTileIndexStartId + 5] = newTileVertexStartId + 3;

                        newTilesIds[newTileId] = 0;
                    }
                }
            }

            _vertexBuffer.SetData(newVertices, 0, newVertices.Length, SetDataOptions.Discard);
            _indexBuffer.SetData(newIndices, 0, newIndices.Length, SetDataOptions.Discard);

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

            int tileId = (y * Columns) + x,
                vertexTileId = tileId * 4;

            Data[tileId] = gid;

            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];

            // empty tile
            if (gid == 0) {
                vertices[0] = vertices[1] = vertices[2] = vertices[3] = new VertexPositionColorTexture(Microsoft.Xna.Framework.Vector3.Zero, Color.White, Vector2.Zero);
                _vertexBuffer.SetData(vertexTileId * _vertexBuffer.VertexDeclaration.VertexStride, vertices, 0, 4, _vertexBuffer.VertexDeclaration.VertexStride, SetDataOptions.None);
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

            //  
            // Vertices layout:
            //
            //  1--3/5--7
            //  |\  | \ |
            //  | \ |  \|
            //  0--2/4--6
            //

            vertices[0] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, (y + 1) * TileSize.Height, 0), 
                Color.White, 
                new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, (texTop + TileSize.Height) / Texture.Height)
            );

            vertices[1] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, y * TileSize.Height, 0), 
                Color.White, 
                new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, texTop / Texture.Height)
            );

            vertices[2] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, (y + 1) * TileSize.Height, 0), 
                Color.White, 
                new Microsoft.Xna.Framework.Vector2((texLeft + TileSize.Width) / Texture.Width, (texTop + TileSize.Height) / Texture.Height)
            );

            vertices[3] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, y * TileSize.Height, 0), 
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

            _vertexBuffer.SetData(vertexTileId * _vertexBuffer.VertexDeclaration.VertexStride, vertices, 0, 4, _vertexBuffer.VertexDeclaration.VertexStride, SetDataOptions.None);

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

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
            if (_vertexBuffer.VertexCount == 0) {
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

            GraphicsDevice device = Game.Instance.GraphicsDevice;
            
            foreach (var pass in bs) {
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods
    }
}
