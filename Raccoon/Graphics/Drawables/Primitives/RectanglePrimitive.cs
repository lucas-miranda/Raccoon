using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics.Primitives {
    public class RectanglePrimitive : PrimitiveGraphic {
        #region Private Members

        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

#if DEBUG
        private IndexBuffer _debug_indexBuffer;
#endif
        
        private VertexPositionColor[] _vertices;

        private bool _filled = true;
        private float _lastAppliedLayerDepth;

        #endregion Private Members

        #region Constructors

        public RectanglePrimitive(float width, float height) {
            Size = new Size(width, height);
            Load();
        }

        public RectanglePrimitive(float wh) : this(wh, wh) {
        }

        public RectanglePrimitive(Size size) : this(size.Width, size.Height) {
        }

        public RectanglePrimitive(Rectangle rectangle) : this(rectangle.Width, rectangle.Height) {
        }

        #endregion Constructors

        #region Public Properties

        public Rectangle Rectangle {
            get {
                return new Rectangle(Position - Origin, Size);
            }

            set {
                Position = value.Position + Origin;

                if (value.Size != Size) {
                    Size = value.Size;
                    NeedsReload = true;
                }
            }
        }

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

        #region Public Methods

        public override void Dispose() { }

        public void Setup(Size size) {
            Size = size;
            NeedsReload = true;
        }

        public void Setup(float width, float height) {
            Setup(new Size(width, height));
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();

            if (_vertexBuffer == null) {
                // first time setup
                Setup();
            }

            if (Filled) {
                _indexBuffer.SetData(new int[] { 0, 1, 2,  2, 1, 3 });

#if DEBUG
                _debug_indexBuffer.SetData(new int[] { 1, 0, 2,  1, 3, 2 });
#endif
            } else {
                _indexBuffer.SetData(new int[] { 0, 1, 3, 2, 0 });

#if DEBUG
                _debug_indexBuffer.SetData(new int[] { 1, 0, 2,  1, 3, 2 });
#endif
            }

            // update vertices
            // bottom-left
            _vertices[0].Position = new Microsoft.Xna.Framework.Vector3(0f, Height, _lastAppliedLayerDepth);

            // top-left
            _vertices[1].Position = new Microsoft.Xna.Framework.Vector3(0f, 0f, _lastAppliedLayerDepth);

            // bottom-right
            _vertices[2].Position = new Microsoft.Xna.Framework.Vector3(Width, Height, _lastAppliedLayerDepth);

            // top-right
            _vertices[3].Position = new Microsoft.Xna.Framework.Vector3(Width, 0f, _lastAppliedLayerDepth); 

            _vertexBuffer.SetData(_vertices);
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            // only update vertices layer depth if parameter value is differente from last applied value (to avoid redundancy calls)
            if (layerDepth != _lastAppliedLayerDepth) {
                UpdateVerticesLayerDepth(layerDepth);
            }

            // TODO: draw using a PrimitiveBatch to group some of them, every Renderer should provide a PrimitiveBatch interface
            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X * scale.X, Scale.Y * scale.Y, 1f)
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0f) 
                * Microsoft.Xna.Framework.Matrix.CreateRotationZ(Math.ToRadians(Rotation + rotation))
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f) 
                * Renderer.World;

            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.DiffuseColor = color * Color;
            bs.Alpha = Opacity;
            bs.TextureEnabled = false;

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
                    device.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, _vertexBuffer.VertexCount, 0, 4);
                }
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods

        #region Private Methods

        private void Setup() {
            _vertices = new VertexPositionColor[4];

            //  
            // Vertices layout:
            //
            //  1--3
            //  |\ |
            //  | \|
            //  0--2
            //

            _vertexBuffer = new DynamicVertexBuffer(Game.Instance.GraphicsDevice, VertexPositionColor.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);

            // bottom-left
            _vertices[0] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(0f, Height, _lastAppliedLayerDepth),
                Microsoft.Xna.Framework.Color.White
            );

            // top-left
            _vertices[1] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(0f, 0f, _lastAppliedLayerDepth),
                Microsoft.Xna.Framework.Color.White 
            );

            // bottom-right
            _vertices[2] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(Width, Height, _lastAppliedLayerDepth), 
                Microsoft.Xna.Framework.Color.White 
            );

            // top-right
            _vertices[3] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(Width, 0f, _lastAppliedLayerDepth), 
                Microsoft.Xna.Framework.Color.White 
            );

            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new IndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, 2 * 3, BufferUsage.WriteOnly);

#if DEBUG
            _debug_indexBuffer = new IndexBuffer(Game.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, _indexBuffer.IndexCount, BufferUsage.WriteOnly);
#endif

        }

        private void UpdateVerticesLayerDepth(float layerDepth) {
            // bottom-left
            _vertices[0].Position = new Microsoft.Xna.Framework.Vector3(0f, Height, layerDepth);

            // top-left
            _vertices[1].Position = new Microsoft.Xna.Framework.Vector3(0f, 0f, layerDepth);

            // bottom-right
            _vertices[2].Position = new Microsoft.Xna.Framework.Vector3(Width, Height, layerDepth);

            // top-right
            _vertices[3].Position = new Microsoft.Xna.Framework.Vector3(Width, 0f, layerDepth); 

            _vertexBuffer.SetData(_vertices, 0, _vertices.Length, SetDataOptions.None);

            _lastAppliedLayerDepth = layerDepth;
        }

        #endregion Private Methods
    }
}
