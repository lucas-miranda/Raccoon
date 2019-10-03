using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public class Quad : Graphic {
        #region Private Members

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        private bool _filled = true, _needsSetup;
        private float _lastAppliedLayerDepth;

        #endregion Private Members

        #region Constructors

        public Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            Setup(a, b, c, d);
            Load();
        }

        public Quad(float width, float height) {
            Setup(width, height);
        }

        public Quad(float wh) : this(wh, wh) {
        }

        public Quad(Size size) : this(size.Width, size.Height) {
        }

        #endregion Constructors

        #region Public Properties

        public Vector2 PointA { get; private set; }
        public Vector2 PointB { get; private set; }
        public Vector2 PointC { get; private set; }
        public Vector2 PointD { get; private set; }

        public bool Filled {
            get {
                return _filled;
            }

            set {
                if (value == _filled) {
                    return;
                }

                _filled = value;
                NeedsReload = true;
            }
        }

        #endregion Public Properties

        public override void Update(int delta) {
            base.Update(delta);
        }

        public void Setup(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            PointA = a;
            PointB = b;
            PointC = c;
            PointD = d;

            float left = Math.Min(Math.Min(Math.Min(a.X, b.X), c.X), d.X),
                  right = Math.Max(Math.Max(Math.Max(a.X, b.X), c.X), d.X),
                  top = Math.Min(Math.Min(Math.Min(a.Y, b.Y), c.Y), d.Y),
                  bottom = Math.Max(Math.Max(Math.Max(a.Y, b.Y), c.Y), d.Y);

            Size = new Size(Math.Abs(right - left), Math.Abs(bottom - top));

            NeedsReload = _needsSetup = true;
        }

        public void Setup(float width, float height) {
            PointA = Vector2.Zero;
            PointB = new Vector2(width, 0f);
            PointC = new Vector2(width, height);
            PointD = new Vector2(0f, height);
            Size = new Size(width, height);
            NeedsReload = _needsSetup = true;
        }

        #region Protected Methods

        protected override void Load() {
            base.Load();

            if (_vertexBuffer == null || _needsSetup) {
                Setup();
            }

            if (Filled) {
                _indexBuffer.SetData(new int[] { 0, 1, 2,  2, 1, 3 });
            } else {
                _indexBuffer.SetData(new int[] { 
                    1, 3,
                    3, 2,
                    2, 0,
                    0, 1
                });
            }
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            // only update vertices layer depth if parameter value is differente from last applied value (to avoid redundancy calls)
            if (layerDepth != _lastAppliedLayerDepth) {
                UpdateVerticesLayerDepth(layerDepth);
            }

            // TODO: draw using a PrimitiveBatch to group some of them, every Renderer should provide a PrimitiveBatch interface
            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X * scale.X, Scale.Y * scale.Y, 1f)
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(-(Origin.X + origin.X), -(Origin.Y + origin.Y), 0f)
                * Microsoft.Xna.Framework.Matrix.CreateRotationZ(Math.ToRadians(Rotation + rotation))
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f)
                * Renderer.World;

            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.DiffuseColor = color * Color;
            bs.Alpha = Opacity;
            bs.TextureEnabled = false;

            shaderParameters?.ApplyParameters(shader);

            GraphicsDevice device = Game.Instance.GraphicsDevice;

            // we need to manually update every GraphicsDevice states here
            device.BlendState = Renderer.SpriteBatch.BlendState;
            device.SamplerStates[0] = Renderer.SpriteBatch.SamplerState;
            device.DepthStencilState = Renderer.SpriteBatch.DepthStencilState;
            device.RasterizerState = Renderer.SpriteBatch.RasterizerState;

            foreach (object pass in bs) {
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);

                if (Filled) {
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.VertexCount, 0, 2);
                } else {
                    device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _vertexBuffer.VertexCount, 0, 4);
                }
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods

        private void Setup() {
            // preparing vertices
            VertexPositionColor[] vertices = new VertexPositionColor[4];

            //
            // Vertices layout:
            //
            //  1--3
            //  |\ |
            //  | \|
            //  0--2
            //

            // PointA
            vertices[1] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointA.X, PointA.Y, _lastAppliedLayerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // PointB
            vertices[3] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointB.X, PointB.Y, _lastAppliedLayerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // PointC
            vertices[2] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointC.X, PointC.Y, _lastAppliedLayerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // PointD
            vertices[0] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointD.X, PointD.Y, _lastAppliedLayerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            if (_vertexBuffer == null) {
                _vertexBuffer = new VertexBuffer(
                    Game.Instance.GraphicsDevice, 
                    VertexPositionColor.VertexDeclaration, 
                    vertices.Length, 
                    BufferUsage.WriteOnly
                );
            }

            _vertexBuffer.SetData(vertices);

            if (_indexBuffer == null) {
                _indexBuffer = new IndexBuffer(
                    Game.Instance.GraphicsDevice, 
                    IndexElementSize.ThirtyTwoBits, 
                    8,
                    BufferUsage.WriteOnly
                );
            }

            _needsSetup = false;
        }

        private void UpdateVerticesLayerDepth(float layerDepth) {
            VertexPositionColor[] vertices = new VertexPositionColor[4];

            // PointA
            vertices[1] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointA.X, PointA.Y, layerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // PointB
            vertices[3] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointB.X, PointB.Y, layerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // PointC
            vertices[2] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointC.X, PointC.Y, layerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // PointD
            vertices[0] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(PointD.X, PointD.Y, layerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            _vertexBuffer.SetData(0, vertices, 0, vertices.Length, VertexPositionColor.VertexDeclaration.VertexStride);

            _lastAppliedLayerDepth = layerDepth;
        }
    }
}
