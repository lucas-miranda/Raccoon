using System.Collections.Generic;

namespace Raccoon.Graphics.Primitives {
    public class Polygon : Graphic {
        public Polygon(Raccoon.Polygon polygon, Color color) {
            Shape = polygon;
            Color = color;
        }

        public Polygon(IEnumerable<Vector2> points, Color color) : this(new Raccoon.Polygon(points), color) { }

        public Raccoon.Polygon Shape { get; set; }

        public override void Render(Vector2 position, float rotation) {
            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            for (int i = 0; i < Shape.VertexCount; i++) {
                Vector2 vertex = Shape[i], nextVertex = Shape[(i + 1) % Shape.VertexCount];
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                        new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(position.X - Origin.X + vertex.X, position.Y - Origin.Y + vertex.Y, 0), Color),
                        new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(position.X - Origin.X + nextVertex.X, position.Y - Origin.Y + nextVertex.Y, 0), Color)
                    }, 0, 1);
            }
        }

        public override void Dispose() { }
    }
}
