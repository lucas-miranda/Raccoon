namespace Raccoon.Graphics.Primitives {
    public class RectanglePrimitive : PrimitiveGraphic {
        #region Constructors

        public RectanglePrimitive(float width, float height) {
            Setup(width, height);
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

        public bool Filled { get; set; } = true;

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() { 
        }

        public void Setup(Size size) {
            Size = size;
        }

        public void Setup(float width, float height) {
            Setup(new Size(width, height));
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (Filled) {
                Renderer.DrawFilledRectangle(
                    Position + position,
                    Size,
                    (Color * color) * Opacity,
                    Rotation + rotation,
                    Scale * scale,
                    Origin + origin,
                    Scroll + scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );
            } else {
                Renderer.DrawHollowRectangle(
                    Position + position,
                    Size,
                    (Color * color) * Opacity,
                    Rotation + rotation,
                    Scale * scale,
                    Origin + origin,
                    Scroll + scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );
            }
        }

        #endregion Protected Methods
    }
}
