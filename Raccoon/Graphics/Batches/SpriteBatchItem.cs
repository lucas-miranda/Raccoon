using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public class SpriteBatchItem : System.IComparable<SpriteBatchItem> {
        #region Public Properties

        public Texture Texture { get; set; }
        public Shader Shader { get; set; }
        public VertexPositionColorTexture[] VertexData { get; private set; } = new VertexPositionColorTexture[4];

        #endregion Public Properties

        #region Public Methods

        public void Set(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            Texture = texture;
            Shader = shader;

            if (!sourceRectangle.HasValue) {
                sourceRectangle = new Rectangle(texture.Size);
            }

            float cos = Math.Cos(rotation),
                  sin = Math.Sin(rotation);

            Vector2 topLeftOrigin = -origin * scale,
                    topRightOrigin = (-origin + new Vector2(destinationRectangle.Width, 0f)) * scale,
                    bottomRightOrigin = (-origin + destinationRectangle.Size) * scale,
                    bottomLeftOrigin = (-origin + new Vector2(0f, destinationRectangle.Height)) * scale;

            VertexData[0] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.X + (topLeftOrigin.X * cos - topLeftOrigin.Y * sin), destinationRectangle.Y + (topLeftOrigin.X * sin + topLeftOrigin.Y * cos), layerDepth), 
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Left / texture.Width, sourceRectangle.Value.Top / texture.Height)
            );

            VertexData[1] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.X + (topRightOrigin.X * cos - topRightOrigin.Y * sin), destinationRectangle.Y + (topRightOrigin.X * sin + topRightOrigin.Y * cos), layerDepth), 
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Right / texture.Width, sourceRectangle.Value.Top / texture.Height)
            );

            VertexData[2] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.X + (bottomRightOrigin.X * cos - bottomRightOrigin.Y * sin), destinationRectangle.Y + (bottomRightOrigin.X * sin + bottomRightOrigin.Y * cos), layerDepth), 
                color,
                new Microsoft.Xna.Framework.Vector2(sourceRectangle.Value.Right / texture.Width, sourceRectangle.Value.Bottom / texture.Height)
            );

            VertexData[3] = new VertexPositionColorTexture(
                new Vector3(destinationRectangle.X + (bottomLeftOrigin.X * cos - bottomLeftOrigin.Y * sin), destinationRectangle.Y + (bottomLeftOrigin.X * sin + bottomLeftOrigin.Y * cos), layerDepth), 
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

        public void Set(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (!sourceRectangle.HasValue) {
                sourceRectangle = new Rectangle(texture.Size);
            }

            Rectangle destinationRectagle = new Rectangle(position, sourceRectangle.Value.Size);
            Set(texture, destinationRectagle, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public int CompareTo(SpriteBatchItem other) {
            if (Shader == other.Shader) {
                return 0;
            } else if (Shader == null) {
                return -1;
            } else if (other.Shader == null) {
                return 1;
            }

            return Shader.Id.CompareTo(other.Shader.Id);
        }

        #endregion Public Methods
    }
}
