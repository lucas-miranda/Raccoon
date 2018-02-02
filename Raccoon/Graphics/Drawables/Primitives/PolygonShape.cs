using System.Collections.Generic;

namespace Raccoon.Graphics.Primitives {
    public class PolygonPrimitive : Graphic {
        public PolygonPrimitive(Polygon polygon, Color color) {
            Shape = polygon;
            Color = color;
        }

        public PolygonPrimitive(IEnumerable<Vector2> points, Color color) : this(new Raccoon.Polygon(points), color) { }

        public Polygon Shape { get; set; }

        public override void Render(Vector2 position, Color color, float rotation) {
            if (Shape.VertexCount == 0) {
                return;
            }

            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X, position.Y, 0f) * Game.Instance.Core.MainSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.MainSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.MainSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = Opacity;

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[Shape.VertexCount * 2];
            for (int i = 0; i < Shape.VertexCount; i++) {
                Vector2 vertex = Shape[i], nextVertex = Shape[(i + 1) % Shape.VertexCount];
                vertices[i * 2] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(vertex.X - Origin.X, vertex.Y - Origin.Y, 0), Microsoft.Xna.Framework.Color.White);
                vertices[i * 2 + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(nextVertex.X - Origin.X, nextVertex.Y - Origin.Y, 0), Microsoft.Xna.Framework.Color.White);
            }

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, vertices, 0, Shape.VertexCount);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.BasicEffect.View = Game.Instance.Core.BasicEffect.Projection = Microsoft.Xna.Framework.Matrix.Identity;
        }

        public override void Dispose() { }
    }
}
