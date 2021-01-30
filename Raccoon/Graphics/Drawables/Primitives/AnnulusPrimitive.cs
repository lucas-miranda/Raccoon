using Raccoon.Util;

namespace Raccoon.Graphics.Primitives {
    public class AnnulusPrimitive : PrimitiveGraphic {
        private Microsoft.Xna.Framework.Graphics.VertexPositionColor[] _vertices;
        private int[] _indices;

        private int _segments;
        private float _innerRadius, _outerRadius;

        private Color _innerColor = Color.White,
                      _outerColor = Color.White;

        public AnnulusPrimitive(float outerRadius, float innerRadius) {
            if (outerRadius < 0) {
                throw new System.ArgumentException("Outer Radius can't be negative", nameof(outerRadius));
            }

            if (innerRadius < 0) {
                throw new System.ArgumentException("Inner Radius can't be negative", nameof(innerRadius));
            }

            if (innerRadius > outerRadius) {
                throw new System.ArgumentException($"Inner Radius ({innerRadius}) should be smaller or equals Outer Radius ({outerRadius})", nameof(innerRadius));
            }

            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
            NeedsReload = true;
        }

        public float InnerRadius {
            get {
                return _innerRadius;
            }

            set {
                if (value > OuterRadius) {
                    throw new System.ArgumentException($"Inner Radius ({value}) should be smaller or equals Outer Radius ({OuterRadius})");
                }

                _innerRadius = value;
                NeedsReload = true;
            }
        }

        public float OuterRadius {
            get {
                return _outerRadius;
            }

            set {
                if (value < InnerRadius) {
                    throw new System.ArgumentException($"Outer Radius ({value}) should be greater or equals Inner Radius ({InnerRadius})");
                }

                _outerRadius = value;
                NeedsReload = true;
            }
        }

        public int Segments {
            get {
                return _segments > 0 ? _segments : (int) (OuterRadius <= 3 ? (3 * OuterRadius) : (2 * OuterRadius));
            }

            set {
                if (value < 0) {
                    throw new System.ArgumentException($"Segments can't be negative value ({value}), but it can be zero for default segment calculation.");
                }

                _segments = value;
                NeedsReload = true;
            }
        }

        public Color InnerColor {
            get {
                return _innerColor;
            }

            set {
                _innerColor = value;
                NeedsReload = true;
            }
        }

        public Color OuterColor {
            get {
                return _outerColor;
            }

            set {
                _outerColor = value;
                NeedsReload = true;
            }
        }

        public override void Dispose() { 
            base.Dispose();
        }

        protected override void Load() {
            base.Load();

            int segments = Segments,
                verticesCount = 2 * segments,
                indicesCount = 2 * segments * 3; // 2 triangles for each segment

            if (_indices == null || _indices.Length != indicesCount) {
                _indices = new int[indicesCount];
            }

            if (_vertices == null || _vertices.Length != verticesCount) {
                _vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[verticesCount];
            }

            // update vertices
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            Vector2 center = Vector2.Zero;
            float theta = (float) (2.0 * Math.PI / segments),
                  t, 
                  c = (float) System.Math.Cos(theta), 
                  s = (float) System.Math.Sin(theta), // precalculate the sine and cosine
                  innerX = InnerRadius * Math.Cos(0),
                  innerY = InnerRadius * Math.Sin(0),
                  outerX = OuterRadius * Math.Cos(0),
                  outerY = OuterRadius * Math.Sin(0);

            int i;
            for (i = 0; i < segments; i++) {
                _vertices[2 * i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(
                    new Microsoft.Xna.Framework.Vector3(
                        center.X + innerX, 
                        center.Y + innerY,
                        0f
                    ),
                    InnerColor
                );

                _vertices[2 * i + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(
                    new Microsoft.Xna.Framework.Vector3(
                        center.X + outerX, 
                        center.Y + outerY,
                        0f
                    ),
                    OuterColor
                );

                // apply the rotation matrix
                // inner
                t = innerX;
                innerX = c * innerX - s * innerY;
                innerY = s * t + c * innerY;

                // outer
                t = outerX;
                outerX = c * outerX - s * outerY;
                outerY = s * t + c * outerY;

                // 1st triangle
                _indices[i * 6] = 2 * i; // inner vertex
                _indices[i * 6 + 1] = 2 * i + 1; // outer vertex
                _indices[i * 6 + 2] = (2 * (i + 1)) % _vertices.Length; // next inner vertex (cyclic)

                // 2nd triangle
                _indices[i * 6 + 3] = (2 * (i + 1)) % _vertices.Length; // next inner vertex (cyclic)
                _indices[i * 6 + 4] = 2 * i + 1; // outer vertex
                _indices[i * 6 + 5] = (2 * (i + 1) + 1) % _vertices.Length; // next outer vertex (cyclic)
            }
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            Renderer.DrawVertices(
                _vertices,
                minVertexIndex: 0,
                verticesLength: _vertices.Length,
                indices: _indices,
                minIndex: 0,
                primitivesCount: 2 * Segments,
                isHollow: false,
                position: position,
                rotation: rotation,
                scale: scale,
                color: new Color(color, (color.A / 255f) * Opacity),
                origin: origin,
                scroll: scroll,
                shader: shader,
                shaderParameters: shaderParameters,
                layerDepth: layerDepth
            );
        }
    }
}
