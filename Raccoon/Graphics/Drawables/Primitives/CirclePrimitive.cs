using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics.Primitives {
    public class CirclePrimitive : Graphic {
        #region Private Members

        private DynamicVertexBuffer _vertexBuffer;
        private DynamicIndexBuffer _indexBuffer;

#if DEBUG
        private DynamicIndexBuffer _debug_indexBuffer;
#endif
        
        private VertexPositionColor[] _vertices;

        private int _segments;
        private float _radius;

        #endregion Private Members

        #region Constructors

        public CirclePrimitive(float radius) {
            Radius = radius;
            Load();
        }

        public CirclePrimitive(Circle circle) : this(circle.Radius) {
        }

        #endregion Constructors

        #region Public Properties

        public float Radius {
            get {
                return _radius;
            }

            set {
                _radius = value;
                Size = new Size(_radius * _radius);
                NeedsReload = true;
            }
        }

        public float Diameter {
            get {
                return 2f * _radius;
            }

            set {
                Radius = value / 2f;
            }
        }

        public int Segments {
            get {
                return _segments > 0 ? _segments : (int) (Radius * Radius);
            }

            set {
                _segments = value;
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
            effect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X - Origin.X, Position.Y + position.Y - Origin.Y, 0f) * Surface.World;
            effect.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Surface.View * scrollMatrix;
            effect.Projection = Surface.Projection;

            GraphicsDevice device = Game.Instance.Core.GraphicsDevice;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, Segments + 1);
            }

            effect.Alpha = 1f;
            effect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;

        }

        public override void Dispose() { }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();

            int segments = Segments;

            if (_vertexBuffer == null) {
                // first time setup
                Setup();
                _vertices = new VertexPositionColor[segments + 1];
                _vertexBuffer = new DynamicVertexBuffer(Game.Instance.Core.GraphicsDevice, VertexPositionColor.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
            } else if (_vertices.Length != segments + 1) {
                _vertices = new VertexPositionColor[segments + 1];
            }

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            Vector2 center = Vector2.Zero;
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = Radius * Math.Cos(0),
                  y = Radius * Math.Sin(0);


            int i;
            for (i = 0; i < _vertices.Length; i++) {
                _vertices[i] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X + x, center.Y + y, 0f), Color.White);

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;
            }

            //_vertices[i] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X + x, center.Y + y, 0), Color.White); // just to close the last segment

            _vertexBuffer.SetData(_vertices);
        }

        #endregion Protected Methods

        #region Private Methods

        private void Setup() {
            int[] indices = new int[2 * 3];
#if DEBUG
            int[] debug_indices = new int[indices.Length];
#endif

            //  
            // Vertices layout:
            //
            //  1--3
            //  |\ |
            //  | \|
            //  0--2
            //

            _indexBuffer = new DynamicIndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;

            _indexBuffer.SetData(indices);

#if DEBUG
            _debug_indexBuffer = new DynamicIndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, debug_indices.Length, BufferUsage.WriteOnly);

            debug_indices[0] = 1;
            debug_indices[1] = 0;
            debug_indices[2] = 2;
            debug_indices[3] = 1;
            debug_indices[4] = 3;
            debug_indices[5] = 2;

            _debug_indexBuffer.SetData(debug_indices);
#endif

        }

        #endregion Private Methods
    }
}
