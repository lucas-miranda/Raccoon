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

            if (_needsSetup) {
                Setup();
            }

            _indices = Filled ? FilledIndices : HollowIndices;
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            Renderer.DrawVertices(
                _vertices,
                minVertexIndex: 0,
                _vertices.Length,
                _indices,
                minIndex: 0,
                primitivesCount: Filled ? 2 : 4, // Filled ? triangles : lines
                isHollow: !Filled,
                Position + position,
                Rotation + rotation,
                Scale * scale,
                (Color * color) * Opacity,
                Origin + origin,
                Scroll + scroll,
                shader,
                shaderParameters,
                layerDepth
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

            // PointA
            _vertices[1] = new Vector2(PointA.X, PointA.Y);

            // PointB
            _vertices[3] = new Vector2(PointB.X, PointB.Y);

            // PointC
            _vertices[2] = new Vector2(PointC.X, PointC.Y);

            // PointD
            _vertices[0] = new Vector2(PointD.X, PointD.Y);

            _needsSetup = false;
        }
    }
}
