using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics.Primitives {
    public class CirclePrimitive : Graphic {
        #region Private Members

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

#if DEBUG
        private IndexBuffer _debug_indexBuffer;
#endif
        
        private int _segments;
        private float _radius;
        private bool _filled;

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
                return _segments > 0 ? _segments : (int) (Radius + Radius);
            }

            set {
                _segments = value;
                NeedsReload = true;
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
            effect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X - Origin.X, Position.Y + position.Y - Origin.Y, 0f) * Surface.World;
            effect.View = Microsoft.Xna.Framework.Matrix.Invert(scrollMatrix) * Surface.View * scrollMatrix;
            effect.Projection = Surface.Projection;

            GraphicsDevice device = Game.Instance.Core.GraphicsDevice;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                device.Indices = _indexBuffer;
                device.SetVertexBuffer(_vertexBuffer);

                if (Filled) {
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Segments);
                } else {
                    device.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, Segments);
                }
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

            int[] indices = null;

            if (Filled) {
                indices = new int[(segments + 1) * 3];
            } else {
                indices = new int[segments + 1];
            }

            VertexPositionColor[] _vertices = new VertexPositionColor[segments + 1];

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            Vector2 center = Vector2.Zero;
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = Radius * Math.Cos(0),
                  y = Radius * Math.Sin(0);

            // center
            int centerIndex = _vertices.Length - 1;
            _vertices[centerIndex] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X, center.Y, 0f), Color.White);

            int i;
            for (i = 0; i < _vertices.Length; i++) {
                _vertices[i] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X + x, center.Y + y, 0f), Color.White);

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;

                if (Filled) {
                    indices[i * 3] = centerIndex; // circle center
                    indices[i * 3 + 1] = i; // current vertex
                    indices[i * 3 + 2] = 1 + (i % _vertices.Length); // next vertex (cyclic)
                } else {
                    indices[i] = i; // current vertex
                }
            }

            _indexBuffer = new IndexBuffer(Game.Instance.Core.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            _vertexBuffer = new DynamicVertexBuffer(Game.Instance.Core.GraphicsDevice, VertexPositionColor.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices);
        }

        #endregion Protected Methods
    }
}
