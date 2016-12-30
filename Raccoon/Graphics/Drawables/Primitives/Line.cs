namespace Raccoon.Graphics.Primitives {
    public class Line : Graphic {
        private Vector2 _to;

        public Line(Vector2 from, Vector2 to, Color color) {
            From = from;
            To = To;
            Color = color;
        }

        public Line(Vector2 length, Color color) {
            From = Vector2.Zero;
            To = length;
            Color = color;
        }

        public Vector2 From { get { return Position - Origin; } set { Position = value + Origin; } }
        public Vector2 To { get { return From + _to; } set { _to = value - From; } }
        public Raccoon.Line Equation { get { return new Raccoon.Line(From, To); } }

        public override void Render(Vector2 position, float rotation) {
            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(position.X - Origin.X, position.Y - Origin.Y, 0), Color),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(position.X - Origin.X + _to.X, position.Y - Origin.Y + _to.Y, 0), Color)
                }, 0, 1);
        }

        public override void Dispose() { }
    }
}
