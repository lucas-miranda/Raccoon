using System.Collections.Generic;

namespace Raccoon.Graphics.Primitives {
    public class PolygonPrimitive : PrimitiveGraphic {
        #region Constructors

        public PolygonPrimitive(Polygon polygon, Color color) {
            Shape = polygon;
            Color = color;
        }

        public PolygonPrimitive(IEnumerable<Vector2> points, Color color) : this(new Polygon(points), color) { 
        }

        #endregion Constructors

        #region Public Properties

        public Polygon Shape { get; set; }
        public bool Filled { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() { 
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (Shape.VertexCount == 0) {
                return;
            }

            if (Filled) {
                Renderer.DrawFilledPolygon(
                    Shape,
                    position,
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
                Renderer.DrawHollowPolygon(
                    Shape,
                    position,
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
