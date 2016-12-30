using System;

using Raccoon.Graphics;

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
                (Graphic as Graphics.Primitives.Rectangle).Size = graphicSize;
                _graphicNeedUpdate = false;
            }


            Scene scene = Game.Instance.Scene;
            Rectangle clip = scene == null ? new Rectangle(0, 0, Rows, Columns) : new Rectangle(Math.Max(0, (int) Math.Floor(scene.Camera.X / TileSize.Width)), Math.Max(0, (int) Math.Floor(scene.Camera.Y / TileSize.Height)), Math.Min(Columns - 1, (int) Math.Floor(scene.Camera.Width / TileSize.Width)), Math.Min(Rows - 1, (int) Math.Floor(scene.Camera.Height / TileSize.Height)));
            for (int y = (int) clip.Top; y < _data.GetLength(0) && y <= (int) clip.Bottom; y++) {
                for (int x = (int) clip.Left; x < _data.GetLength(1) && x <= (int) clip.Right; x++) {
                    if (!_data[y, x])
                        continue;

                    Graphic.Position = Position * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + new Vector2(x * TileSize.Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, y * TileSize.Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom);
                    Graphic.Color = Color;
                    Graphic.Layer = Entity.Layer + (_data[y, x] ? 2 : 1);
                    Graphic.Render();
                }
            }
        }

        public void SetData(bool[,] data) {
            _data = data;
        }

        public bool IsCollidable(int x, int y) {
            return !(x < 0 || x >= Columns || y < 0 || y >= Rows) && _data[y, x];
        }

        public void SetCollidable(int x, int y, bool collidable = true) {
            if (x < 0 || x >= Columns || y < 0 || y >= Rows)
                return;

            _data[y, x] = collidable;
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(Size tileSize, int columns, int rows) {
            TileSize = tileSize;
            Columns = columns;
            Rows = rows;
            Size = new Size(TileSize.Width * Columns, TileSize.Height * Rows);
            _data = new bool[Rows, Columns];
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    _data[y, x] = false;
                }
            }

#if DEBUG
            Graphic = new Graphics.Primitives.Rectangle(TileSize.Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1, TileSize.Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + 1, Color, false);
            _graphicNeedUpdate = false;
#endif
        }

        #endregion Private Methods
    }
}
