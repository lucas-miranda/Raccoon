﻿namespace Raccoon.Graphics.Primitives {
    public class LinePrimitive : Graphic {
        private Vector2 _to;

        public LinePrimitive(Vector2 from, Vector2 to, Color color) {
            From = from;
            To = To;
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
            Game.Instance.Core.BasicEffect.World = Surface.World;
            Game.Instance.Core.BasicEffect.View = Surface.View;
            Game.Instance.Core.BasicEffect.Projection = Surface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = Opacity;

            foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                        new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Position.X + position.X - Origin.X, Position.Y + position.Y - Origin.Y, 0), Microsoft.Xna.Framework.Color.White),
                        new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Position.X + position.X - Origin.X + _to.X, Position.Y + position.Y - Origin.Y + _to.Y, 0), Microsoft.Xna.Framework.Color.White)
                    }, 0, 1);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f);
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.BasicEffect.View = Game.Instance.Core.BasicEffect.Projection = Microsoft.Xna.Framework.Matrix.Identity;
        }

        public override void Dispose() { }
    }
}
