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
            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X, position.Y, 0f) * Game.Instance.Core.DefaultSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DefaultSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DefaultSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(Color.R / 255f, Color.G / 255f, Color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = Opacity;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[Shape.VertexCount * 2];
            for (int i = 0; i < Shape.VertexCount; i++) {
                Vector2 vertex = Shape[i], nextVertex = Shape[(i + 1) % Shape.VertexCount];
                vertices[i * 2] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(vertex.X - Origin.X, vertex.Y - Origin.Y, 0), FinalColor);
                vertices[i * 2 + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(nextVertex.X - Origin.X, nextVertex.Y - Origin.Y, 0), FinalColor);
            }

            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, vertices, 0, Shape.VertexCount);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.BasicEffect.View = Game.Instance.Core.BasicEffect.Projection = Microsoft.Xna.Framework.Matrix.Identity;
        }

        public override void Dispose() { }
    }
}
