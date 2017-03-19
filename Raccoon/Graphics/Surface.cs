using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public enum BlendState {
        AlphaBlend,
        Additive,
        NonPremultiplied,
        Opaque
    }

    public class Surface {
        private static Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState> _blendstates = new Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState>();

        private Vector2 _scale = Vector2.One;

        static Surface() {
            _blendstates.Add(BlendState.AlphaBlend, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend);
            _blendstates.Add(BlendState.Additive, Microsoft.Xna.Framework.Graphics.BlendState.Additive);
            _blendstates.Add(BlendState.NonPremultiplied, Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied);
            _blendstates.Add(BlendState.Opaque, Microsoft.Xna.Framework.Graphics.BlendState.Opaque);
        }

        public Surface(BlendState blendState = BlendState.AlphaBlend) {
            if (Game.Instance.Core.GraphicsDevice == null) throw new NoSuitableGraphicsDeviceException("Surface needs a valid graphics device. Maybe are you using before first Scene.Start() is called?");

            BlendState = blendState;
            Projection = Matrix.CreateOrthographicOffCenter(0f, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0f, 1f, 0f);
            SpriteBatch = new SpriteBatch(Game.Instance.Core.GraphicsDevice);
        }

        public BlendState BlendState { get; private set; }

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

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), color);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, color);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, sourceRectangle, color);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), sourceRectangle, color);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), sourceRectangle, color, rotation, origin, (SpriteEffects) flip, 0f);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, sourceRectangle, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, sourceRectangle, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }
        
        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, ImageFlip flip, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, rotation, origin, scale, (SpriteEffects) flip, 0f);
        }

        internal void Prepare(Vector2 position, Vector2 scrollFactor, Shader shader) {
            Game.Instance.Core.BasicEffect.World = Matrix.CreateTranslation(position.X * scrollFactor.X, position.Y * scrollFactor.Y, 0f) * World;
            Game.Instance.Core.BasicEffect.View = Matrix.CreateScale(1f / scrollFactor.X, 1f / scrollFactor.Y, 1f) * View * Matrix.CreateScale(scrollFactor.X, scrollFactor.Y, 1f);
            Game.Instance.Core.BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0f, 0f, -1f);
            Game.Instance.Core.BasicEffect.TextureEnabled = true;

            foreach (EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }

            Game.Instance.Core.BasicEffect.TextureEnabled = false;

            if (shader != null) {
                shader.Apply();
            }
        }

        internal void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null) {
            SpriteBatch.Begin(sortMode, _blendstates[BlendState], samplerState, depthStencilState, rasterizerState, effect, null);
        }

        internal void End() {
            SpriteBatch.End();
        }
    }
}
