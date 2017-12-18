using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Grid : Graphic {
        private VertexPositionColor[] _vertices = new VertexPositionColor[0];
        private int _lineCount;

        public Grid(int columns, int rows, Size tileSize, Color? color = null) {
            Setup(columns, rows, tileSize, color);
        }

        public Grid(Size tileSize, Color? color = null) {
            TileSize = tileSize;
            Color = color == null ? Color : color.Value;
        }

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Size TileSize { get; private set; }

        public override void Render(Vector2 position, Color color, float rotation) {
            if (_vertices.Length == 0) {
                return;
            }

            Vector2 scroll = Scroll.X == 0f && Scroll.Y == 0f ? new Vector2(Util.Math.Epsilon) : Scroll;
            Microsoft.Xna.Framework.Matrix scrollMatrix = Microsoft.Xna.Framework.Matrix.CreateScale(scroll.X, scroll.Y, 1f);

            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X, Scale.Y, 1) * Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X, position.Y, 0f) * Surface.World;
            Game.Instance.Core.BasicEffect.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Surface.View * scrollMatrix;
            Game.Instance.Core.BasicEffect.Projection = Surface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = Opacity;

            foreach (EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _vertices, 0, _lineCount);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
        }

        public void Setup(int columns, int rows, Size tileSize, Color? color = null) {
            Color = color == null ? Color : color.Value;
            if (Columns == columns && Rows == rows && TileSize == tileSize) {
                return;
            }

            Columns = columns;
            Rows = rows;
            TileSize = tileSize;
            Size = new Size(Columns * TileSize.Width, Rows * TileSize.Height);

            if (Columns == 0 || Rows == 0) {
                return;
            }

            _vertices = new VertexPositionColor[2 * (Columns + Rows + 2)];
            _lineCount = Columns + Rows + 2;

            int id = 0;
            for (int column = 1; column < Columns; column++) {
                _vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * TileSize.Width, 0, 0), new Color(0x494949ff));
                _vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * TileSize.Width, Rows * TileSize.Height, 0), new Color(0x494949ff));
                id += 2;
            }

            for (int row = 1; row < Rows; row++) {
                _vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, row * TileSize.Height, 0), new Color(0x494949ff));
                _vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, row * TileSize.Height, 0), new Color(0x494949ff));
                id += 2;
            }

            // left border
            _vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White);
            _vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, Rows * TileSize.Height, 0), Color.White);
            id += 2;

            // right border
            _vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, 0, 0), Color.White);
            _vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, Rows * TileSize.Height, 0), Color.White);
            id += 2;

            // top border
            _vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White);
            _vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, 0, 0), Color.White);
            id += 2;

            // bottom border
            _vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, Rows * TileSize.Height, 0), Color.White);
            _vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, Rows * TileSize.Height, 0), Color.White);
            id += 2;
        }

        public void Setup(int columns, int rows) {
            Setup(columns, rows, TileSize, Color);
        }

        public override void Dispose() { }
    }
}
