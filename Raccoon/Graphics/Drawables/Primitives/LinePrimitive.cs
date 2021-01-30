namespace Raccoon.Graphics.Primitives {
    public class LinePrimitive : PrimitiveGraphic {
        #region Constructors

        public LinePrimitive(Vector2 from, Vector2 to, Color color) {
            From = from;
            To = to;
            Color = color;
        }

        public LinePrimitive(Vector2 length, Color color) {
            From = Vector2.Zero;
            To = length;
            Color = color;
        }

        #endregion Constructors

        #region Public Properties

        public Vector2[] Points { get; private set; } = new Vector2[2];
        public Line Equation { get { return new Line(From, To); } }

        public Vector2 From { 
            get { 
                return Position + Points[0]; 
            } 

            set { 
                if (value == Points[0]) {
                    return;
                }

                Points[0] = value;
            } 
        }

        public Vector2 To { 
            get { 
                return Position + Points[1]; 
            } 

            set { 
                if (value == Points[1]) {
                    return;
                }


                Points[1] = value;
            } 
        }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() { 
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            Renderer.DrawLines(
                Points,
                position,
                new Color(color, (color.A / 255f) * Opacity),
                rotation,
                scale,
                origin,
                scroll,
                shader,
                shaderParameters,
                cyclic: false,
                layerDepth: layerDepth
            );
        }

        #endregion Protected Methods
    }
}
