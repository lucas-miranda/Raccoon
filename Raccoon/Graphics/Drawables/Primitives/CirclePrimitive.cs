using Raccoon.Util;

namespace Raccoon.Graphics.Primitives {
    public class CirclePrimitive : PrimitiveGraphic {
        #region Private Members

        private Vector2[] _vertices;
        private int[] _indices;

        private int _segments;
        private float _radius, _arcFill = 1f;
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
                Size = new Size(_radius + _radius);
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
                return _segments > 0 ? _segments : (int) (Radius <= 3 ? (3 * Radius) : (2 * Radius));
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

        public float ArcFill {
            get {
                return _arcFill;
            }

            set {
                _arcFill = Math.Clamp(value, 0f, 1f);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() { 
            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();

            int segments = Segments,
                indicesCount;

            if (Filled) {
                indicesCount = segments * 3 + 1;
            } else {
                indicesCount = segments * 2 + 1;
            }

            if (_indices == null) {
                _indices = new int[indicesCount];
            } else if (_indices.Length < indicesCount) {
                System.Array.Resize(ref _indices, indicesCount);
            }

            if (_vertices == null) {
                _vertices = new Vector2[segments + 1];
            } else if (_vertices.Length < segments + 1) {
                System.Array.Resize(ref _vertices, segments + 1);
            }

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            Vector2 center = Vector2.Zero;
            float theta = (float) (2.0 * Math.PI / segments);
            float t, c = (float) System.Math.Cos(theta), s = (float) System.Math.Sin(theta); // precalculate the sine and cosine

            float x = Radius * Math.Cos(0),
                  y = Radius * Math.Sin(0);

            // center
            int centerIndex = _vertices.Length - 1;
            _vertices[centerIndex] = center;

            int i;
            for (i = 0; i < _vertices.Length - 1; i++) {
                _vertices[i] = new Vector2(center.X + x, center.Y + y);

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;

                if (Filled) {
                    _indices[i * 3] = centerIndex; // circle center
                    _indices[i * 3 + 1] = i; // current vertex
                    _indices[i * 3 + 2] = (i + 1) % (_vertices.Length - 1); // next vertex (cyclic)
                } else {
                    _indices[i * 2] = i; // current vertex
                    _indices[i * 2 + 1] = (i + 1) % (_vertices.Length - 1); // current vertex
                }
            }
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (_arcFill <= 0.001f) {
                return;
            }
             
            Renderer.DrawVertices(
                _vertices,
                minVertexIndex: 0,
                _vertices.Length,
                _indices,
                minIndex: 0,
                primitivesCount: Math.EqualsEstimate(_arcFill, 1f) ? Segments : (int) Math.Round(_arcFill * Segments),
                isHollow: !Filled,
                position,
                rotation,
                scale,
                new Color(color, (color.A / 255f) * Opacity),
                origin,
                scroll,
                shader,
                shaderParameters,
                layerDepth
            );
        }

        #endregion Protected Methods
    }
}
