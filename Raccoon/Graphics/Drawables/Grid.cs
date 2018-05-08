using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Grid : Graphic {
        #region Private Members

        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;
        private int _usingVerticesCount, _usingIndicesCount;

        // border
        private bool _useBorderColor;
        private Color _borderColor = Color.White;

        #endregion Private Members

        #region Constructors

        public Grid(int columns, int rows, Size tileSize) {
            Color = new Color(0x494949FF);
            Setup(columns, rows, tileSize);
        }

        public Grid(Size tileSize) {
            Color = new Color(0x494949FF);
            TileSize = tileSize;
        }

        #endregion Constructors

        #region Public Properties

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Size TileSize { get; private set; }

        public Color BorderColor {
            get {
                return _borderColor;
            }

            set {
                _borderColor = value;
                _useBorderColor = true;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
            if (Columns == 0 || Rows == 0 || TileSize.Area == 0) {
                return;
            }

            scroll += Scroll;
            scroll = scroll.LengthSquared() == 0f ? new Vector2(Util.Math.Epsilon) : scroll;
            Microsoft.Xna.Framework.Matrix scrollMatrix = Microsoft.Xna.Framework.Matrix.CreateScale(scroll.X, scroll.Y, 1f);

            BasicEffect effect = Game.Instance.Core.BasicEffect;
            float[] colorNormalized = (color * Color).Normalized;
            effect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(colorNormalized[0], colorNormalized[1], colorNormalized[2]);
            effect.Alpha = Opacity;
            effect.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X * scale.X, Scale.Y * scale.Y, 1f) * Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X - Origin.X, Position.Y + position.Y - Origin.Y, 0f) * Surface.World;
            effect.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Surface.View * scrollMatrix;
            effect.Projection = Surface.Projection;

            GraphicsDevice device = Game.Instance.Core.GraphicsDevice;
            device.Indices = _indexBuffer;
            device.SetVertexBuffer(_vertexBuffer);

            // grid
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, (Columns - 1) + (Rows - 1));
            }

            // borders
            if (_useBorderColor) {
                colorNormalized = (color * BorderColor).Normalized;
                effect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(colorNormalized[0], colorNormalized[1], colorNormalized[2]);
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.LineStrip, _usingVerticesCount - 4, _usingIndicesCount - 8, 8);
            }

            effect.Alpha = 1f;
            effect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        public void Setup(int columns, int rows, Size tileSize) {
            if (Columns == columns && Rows == rows && TileSize == tileSize) {
                return;
            }

            Columns = columns;
            Rows = rows;
            TileSize = tileSize;
            Size = new Size(Columns, Rows) * tileSize;

            if (Columns == 0 || Rows == 0) {
                return;
            }

            VertexPositionColor[] vertices = new VertexPositionColor[(Columns + Rows) * 2];
            int[] indices = new int[vertices.Length + 4];


            if (_vertexBuffer == null || vertices.Length > _vertexBuffer.VertexCount) {
                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.Core.GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || indices.Length > _indexBuffer.IndexCount) {
                _indexBuffer = new DynamicIndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            }

            //  
            // Vertices layout:
            //
            //(n-4)--4----6--(n-3)
            //  |    |    |    |
            //  |    |    |    |
            //  0----+----+----1
            //  |    |    |    |
            //  |    |    |    |
            //  2----+----+----3
            //  |    |    |    |
            //  |    |    |    |
            //(n-1)--5----7--(n-2)
            //

            int id = 0; // vertex/index id

            for (int row = 1; row < Rows; row++, id += 2) {
                vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, row * TileSize.Height, 0f), Color.White);
                vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, row * TileSize.Height, 0f), Color.White);
                indices[id] = id;
                indices[id + 1] = id + 1;
            }

            for (int column = 1; column < Columns; column++, id += 2) {
                vertices[id] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * TileSize.Width, 0f, 0f), Color.White);
                vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * TileSize.Width, Rows * TileSize.Height, 0f), Color.White);
                indices[id] = id;
                indices[id + 1] = id + 1;
            }

            // top-left
            vertices[id] = new VertexPositionColor(Microsoft.Xna.Framework.Vector3.Zero, Color.White);

            // top-right
            vertices[id + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, 0f, 0f), Color.White);

            // bottom-right
            vertices[id + 2] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Columns * TileSize.Width, Rows * TileSize.Height, 0f), Color.White);

            // bottom-left
            vertices[id + 3] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, Rows * TileSize.Height, 0f), Color.White);

            // top border
            indices[id] = 0;
            indices[id + 1] = 1;

            // right border
            indices[id + 2] = 1;
            indices[id + 3] = 2;

            // bottom border
            indices[id + 4] = 2;
            indices[id + 5] = 3;

            // left border
            indices[id + 6] = 3;
            indices[id + 7] = 0;

            _usingVerticesCount = vertices.Length;
            _usingIndicesCount = indices.Length;

            _vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.None);
            _indexBuffer.SetData(indices, 0, indices.Length, SetDataOptions.None);
        }

        public void Setup(int columns, int rows) {
            Setup(columns, rows, TileSize);
        }

        public override void Dispose() { }

        #endregion Public Methods
    }
}
