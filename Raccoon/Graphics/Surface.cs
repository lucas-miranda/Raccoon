using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Surface {
        private static Surface PreparedSurface = null;

        private Vector2 _scale = Vector2.One;

        public Surface() {
            if (Game.Instance.Core.GraphicsDevice == null) throw new NoSuitableGraphicsDeviceException("Surface needs a valid graphics device. Maybe are you using before first Scene.Start() is called?");
            Projection = Matrix.CreateOrthographicOffCenter(0, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0, 1f, 0f);
            SpriteBatch = new SpriteBatch(Game.Instance.Core.GraphicsDevice);
        }

        public Vector2 Scale {
            get {
                return _scale;
            }

            set {
                _scale = value;
                View = Matrix.CreateScale(_scale.X, _scale.Y, 1);
            }
        }
        
        internal SpriteBatch SpriteBatch { get; private set; }
        internal Matrix World { get; set; } = Matrix.Identity;
        internal Matrix View { get; set; } = Matrix.Identity;
        internal Matrix Projection { get; set; } = Matrix.Identity;

        public void Draw(Texture texture, Vector2? position = default(Vector2?), Size? destinationSize = default(Size?), Rectangle? sourceRectangle = default(Rectangle?), Vector2? origin = default(Vector2?), float rotation = 0, Vector2? scale = default(Vector2?), Color? color = default(Color?), ImageFlip flip = ImageFlip.None, float layerDepth = 0) {
            Microsoft.Xna.Framework.Rectangle? destinationRect = null;
            if (destinationSize != null) {
                destinationRect = new Microsoft.Xna.Framework.Rectangle((int) position.Value.X, (int) position.Value.Y, (int) destinationSize.Value.Width, (int) destinationSize.Value.Height);
            }

            Prepare();
            SpriteBatch.Draw(texture.XNATexture, destinationSize != null ? null : position, destinationRect, sourceRectangle, origin, rotation, scale, color, (SpriteEffects) flip, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, ImageFlip flip, float layerDepth) {
            Prepare();
            SpriteBatch.Draw(texture.XNATexture, destinationRectangle, sourceRectangle, color, rotation, origin, (SpriteEffects) flip, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, ImageFlip flip, float layerDepth) {
            Prepare();
            SpriteBatch.DrawString(font.SpriteFont, text, position, color, rotation, origin, scale, (SpriteEffects) flip, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color) {
            Prepare();
            SpriteBatch.DrawString(font.SpriteFont, text, position, color);
        }
        
        internal void Prepare() {
            if (PreparedSurface == this) {
                return;
            }

            PreparedSurface = this;

            Game.Instance.Core.BasicEffect.World = World;
            Game.Instance.Core.BasicEffect.View = View;
            Game.Instance.Core.BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0, 0f, -1f);
            Game.Instance.Core.BasicEffect.TextureEnabled = true;

            foreach (EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }

            Game.Instance.Core.BasicEffect.TextureEnabled = false;
        }

        internal void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null) {
            SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, null);
        }

        internal void End() {
            SpriteBatch.End();
        }
    }
}
