using System.Collections.Generic;

namespace Raccoon.Graphics.Primitives {
    public class PolygonPrimitive : PrimitiveGraphic {
        private Polygon _shape;

        #region Constructors

        public PolygonPrimitive(Polygon polygon, Color color) {
            if (polygon == null) {
                throw new System.ArgumentNullException(nameof(polygon));
            }

            Shape = polygon;
            Color = color;
        }

        public PolygonPrimitive(IEnumerable<Vector2> points, Color color) : this(new Polygon(points), color) { 
            if (points == null) {
                throw new System.ArgumentNullException(nameof(points));
            }
        }

        #endregion Constructors

        #region Public Properties

        public bool Filled { get; set; }

        public Polygon Shape {
            get {
                return _shape;
            }

            set {
                if (value == null) {
                    throw new System.ArgumentNullException(nameof(value));
                }

                _shape = value;
                Size = _shape.BoundingBox().Size;
            }
        }

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
                    color * Opacity,
                    rotation,
                    scale,
                    origin,
                    scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );
            } else {
                Renderer.DrawHollowPolygon(
                    Shape,
                    position,
                    color * Opacity,
                    rotation,
                    scale,
                    origin,
                    scroll,
                    shader,
                    shaderParameters,
                    layerDepth
                );
            }
        }

        #endregion Protected Methods
    }
}
