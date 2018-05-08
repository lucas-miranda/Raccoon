using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitives {
    public class RectanglePrimitive : Graphic {
        #region Private Members

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

#if DEBUG
        private IndexBuffer _debug_indexBuffer;
#endif
        
        private VertexPositionColor[] _vertices;

        private bool _filled = true;

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

        public override void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
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
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);

                if (Filled) {
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                } else {
                    device.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, 4);
                }
            }

            effect.Alpha = 1f;
            effect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

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
            _vertices[0].Position = new Microsoft.Xna.Framework.Vector3(0f, Height, 0f);

            // top-left
            _vertices[1].Position = new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f);

            // bottom-right
            _vertices[2].Position = new Microsoft.Xna.Framework.Vector3(Width, Height, 0f);

            // top-right
            _vertices[3].Position = new Microsoft.Xna.Framework.Vector3(Width, 0f, 0f); 

            _vertexBuffer.SetData(_vertices);
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

            _vertexBuffer = new VertexBuffer(Game.Instance.Core.GraphicsDevice, VertexPositionColor.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);

            // bottom-left
            _vertices[0] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(0f, Height, 0f),
                Microsoft.Xna.Framework.Color.White
            );

            // top-left
            _vertices[1] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f),
                Microsoft.Xna.Framework.Color.White 
            );

            // bottom-right
            _vertices[2] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(Width, Height, 0f), 
                Microsoft.Xna.Framework.Color.White 
            );

            // top-right
            _vertices[3] = new VertexPositionColor(
                new Microsoft.Xna.Framework.Vector3(Width, 0f, 0f), 
                Microsoft.Xna.Framework.Color.White 
            );

            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new IndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, 2 * 3, BufferUsage.WriteOnly);

#if DEBUG
            _debug_indexBuffer = new IndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, _indexBuffer.IndexCount, BufferUsage.WriteOnly);
#endif

        }

        #endregion Private Methods
    }
}
