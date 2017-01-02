using System;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class TileMap : Graphic {
        #region Private Static Members

        private static readonly Regex GidRegex = new Regex(@"(\d+)");

        #endregion Private Static Members

        #region Private Members

        private int _tileSetRows, _tileSetColumns, _triangleCount;
        private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[0];
        private Vector2 _texSpriteCount;
        private Microsoft.Xna.Framework.Matrix _lastWorldMatrix;

        #endregion Private Members

        #region Constructors

        public TileMap(string filename, Size tileSize) {
            TileSize = tileSize;
            Texture = new Texture(filename);
            Load();
        }

        #endregion Constructors

        #region Public Properties

        public Size TileSize { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Texture Texture { get; private set; }
        public new float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public new float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }

        public new Vector2 Position {
            get {
                return base.Position;
            }

            set {
                Microsoft.Xna.Framework.Vector3 displacement = new Microsoft.Xna.Framework.Vector3(value.X - X, value.Y - Y, 0);
                base.Position = value;

                for (int i = 0; i < _vertices.Length; i++) {
                    _vertices[i].Position += displacement;
                }
            }
        }

        #endregion Public Properties

        #region Public Methods
        
        public override void Render(Vector2 position, float rotation) {
            Game.Instance.Core.BasicEffect.TextureEnabled = true;
            Game.Instance.Core.BasicEffect.Texture = Texture.XNATexture;
            _lastWorldMatrix = Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(new Microsoft.Xna.Framework.Vector3(position.X, position.Y, 0f)) * worldMatrix;
            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _triangleCount);
            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.Identity;
            Game.Instance.Core.BasicEffect.Texture = null;
            Game.Instance.Core.BasicEffect.TextureEnabled = false;
        }

        public override void DebugRender() {
            Microsoft.Xna.Framework.Matrix worldMatrix = Game.Instance.Core.BasicEffect.World;
            Game.Instance.Core.BasicEffect.World = _lastWorldMatrix;

            for (int row = 0; row <= Rows; row++) {
                Debug.DrawLine(Position + new Vector2(0, row * TileSize.Height), Position + new Vector2(Columns * TileSize.Width, row * TileSize.Height), row == 0 || row == Rows ? new Color(0xccccccff) : new Color(0x4c4c4cff));
            }

            for (int column = 0; column <= Columns; column++) {
                Debug.DrawLine(Position + new Vector2(column * TileSize.Width, 0), Position + new Vector2(column * TileSize.Width, Rows * TileSize.Height), column == 0 || column == Columns ? new Color(0xccccccff) : new Color(0x4c4c4cff));
            }

            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.Identity;
        }

        public void Setup(int columns, int rows) {
            Columns = columns;
            Rows = rows;
            Size = new Size(Columns * TileSize.Width, Rows * TileSize.Height);

            if (columns * rows * 6 != _vertices.Length) {
                VertexPositionColorTexture[] newVertices = new VertexPositionColorTexture[Columns * Rows * 6];
                if (newVertices.Length > _vertices.Length) {
                    _vertices.CopyTo(newVertices, 0);
                } else {
                    for (int i = 0; i < newVertices.Length; i++) {
                        newVertices[i] = _vertices[i];
                    }
                }

                _vertices = newVertices;
                _triangleCount = Columns * Rows * 2;
            }
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
            int columns = data.GetLength(1);
            uint[][] newData = new uint[data.GetLength(0)][];
            for (int row = 0; row < newData.Length; row++) {
                newData[row] = new uint[columns];
                for (int column = 0; column < columns; column++) {
                    newData[row][column] = data[row, column];
                }
            }

            SetData(newData);
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

        public void SetTile(int x, int y, uint gid) {
            if (!ExistsTile(x, y)) throw new ArgumentException($"x or y out of bounds [0 0 {Columns} {Rows}]");

            if (gid == 0) {
                return;
            }

            Microsoft.Xna.Framework.Vector3 displacement = new Microsoft.Xna.Framework.Vector3(X, Y, 0);

            bool isAntiDiagonally = (gid & Tiled.TiledTile.FlippedDiagonallyFlag) != 0;

            ImageFlip flip = ImageFlip.None;
            if ((gid & Tiled.TiledTile.FlippedHorizontallyFlag) != 0) {
                flip |= ImageFlip.Horizontal;
            }

            if ((gid & Tiled.TiledTile.FlippedVerticallyFlag) != 0) {
                flip |= ImageFlip.Vertical;
            }

            int id = (int) (gid & ~(Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag)) - 1; // clear flags
            int tileId = (y * Columns + x) * 6, texRow = id / (int) _texSpriteCount.X;
            Vector2 texTopLeft = new Vector2(id - texRow * (int) _texSpriteCount.X, texRow);

            _vertices[tileId] = new VertexPositionColorTexture(displacement + new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, y * TileSize.Height, LayerDepth), Color, texTopLeft / _texSpriteCount);
            _vertices[tileId + 1] = new VertexPositionColorTexture(displacement + new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, y * TileSize.Height, LayerDepth), Color, (texTopLeft + Vector2.Right) / _texSpriteCount);
            _vertices[tileId + 2] = new VertexPositionColorTexture(displacement + new Microsoft.Xna.Framework.Vector3((x + 1) * TileSize.Width, (y + 1) * TileSize.Height, LayerDepth), Color, (texTopLeft + Vector2.DownRight) / _texSpriteCount);

            _vertices[tileId + 3] = _vertices[tileId + 2];
            _vertices[tileId + 4] = new VertexPositionColorTexture(displacement + new Microsoft.Xna.Framework.Vector3(x * TileSize.Width, (y + 1) * TileSize.Height, LayerDepth), Color, (texTopLeft + Vector2.Down) / _texSpriteCount);
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

        public bool ExistsTile(int x, int y) {
            return !(x < 0 || x >= Columns || y < 0 || y >= Rows);
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
            _texSpriteCount = new Vector2((int) (Texture.Width / TileSize.Width), (int) (Texture.Height / TileSize.Height));
        }

        #endregion Protected Methods
    }
}
