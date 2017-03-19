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

        public override void Render(Vector2 position, float rotation) {
            if (_vertices.Length == 0) {
                return;
            }

            Game.Instance.Core.GraphicsDevice.SetRenderTarget(Game.Instance.Core.SecondaryCanvas.XNARenderTarget);
            Game.Instance.Core.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
            Game.Instance.Core.RenderTargetStack.Push(Game.Instance.Core.SecondaryCanvas.XNARenderTarget);

            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X, Scale.Y, 1) * Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X * Scroll.X, position.Y * Scroll.Y, 0f) * Microsoft.Xna.Framework.Matrix.CreateLookAt(new Microsoft.Xna.Framework.Vector3(0f, 0f, 1f), new Microsoft.Xna.Framework.Vector3(0f, 0f, -1f), Microsoft.Xna.Framework.Vector3.Up) * Surface.World;
            Game.Instance.Core.BasicEffect.View = Microsoft.Xna.Framework.Matrix.CreateScale(1f / Scroll.X, 1f / Scroll.Y, 1f) * Surface.View * Microsoft.Xna.Framework.Matrix.CreateScale(Scroll.X, Scroll.Y, 1f);
            Game.Instance.Core.BasicEffect.Projection = Surface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(Color.R / 255f, Color.G / 255f, Color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = Opacity;

            foreach (EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _vertices, 0, _lineCount);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);

            // draw to main render target
            Game.Instance.Core.RenderTargetStack.Pop();
            Game.Instance.Core.GraphicsDevice.SetRenderTarget(Game.Instance.Core.RenderTargetStack.Peek());
            Game.Instance.Core.MainSpriteBatch.Begin(SpriteSortMode.Immediate, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null, null);
            Game.Instance.Core.MainSpriteBatch.Draw(Game.Instance.Core.SecondaryCanvas.XNARenderTarget, Microsoft.Xna.Framework.Vector2.Zero, Microsoft.Xna.Framework.Color.White);
            Game.Instance.Core.MainSpriteBatch.End();
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
