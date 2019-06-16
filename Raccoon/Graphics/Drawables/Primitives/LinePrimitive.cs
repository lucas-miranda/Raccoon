using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitives {
    public class LinePrimitive : PrimitiveGraphic {
        #region Private Members

        private Vector2 _to;

        #endregion Private Members

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

        public Vector2 From { get { return Position - Origin; } set { Position = value + Origin; } }
        public Vector2 To { get { return From + _to; } set { _to = value - From; } }
        public Line Equation { get { return new Line(From, To); } }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() { }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            origin = Origin + origin;

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Renderer.World;
            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.SetMaterial(color * Color, Opacity);
            bs.TextureEnabled = false;

            shaderParameters?.ApplyParameters(shader);

            GraphicsDevice device = Game.Instance.GraphicsDevice;

            // we need to manually update every GraphicsDevice states here
            device.BlendState = Renderer.SpriteBatch.BlendState;
            device.SamplerStates[0] = Renderer.SpriteBatch.SamplerState;
            device.DepthStencilState = Renderer.SpriteBatch.DepthStencilState;
            device.RasterizerState = Renderer.SpriteBatch.RasterizerState;

            foreach (object pass in bs) {
                device.DrawUserPrimitives(PrimitiveType.LineList,
                    new VertexPositionColor[2] {
                        new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Position.X + position.X - origin.X, Position.Y + position.Y - origin.Y, layerDepth), Microsoft.Xna.Framework.Color.White),
                        new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(Position.X + position.X - origin.X + _to.X, Position.Y + position.Y - origin.Y + _to.Y, layerDepth), Microsoft.Xna.Framework.Color.White)
                    }, 0, 1);
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods
    }
}
