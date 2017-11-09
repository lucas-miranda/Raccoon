namespace Raccoon.Graphics.Primitives {
    public class LineShape : Graphic {
        private Vector2 _to;

        public LineShape(Vector2 from, Vector2 to, Color color) {
            From = from;
            To = To;
            Color = color;
        }

        public LineShape(Vector2 length, Color color) {
            From = Vector2.Zero;
            To = length;
            Color = color;
        }

        public Vector2 From { get { return Position - Origin; } set { Position = value + Origin; } }
        public Vector2 To { get { return From + _to; } set { _to = value - From; } }
        public Line Equation { get { return new Line(From, To); } }

        public override void Render(Vector2 position, Color color, float rotation) {
            color *= Opacity;
            Game.Instance.Core.BasicEffect.World = Surface.World;
            Game.Instance.Core.BasicEffect.View = Surface.View;
            Game.Instance.Core.BasicEffect.Projection = Surface.Projection;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(position.X - Origin.X, position.Y - Origin.Y, 0), color),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(position.X - Origin.X + _to.X, position.Y - Origin.Y + _to.Y, 0), color)
                }, 0, 1);

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.BasicEffect.View = Game.Instance.Core.BasicEffect.Projection = Microsoft.Xna.Framework.Matrix.Identity;
        }

        public override void Dispose() { }
    }
}
