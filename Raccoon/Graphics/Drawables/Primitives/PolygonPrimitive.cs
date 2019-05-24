using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Raccoon.Util;

namespace Raccoon.Graphics.Primitives {
    public class PolygonPrimitive : PrimitiveGraphic {
        #region Constructors

        public PolygonPrimitive(Polygon polygon, Color color) {
            Shape = polygon;
            Color = color;
        }

        public PolygonPrimitive(IEnumerable<Vector2> points, Color color) : this(new Polygon(points), color) { }

        #endregion Constructors

        #region Public Properties

        public Polygon Shape { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void Dispose() { }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (Shape.VertexCount == 0) {
                return;
            }

            VertexPositionColor[] vertices = new VertexPositionColor[Shape.VertexCount * 2];
            for (int i = 0; i < Shape.VertexCount; i++) {
                Vector2 vertex = Shape[i], nextVertex = Shape[(i + 1) % Shape.VertexCount];
                vertices[i * 2] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(vertex.X - Origin.X, vertex.Y - Origin.Y, layerDepth), Microsoft.Xna.Framework.Color.White);
                vertices[i * 2 + 1] = new VertexPositionColor(new Microsoft.Xna.Framework.Vector3(nextVertex.X - Origin.X, nextVertex.Y - Origin.Y, layerDepth), Microsoft.Xna.Framework.Color.White);
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateScale(Scale.X * scale.X, Scale.Y * scale.Y, 1f)
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0f)
                * Microsoft.Xna.Framework.Matrix.CreateRotationZ(Math.ToRadians(Rotation + rotation))
                * Microsoft.Xna.Framework.Matrix.CreateTranslation(Position.X + position.X, Position.Y + position.Y, 0f)
                * Renderer.World;

            bs.View = Renderer.View;
            bs.Projection = Renderer.Projection;

            // material
            bs.SetMaterial(color * Color, Opacity);
            bs.TextureEnabled = false;

            GraphicsDevice device = Game.Instance.GraphicsDevice;

            // we need to manually update every GraphicsDevice states here
            device.BlendState = Renderer.SpriteBatch.BlendState;
            device.SamplerStates[0] = Renderer.SpriteBatch.SamplerState;
            device.DepthStencilState = Renderer.SpriteBatch.DepthStencilState;
            device.RasterizerState = Renderer.SpriteBatch.RasterizerState;

            foreach (object pass in bs) {
                device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, Shape.VertexCount);
            }

            bs.ResetParameters();
        }

        #endregion Protected Methods
    }
}
