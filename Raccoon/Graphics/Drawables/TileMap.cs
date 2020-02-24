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
        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private float _lastAppliedLayerDepth;

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

            if (_vertexBuffer != null) {
                previousVertices = new VertexPositionColorTexture[_vertexBuffer.VertexCount];
                _vertexBuffer.GetData(previousVertices, 0, _vertexBuffer.VertexCount);
            }

            if (_vertexBuffer == null || newVertices.Length > _vertexBuffer.VertexCount) {
                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, newVertices.Length, BufferUsage.None);
            }

            // prepare index buffer
            int[] previousIndices = null,
                  newIndices = new int[columns * rows * 6];

            if (_indexBuffer != null)  {
                previousIndices = new int[_indexBuffer.IndexCount];
                _indexBuffer.GetData(previousIndices, 0, _indexBuffer.IndexCount);
            }

            if (_indexBuffer == null || newIndices.Length > _indexBuffer.IndexCount) {
                _indexBuffer = new DynamicIndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, newIndices.Length, BufferUsage.None);
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

            if (newVertices.Length != 0) {
                _vertexBuffer.SetData(newVertices, 0, newVertices.Length, SetDataOptions.Discard);
            }

            if (newIndices.Length != 0) {
                _indexBuffer.SetData(newIndices, 0, newIndices.Length, SetDataOptions.Discard);
            }

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
                //System.Array.Clear(_vertices, vertexTileId * 4, 4);
                //vertices[0] = vertices[1] = vertices[2] = vertices[3] = new VertexPositionColorTexture(Microsoft.Xna.Framework.Vector3.Zero, Color.White, Vector2.Zero);
                _vertexBuffer.SetData(vertexTileId * _vertexBuffer.VertexDeclaration.VertexStride, vertices, 0, 4, _vertexBuffer.VertexDeclaration.VertexStride, SetDataOptions.None);
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
                new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, (y + 1) * TileSize.Height, _lastAppliedLayerDepth),
                Color.White,
                new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, (texTop + TileSize.Height) / Texture.Height)
            );

            vertices[1] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, y * TileSize.Height, _lastAppliedLayerDepth),
                Color.White,
                new Microsoft.Xna.Framework.Vector2(texLeft / Texture.Width, texTop / Texture.Height)
            );

            vertices[2] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, (y + 1) * TileSize.Height, _lastAppliedLayerDepth),
                Color.White,
                new Microsoft.Xna.Framework.Vector2((texLeft + TileSize.Width) / Texture.Width, (texTop + TileSize.Height) / Texture.Height)
            );

            vertices[3] = new VertexPositionColorTexture(
                new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, y * TileSize.Height, _lastAppliedLayerDepth),
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
            if (IsDisposed) {
                return;
            }

            _texture = null;

            if (Grid != null) {
                Grid.Dispose();
                Grid = null;
            }

            if (_vertexBuffer != null) {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            if (_indexBuffer != null) {
                _indexBuffer.Dispose();
                _indexBuffer = null;
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
            if (_vertexBuffer == null || Columns * Rows == 0 || Texture == null) {
                return;
            }

            // only update vertices layer depth if parameter value is differente from last applied value (to avoid redundancy calls)
            if (layerDepth != _lastAppliedLayerDepth) {
                UpdateVerticesLayerDepth(layerDepth);
            }

            /*
               Note about rendering here instead using Renderer to do the job:

                 I decided to not use Renderer to draw this, since *almost* always the
               texture used to store tiles will be different from atlases used to other
               other things. The texture swapping at SpriteBatch will be the same as
               drawing here and, maybe, drawing here is even better, since we don't need
               to iterate through all tiles and pack them into SpriteBatchItem.
            */

            BasicShader bs;

            if (Shader != null && Shader is BasicShader) {
                bs = (BasicShader) Shader;
            } else {
                bs = Game.Instance.BasicShader;
            }

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X * scale.X, Scale.Y * scale.Y, 1f)
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(-(Origin.X + origin.X), -(Origin.Y + origin.Y), 0f)
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

            // depth write
            bs.DepthWriteEnabled = true;

            shaderParameters?.ApplyParameters(shader);

            GraphicsDevice device = Game.Instance.GraphicsDevice;

            // we need to manually update every GraphicsDevice states here
            device.BlendState = Renderer.Batch.BlendState;
            device.SamplerStates[0] = Renderer.Batch.SamplerState;
            device.DepthStencilState = Renderer.Batch.DepthStencilState;
            device.RasterizerState = Renderer.Batch.RasterizerState;

            foreach (object pass in bs) {
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0, _triangleCount);
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods

        #region Private Methods

        private void UpdateVerticesLayerDepth(float layerDepth) {
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[_vertexBuffer.VertexCount];
            _vertexBuffer.GetData(vertices, 0, _vertexBuffer.VertexCount);

            for (int i = 0; i < vertices.Length; i++) {
                ref VertexPositionColorTexture vertex = ref vertices[i];
                vertex.Position.Z = layerDepth;
            }

            _vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.None);

            _lastAppliedLayerDepth = layerDepth;
        }

        #endregion Private Methods
    }
}
