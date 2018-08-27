namespace Raccoon.Graphics.Primitives {
    public class LinePrimitive : Graphic {
        private Vector2 _to;

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

        public Vector2 From { get { return Position - Origin; } set { Position = value + Origin; } }
        public Vector2 To { get { return From + _to; } set { _to = value - From; } }
        public Line Equation { get { return new Line(From, To); } }

        public override void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Renderer.World;
            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.SetMaterial(color * Color, Opacity);

            foreach (var pass in bs) {
                Game.Instance.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                        new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Position.X + position.X - Origin.X, Position.Y + position.Y - Origin.Y, 0), Microsoft.Xna.Framework.Color.White),
                        new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Position.X + position.X - Origin.X + _to.X, Position.Y + position.Y - Origin.Y + _to.Y, 0), Microsoft.Xna.Framework.Color.White)
                    }, 0, 1);
            }

            bs.ResetParameters();
        }

        public override void Dispose() { }
    }
}
