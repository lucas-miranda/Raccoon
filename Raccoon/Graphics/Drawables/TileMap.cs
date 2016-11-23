using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

namespace Raccoon.Graphics {
    public class TileMap : Graphic {
        #region Private Static Members

        private static readonly Regex GidRegex = new Regex(@"(\d+)");

        #endregion Private Static Members

        #region Private Members

        private int _tileSetRows, _tileSetColumns;
        private uint[][] _data = new uint[0][];

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
        public Rectangle Clip { get; set; }
        public Texture Texture { get; set; }

        #endregion Internal properties

        #region Public Methods

        public override void Update(int delta) {
        }

        public override void Render() {
            // TODO: draw tiles using vertices
            Rectangle clip = Clip.IsEmpty ? new Rectangle(0, 0, Columns - 1, Rows - 1) : Clip;
            for (int y = (int) clip.Top; y <= (int) clip.Bottom; y++) {
                for (int x = (int) clip.Left; x < _data[y].Length && x <= (int) clip.Right; x++) {
                    uint gid = _data[y][x];
                    if (gid == 0) {
                        continue;
                    }

                    SpriteEffects flipped = SpriteEffects.None;
                    if ((gid & Tiled.TiledTile.FlippedHorizontallyFlag) != 0) {
                        flipped |= SpriteEffects.FlipHorizontally;
                    }

                    if ((gid & Tiled.TiledTile.FlippedVerticallyFlag) != 0) {
                        flipped |= SpriteEffects.FlipVertically;
                    }

                    int id = (int) (gid & ~(Tiled.TiledTile.FlippedHorizontallyFlag | Tiled.TiledTile.FlippedVerticallyFlag | Tiled.TiledTile.FlippedDiagonallyFlag)) - 1; // clear flags
                    Game.Instance.Core.SpriteBatch.Draw(
                        Texture.XNATexture,
                        Position + new Vector2(x * TileSize.Width, y * TileSize.Height),
                        null,
                        new Microsoft.Xna.Framework.Rectangle((id - (id / _tileSetColumns) * _tileSetColumns) * (int) TileSize.Width, (id / _tileSetColumns) * (int) TileSize.Height, (int) TileSize.Width, (int) TileSize.Height),
                        null,
                        0,
                        null,
                        FinalColor,
                        flipped,
                        LayerDepth
                    );
                }
            }
        }

        public void SetData(uint[][] data) {
            _data = data;
            int greaterRowSize = 0;
            for (int row = 0; row < _data.Length; row++) {
                if (greaterRowSize < _data[row].Length) {
                    greaterRowSize = _data[row].Length;
                }
            }

            Columns = greaterRowSize;
            Rows = data.Length;
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

            _data = newData;
            Columns = columns;
            Rows = newData.Length;
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

            _data = newData;
            Columns = columns;
            Rows = rows;
        }

        public void SetTile(int x, int y, uint gid) {
            StretchToFit(x, y);
            _data[y][x] = gid;
        }

        public void SetTile(int x, int y, uint id, ImageFlip flipped, bool flippedDiagonally) {
            SetTile(x, y, id | (flipped.HasFlag(ImageFlip.Horizontal) ? Tiled.TiledTile.FlippedHorizontallyFlag : 0) | (flipped.HasFlag(ImageFlip.Vertical) ? Tiled.TiledTile.FlippedVerticallyFlag : 0) | (flippedDiagonally ? Tiled.TiledTile.FlippedDiagonallyFlag : 0));
        }

        public bool ExistsTile(int x, int y) {
            return y >= 0 && y < _data.Length && x >= 0 && x < _data[y].Length;
        }

        public override void Dispose() {
            if (Texture != null)
                Texture.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            _tileSetColumns = Texture.Width / (int) TileSize.Width;
            _tileSetRows = Texture.Height / (int) TileSize.Height;
        }

        #endregion Protected Methods

        #region Private Methods

        private void StretchToFit(int x, int y) {
            if (_data.Length <= y) {
                uint[][] currentData = _data;
                _data = new uint[y + 1][];
                currentData.CopyTo(_data, 0);
                Rows = y + 1;
                for (int row = currentData.Length; row < Rows; row++) {
                    _data[row] = new uint[0];
                }
            }

            if (_data[y].Length <= x) {
                uint[] currentRowData = _data[y];
                _data[y] = new uint[x + 1];
                currentRowData.CopyTo(_data[y], 0);
                Columns = x + 1;
                for (int column = currentRowData.Length; column < Columns; column++) {
                    _data[y][column] = 0;
                }
            }
        }

        private void ShrinkTo(int columns, int rows) {
            if (columns > Columns || rows > Rows) {
                InflateTo(columns, rows);
                return;
            }

            uint[][] newData = new uint[rows][];
            for (int row = 0; row < rows; row++) {
                if (_data[row].Length > columns) {
                    newData[row] = new uint[columns];
                    _data[row].CopyTo(newData[row], rows);
                } else {
                    _data[row] = newData[row];
                }
            }
        }

        private void InflateTo(int columns, int rows) {
            if (columns < Columns || rows < Rows) {
                ShrinkTo(columns, rows);
                return;
            }

            Rows = rows;
            uint[][] currentData = _data;
            _data = new uint[Rows][];
            currentData.CopyTo(_data, 0);
            for (int row = currentData.Length; row < Rows; row++) {
                _data[row] = new uint[0];
            }

            Columns = columns;
            for (int row = 0; row < Rows; row++) {
                uint[] currentRowData = _data[row];
                _data[row] = new uint[Columns];
                currentRowData.CopyTo(_data[row], 0);
                for (int column = currentRowData.Length; column < Columns; column++) {
                    _data[row][column] = 0;
                }
            }
        }

        #endregion Private Methods
    }
}
