﻿using Raccoon.Graphics;
using System;

namespace Raccoon.Components {
    public class GridCollider : ColliderComponent {
        private bool[,] _data;
        private bool _graphicNeedUpdate;
        private Size _tileSize;

        public GridCollider(Size tileSize, int columns, int rows, string tag) : base(ColliderType.Grid, tag) {
            TileSize = tileSize;
            Columns = columns;
            Rows = rows;
            _data = new bool[Rows, Columns];
            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    _data[y, x] = false;
                }
            }
        }

        public GridCollider(Size tileSize, int columns, int rows, Enum tag) : this(tileSize, columns, rows, tag.ToString()) { }


        public Vector2 Origin { get; set; }
        public Vector2 Position { get { return Entity.Position - Origin; } }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public Size Size { get { return new Size(TileSize.Width * Columns, TileSize.Height * Rows); } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public Rectangle Rect { get { return new Rectangle(Position, Size); } }

        public Size TileSize {
            get {
                return _tileSize;
            }

            set {
                _tileSize = value;
                _graphicNeedUpdate = true;
            }
        }

        public override void Update(int delta) {
        }

        public override void DebugRender() {
            if (_graphicNeedUpdate) {
                if (Graphic == null) {
                    Graphic = new Graphics.Primitives.Rectangle(TileSize.Width * Game.Instance.Scale + 1, TileSize.Height * Game.Instance.Scale + 1, Color, false);
                } else {
                    (Graphic as Graphics.Primitives.Rectangle).Size = new Size(TileSize.Width * Game.Instance.Scale + 1, TileSize.Height * Game.Instance.Scale + 1);
                }

                _graphicNeedUpdate = false;
            }

            for (int y = 0; y < Rows; y++) {
                for (int x = 0; x < Columns; x++) {
                    if (!_data[y, x])
                        continue;

                    Graphic.Position = Position * Game.Instance.Scale + new Vector2(x * TileSize.Width * Game.Instance.Scale, y * TileSize.Height * Game.Instance.Scale);
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
    }
}
