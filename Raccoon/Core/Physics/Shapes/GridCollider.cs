using System;

using Raccoon.Graphics;
using Raccoon.Graphics.Primitives;

namespace Raccoon.Components {
    public class GridCollider : Collider {
        #region Private Members

        private bool[,] _data;
        private bool _graphicNeedUpdate;
        private Size _tileSize;

        #endregion Private Members

        #region Constructors

        public GridCollider(Size tileSize, int columns, int rows, params string[] tags) : base(tags) {
            Initialize(tileSize, columns, rows);
        }

        public GridCollider(Size tileSize, int columns, int rows, params Enum[] tags) : base(tags) {
            Initialize(tileSize, columns, rows);
        }

        #endregion Constructors

        #region Public Properties

        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public Size TileSize {
            get {
                return _tileSize;
            }

            set {
                _tileSize = value;
                Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);
#if DEBUG
                _graphicNeedUpdate = true;
#endif
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender() {
            Size graphicSize = new Size((float) Math.Floor(TileSize.Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1), (float) Math.Floor(TileSize.Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1));
            if (_graphicNeedUpdate || Graphic.Size != graphicSize) {
                (Graphic as RectangleShape).Size = graphicSize;
                _graphicNeedUpdate = false;
            }

            for (int y = 0; y < _data.GetLength(0); y++) {
                for (int x = 0; x < _data.GetLength(1); x++) {
                    if (!_data[y, x]) {
                        continue;
                    }

                    Graphic.Color = Color;
                    Graphic.Render(Graphic.Surface.Transform(Position + new Vector2(x, y) * TileSize, Game.Instance.Core.MainSurface));
                }
            }
        }

        public void Setup(int columns, int rows) {
            if (columns <= 0) {
                throw new ArgumentException("Value should be greater than 0.", "columns");
            }

            if (rows <= 0) {
                throw new ArgumentException("Value should be greater than 0.", "rows");
            }

            Columns = columns;
            Rows = rows;
            _data = new bool[Rows, Columns];
            Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);
        }

        public void SetData(bool[,] data) {
            _data = data;
        }

        public bool IsCollidable(int x, int y) {
            return !(x < 0 || x >= Columns || y < 0 || y >= Rows) && _data[y, x];
        }

        public void SetCollidable(int x, int y, bool collidable = true) {
            if (x < 0 || x >= Columns) {
                throw new ArgumentOutOfRangeException("x", x, $"Value should be between {0} and {Columns - 1}.");
            }

            if (y < 0 || y >= Rows) {
                throw new ArgumentOutOfRangeException("y", y, $"Value should be between {0} and {Rows - 1}.");
            }

            _data[y, x] = collidable;
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(Size tileSize, int columns, int rows) {
            Setup(columns, rows);
            TileSize = tileSize;
            Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);

#if DEBUG
            Graphic = new RectangleShape(TileSize.Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1, TileSize.Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1, Color, false) {
                Surface = Game.Instance.Core.DebugSurface
            };

            _graphicNeedUpdate = false;
#endif
        }

        #endregion Private Methods
    }
}
