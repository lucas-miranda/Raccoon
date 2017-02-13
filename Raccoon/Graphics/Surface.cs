using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Surface {
        private Vector2 _scale = Vector2.One;

        public Surface() {
            if (Game.Instance.Core.GraphicsDevice == null) throw new NoSuitableGraphicsDeviceException("Surface needs a valid graphics device. Maybe are you using before first Scene.Start() is called?");
            Projection = Matrix.CreateOrthographicOffCenter(0f, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0f, 1f, 0f);
            SpriteBatch = new SpriteBatch(Game.Instance.Core.GraphicsDevice);
        }

        public Vector2 Scale {
            get {
                return _scale;
            }

            set {
                _scale = value;
                View = Matrix.CreateScale(_scale.X, _scale.Y, 1f);
            }
        }
        
        internal SpriteBatch SpriteBatch { get; private set; }
        internal Matrix World { get; set; } = Matrix.Identity;
        internal Matrix View { get; set; } = Matrix.Identity;
        internal Matrix Projection { get; set; } = Matrix.Identity;

        public void Draw(Texture texture, Vector2? position = default(Vector2?), Size? destinationSize = default(Size?), Rectangle? sourceRectangle = default(Rectangle?), Vector2? origin = default(Vector2?), float rotation = 0, Vector2? scale = default(Vector2?), Color? color = default(Color?), Vector2? scrollFactor = default(Vector2?), ImageFlip flip = ImageFlip.None) {
            Microsoft.Xna.Framework.Rectangle? destinationRect = null;
            if (destinationSize != null) {
                destinationRect = new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationSize.Value.Width, (int) destinationSize.Value.Height);
            }

            Prepare(position.Value, scrollFactor.Value);
            SpriteBatch.Draw(texture.XNATexture, destinationRect != null ? null : new Microsoft.Xna.Framework.Vector2?(Microsoft.Xna.Framework.Vector2.Zero), destinationRect, sourceRectangle, origin, rotation, scale, color, (SpriteEffects) flip, 0f);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scrollFactor, float rotation, Vector2 origin, ImageFlip flip) {
            Prepare(destinationRectangle.Position, scrollFactor);
            SpriteBatch.Draw(texture.XNATexture, new Rectangle(Vector2.Zero, destinationRectangle.Size), sourceRectangle, color, rotation, origin, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scrollFactor, float rotation, Vector2 origin, Vector2 scale, ImageFlip flip) {
            Prepare(position, scrollFactor);
            SpriteBatch.DrawString(font.SpriteFont, text, Vector2.Zero, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scrollFactor) {
            Prepare(position, scrollFactor);
            SpriteBatch.DrawString(font.SpriteFont, text, Vector2.Zero, color);
        }
        
        internal void Prepare(Vector2 position, Vector2 scrollFactor) {
            Game.Instance.Core.BasicEffect.World = Matrix.CreateTranslation(position.X * scrollFactor.X, position.Y * scrollFactor.Y, 0f) * World;
            Game.Instance.Core.BasicEffect.View = Matrix.CreateScale(1f / scrollFactor.X, 1f / scrollFactor.Y, 1f) * View * Matrix.CreateScale(scrollFactor.X, scrollFactor.Y, 1f);
            Game.Instance.Core.BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0f, 0f, -1f);
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
