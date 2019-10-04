using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class SpriteBatchItem : IBatchItem {
        #region Public Properties

        public Texture Texture { get; private set; }
        public Shader Shader { get; private set; }
        public IShaderParameters ShaderParameters { get; private set; }
        public VertexPositionColorTexture[] VertexData { get; private set; } = new VertexPositionColorTexture[4];
        public int[] IndexData { get; private set; } = new int[6] { 3, 0, 2, 2, 0, 1 };

        #endregion Public Properties

        #region Public Methods

        public void Set(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            Texture = texture;
            Shader = shader;
            ShaderParameters = shaderParameters;

            if (!sourceRectangle.HasValue) {
                sourceRectangle = new Rectangle(texture.Size);
            }

            Vector2 topLeft = -origin * scale,
                    topRight = (-origin + new Vector2(destinationRectangle.Width, 0f)) * scale,
                    bottomRight = (-origin + destinationRectangle.Size) * scale,
                    bottomLeft = (-origin + new Vector2(0f, destinationRectangle.Height)) * scale;

            if (rotation != 0) {
                float cos = Math.Cos(rotation),
                      sin = Math.Sin(rotation);

                topLeft = new Vector2(topLeft.X * cos - topLeft.Y * sin, topLeft.X * sin + topLeft.Y * cos);
                topRight = new Vector2(topRight.X * cos - topRight.Y * sin, topRight.X * sin + topRight.Y * cos);
                bottomRight = new Vector2(bottomRight.X * cos - bottomRight.Y * sin, bottomRight.X * sin + bottomRight.Y * cos);
                bottomLeft = new Vector2(bottomLeft.X * cos - bottomLeft.Y * sin, bottomLeft.X * sin + bottomLeft.Y * cos);
            }

            VertexData[0] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.TopLeft + topLeft, layerDepth),
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Left / texture.Width, sourceRectangle.Value.Top / texture.Height)
            );

            VertexData[1] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.TopLeft + topRight, layerDepth),
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Right / texture.Width, sourceRectangle.Value.Top / texture.Height)
            );

            VertexData[2] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.TopLeft + bottomRight, layerDepth),
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Right / texture.Width, sourceRectangle.Value.Bottom / texture.Height)
            );

            VertexData[3] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.TopLeft + bottomLeft, layerDepth),
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Left / texture.Width, sourceRectangle.Value.Bottom / texture.Height)
            );

            if ((flip & ImageFlip.Horizontal) != ImageFlip.None) {
                Microsoft.Xna.Framework.Vector2 texCoord = VertexData[1].TextureCoordinate;
                VertexData[1].TextureCoordinate = VertexData[0].TextureCoordinate;
                VertexData[0].TextureCoordinate = texCoord;

                texCoord = VertexData[2].TextureCoordinate;
                VertexData[2].TextureCoordinate = VertexData[3].TextureCoordinate;
                VertexData[3].TextureCoordinate = texCoord;
            }

            if ((flip & ImageFlip.Vertical) != ImageFlip.None) {
                Microsoft.Xna.Framework.Vector2 texCoord = VertexData[2].TextureCoordinate;
                VertexData[2].TextureCoordinate = VertexData[1].TextureCoordinate;
                VertexData[1].TextureCoordinate = texCoord;

                texCoord = VertexData[3].TextureCoordinate;
                VertexData[3].TextureCoordinate = VertexData[0].TextureCoordinate;
                VertexData[0].TextureCoordinate = texCoord;
            }
        }

        public void Set(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            Set(texture, destinationRectangle, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters: null, layerDepth);
        }

        public void Set(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, float layerDepth = 1f) {
            if (!sourceRectangle.HasValue) {
                sourceRectangle = new Rectangle(texture.Size);
            }

            Rectangle destinationRectagle = new Rectangle(position, sourceRectangle.Value.Size);
            Set(texture, destinationRectagle, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters, layerDepth);
        }

        public void Set(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            Set(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, shaderParameters: null, layerDepth);
        }

        public void Clear() {
            Texture = null;
            Shader = null;
            ShaderParameters = null;
            //VertexData = null;
        }

        #endregion Public Methods
    }
}
