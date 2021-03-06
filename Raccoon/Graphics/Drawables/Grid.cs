﻿using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class Grid : PrimitiveGraphic {
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
                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            }

            if (_indexBuffer == null || indices.Length > _indexBuffer.IndexCount) {
                _indexBuffer = new DynamicIndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
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

            _vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.Discard);
            _indexBuffer.SetData(indices, 0, indices.Length, SetDataOptions.Discard);
        }

        public void Setup(int columns, int rows) {
            Setup(columns, rows, TileSize);
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            if (_vertexBuffer != null && !_vertexBuffer.IsDisposed) {
                _vertexBuffer.Dispose();
            }

            if (_indexBuffer != null && !_indexBuffer.IsDisposed) {
                _indexBuffer.Dispose();
            }

            _vertexBuffer = null;
            _indexBuffer = null;

            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (Columns == 0 || Rows == 0 || TileSize.Area == 0) {
                return;
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateScale(scale.X, scale.Y, 1f)
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(-origin.X, -origin.Y, 0f)
                * Microsoft.Xna.Framework.Matrix.CreateRotationZ(Math.ToRadians(rotation))
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X, position.Y, 0f)
                * Renderer.World;

            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.SetMaterial(color, Opacity);

            GraphicsDevice device = Game.Instance.GraphicsDevice;

            // texture
            bs.TextureEnabled = false;

            shaderParameters?.ApplyParameters(shader);

            // we need to manually update every GraphicsDevice states here
            device.BlendState = Renderer.Batch.BlendState;
            device.SamplerStates[0] = Renderer.Batch.SamplerState;
            device.DepthStencilState = Renderer.Batch.DepthStencilState;
            device.RasterizerState = Renderer.Batch.RasterizerState;

            // grid
            foreach (object pass in bs) {
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _vertexBuffer.VertexCount, 0, Columns - 1 + (Rows - 1));
            }

            // borders
            if (_useBorderColor) {
                bs.SetMaterial(BorderColor, Opacity);
            }

            shaderParameters?.ApplyParameters(shader);

            // we need to manually update every GraphicsDevice states here
            device.BlendState = Renderer.Batch.BlendState;
            device.SamplerStates[0] = Renderer.Batch.SamplerState;
            device.DepthStencilState = Renderer.Batch.DepthStencilState;
            device.RasterizerState = Renderer.Batch.RasterizerState;

            foreach (object pass in bs) {
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.LineList, _usingVerticesCount - 4, 0, _vertexBuffer.VertexCount, _usingIndicesCount - 8, 4);
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods
    }
}
