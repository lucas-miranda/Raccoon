using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

namespace Raccoon.Graphics {
    public class TileMap : Graphic {
        private const uint FlippedHorizontallyFlag = 0x80000000, FlippedVerticallyFlag = 0x40000000, FlippedDiagonallyFlag = 0x20000000;
        private readonly Regex GidRegex = new Regex(@"(\d+)");

        private int _tileSetRows, _tileSetColumns;
        private int[][] _data;

        public TileMap(string filename, Size tileSize) {
            Name = filename;
            TileSize = tileSize;
            _data = new int[0][];
            Load();
        }

        public Size TileSize { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Rectangle Clip { get; set; }

        internal Texture2D Texture { get; set; }

        public override void Update(int delta) {
        }

        public override void Render() {
            // TODO: draw tiles using vertices
            Rectangle clip = Clip.IsEmpty ? new Rectangle(0, 0, Columns - 1, Rows - 1) : Clip;
            for (int y = (int) clip.Top; y <= (int) clip.Bottom; y++) {
                for (int x = (int) clip.Left; x < _data[y].Length && x <= (int) clip.Right; x++) {
                    int gid = _data[y][x];
                    if (gid < 0) {
                        continue;
                    }

                    SpriteEffects flipped = SpriteEffects.None;
                    if ((gid & FlippedHorizontallyFlag) != 0) {
                        flipped |= SpriteEffects.FlipHorizontally;
                    }

                    if ((gid & FlippedVerticallyFlag) != 0) {
                        flipped |= SpriteEffects.FlipVertically;
                    }

                    gid &= (int) ~(FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag); // clear flags
                    
                    Game.Instance.Core.SpriteBatch.Draw(
                        Texture,
                        Position + new Vector2(x * TileSize.Width, y * TileSize.Height),
                        null,
                        new Microsoft.Xna.Framework.Rectangle((gid - (gid / _tileSetColumns) * _tileSetColumns) * (int) TileSize.Width, (gid / _tileSetColumns) * (int) TileSize.Height, (int) TileSize.Width, (int) TileSize.Height),
                        null,
                        0,
                        null,
                        Color,
                        flipped,
                        LayerDepth
                    );
                }
            }
        }

        public void SetData(int[][] data) {
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

        public void SetData(int[,] data) {
            int columns = data.GetLength(1);
            int[][] newData = new int[data.GetLength(0)][];
            for (int row = 0; row < newData.Length; row++) {
                newData[row] = new int[columns];
                for (int column = 0; column < columns; column++) {
                    newData[row][column] = data[row, column];
                }
            }

            _data = newData;
            Columns = columns;
            Rows = newData.Length;
        }

        public void SetData(string csv, int columns, int rows) {
            int[][] newData = new int[rows][];
            for (int row = 0; row < newData.Length; row++) {
                newData[row] = new int[columns];
            }

            int x = 0, y = 0;
            foreach (Match m in GidRegex.Matches(csv)) {
                newData[y][x] = int.Parse(m.Value) - 1;
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

        public void SetTile(int x, int y, int gid) {
            CheckBounds(x, y);
            _data[y][x] = gid;
        }

        public bool ExistsTile(int x, int y) {
            return y < _data.Length && x < _data[y].Length;
        }

        public override void Dispose() {
            if (Texture != null)
                Texture.Dispose();
        }

        private void CheckBounds(int x, int y) {
            if (_data.Length <= y) {
                int[][] currentData = _data;
                _data = new int[y + 1][];
                currentData.CopyTo(_data, 0);
                Rows = y + 1;
                for (int row = currentData.Length; row < Rows; row++) {
                    _data[row] = new int[0];
                }
            }

            if (_data[y].Length <= x) {
                int[] currentRowData = _data[y];
                _data[y] = new int[x + 1];
                currentRowData.CopyTo(_data[y], 0);
                Columns = x + 1;
                for (int column = currentRowData.Length; column < Columns; column++) {
                    _data[y][column] = 0;
                }
            }
        }

        internal override void Load() {
            if (Game.Instance.Core.SpriteBatch == null)
                return;

            Texture = Game.Instance.Core.Content.Load<Texture2D>(Name);
            Debug.Assert(Texture != null, $"Texture with name '{Name}' not found.");
            _tileSetColumns =  Texture.Width / (int) TileSize.Width;
            _tileSetRows =  Texture.Height / (int) TileSize.Height;
        }
    }
}
