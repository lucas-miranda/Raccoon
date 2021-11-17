using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public class Quad : Graphic {
        #region Private Members

        private static readonly int[] FilledIndices = new int[] { 0, 1, 2,  2, 1, 3 },
                                      HollowIndices = new int[] {
                                                          1, 3,
                                                          3, 2,
                                                          2, 0,
                                                          0, 1
                                                      };

        private Vector2[] _vertices = new Vector2[4];
        private int[] _indices;
        private VertexPositionColorTexture[] _vertexData;

        private bool _filled = true,
                     _needsSetup;

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

        public Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Texture texture, Rectangle? sourceRegion = null, Rectangle? clippingRegion = null) {
            Setup(a, b, c, d);

            if (sourceRegion != null) {
                if (clippingRegion != null) {
                    Setup(texture, sourceRegion.Value, clippingRegion.Value);
                } else {
                    Setup(texture, sourceRegion.Value);
                }
            } else {
                Setup(texture);
            }

            Load();
        }

        public Quad(float width, float height, Texture texture, Rectangle? sourceRegion = null, Rectangle? clippingRegion = null) {
            Setup(width, height);

            if (sourceRegion != null) {
                if (clippingRegion != null) {
                    Setup(texture, sourceRegion.Value, clippingRegion.Value);
                } else {
                    Setup(texture, sourceRegion.Value);
                }
            } else {
                Setup(texture);
            }

            Load();
        }

        public Quad(float wh, Texture texture, Rectangle? sourceRegion = null, Rectangle? clippingRegion = null) : this(wh, wh, texture, sourceRegion, clippingRegion) {
        }

        public Quad(Size size, Texture texture, Rectangle? sourceRegion = null, Rectangle? clippingRegion = null) : this(size.Width, size.Height, texture, sourceRegion, clippingRegion) {
        }

        #endregion Constructors

        #region Public Properties

        public Vector2 PointA { get; private set; }
        public Vector2 PointB { get; private set; }
        public Vector2 PointC { get; private set; }
        public Vector2 PointD { get; private set; }
        public Texture Texture { get; private set; }
        public Rectangle SourceRegion { get; private set; }
        public Rectangle ClippingRegion { get; private set; }

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

        public void Setup(Vector2[] vertices) {
            if (vertices.Length < 4) {
                throw new System.ArgumentException("Vertices count should be at least 4.");
            }

            Setup(
                vertices[0],
                vertices[1],
                vertices[2],
                vertices[3]
            );
        }

        public void Setup(float width, float height) {
            PointA = Vector2.Zero;
            PointB = new Vector2(width, 0f);
            PointC = new Vector2(width, height);
            PointD = new Vector2(0f, height);
            Size = new Size(width, height);
            NeedsReload = _needsSetup = true;
        }

        public void Setup(Texture texture) {
            NeedsReload = _needsSetup = Texture != texture && (Texture == null || texture == null);
            Texture = texture;

            if (texture == null) {
                ClippingRegion = SourceRegion = Rectangle.Empty;
            } else {
                ClippingRegion = SourceRegion = texture.Bounds;
            }
        }

        public void Setup(Texture texture, Rectangle clippingRegion) {
            if (texture == null) {
                throw new System.ArgumentNullException("Invalid texture.");
            }

            NeedsReload = _needsSetup = Texture == null;
            Texture = texture;
            SourceRegion = texture.Bounds;

            if (clippingRegion.Left < 0 || clippingRegion.Top < 0
              || clippingRegion.Right > SourceRegion.Width
              || clippingRegion.Bottom > SourceRegion.Height) {
                throw new System.ArgumentOutOfRangeException("clippingRegion", clippingRegion, $"Value must be within source region bounds {SourceRegion}");
            }

            ClippingRegion = clippingRegion;
        }

        public void Setup(Texture texture, Rectangle sourceRegion, Rectangle clippingRegion) {
            if (texture == null) {
                throw new System.ArgumentNullException("Invalid texture.");
            }

            NeedsReload = _needsSetup = Texture == null;
            Texture = texture;

            if (sourceRegion.Left < Texture.Bounds.Left || sourceRegion.Top < Texture.Bounds.Top
              || sourceRegion.Right > Texture.Bounds.Right || sourceRegion.Bottom > Texture.Bounds.Bottom) {
                throw new System.ArgumentOutOfRangeException("sourceRegion", sourceRegion, "Value must be within texture bounds.");
            }

            SourceRegion = sourceRegion;

            if (clippingRegion.Left < 0 || clippingRegion.Top < 0
              || clippingRegion.Right > SourceRegion.Width
              || clippingRegion.Bottom > SourceRegion.Height) {
                throw new System.ArgumentOutOfRangeException("clippingRegion", clippingRegion, $"Value must be within source region bounds {SourceRegion}");
            }

            ClippingRegion = clippingRegion;
            NeedsReload = _needsSetup = true;
        }

        public void Setup(Texture texture, (Vector2 Position, Color Color, Vector2 TextureCoordinate)[] vertexData) {
            if (texture == null) {
                throw new System.ArgumentNullException("Invalid texture.");
            }

            Texture = texture;

            if (vertexData.Length < 4) {
                throw new System.ArgumentException("Vertices count should be at least 4.");
            }

            if (_vertexData == null) {
                _vertexData = new VertexPositionColorTexture[4];
            }

            for (int i = 0; i < 4; i++) {
                (Vector2 Position, Color Color, Vector2 TextureCoordinate) vertex = vertexData[i];
                _vertexData[i] = new VertexPositionColorTexture(
                    new Microsoft.Xna.Framework.Vector3(vertex.Position, 0f),
                    vertex.Color,
                    vertex.TextureCoordinate
                );
            }
        }

        #region Protected Methods

        protected override void Load() {
            base.Load();

            if (_needsSetup) {
                Setup();
            }

            _indices = Filled ? FilledIndices : HollowIndices;
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (Texture != null) {
                Renderer.Draw(
                    Texture,
                    _vertexData,
                    position,
                    rotation,
                    scale,
                    Flipped,
                    color * Opacity,
                    origin,
                    scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );

                return;
            }

            Renderer.DrawVertices(
                _vertices,
                minVertexIndex: 0,
                verticesLength: _vertices.Length,
                indices: _indices,
                minIndex: 0,
                primitivesCount: Filled ? 2 : 4, // Filled ? triangles : lines
                isHollow: !Filled,
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

        #endregion Protected Methods

        private void Setup() {
            // preparing vertices

            //
            // Vertices layout:
            //
            //  1--3
            //  |\ |
            //  | \|
            //  0--2
            //

            _vertices[1] = PointA;
            _vertices[3] = PointB;
            _vertices[2] = PointC;
            _vertices[0] = PointD;

            _needsSetup = false;
        }
    }
}
