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
        #region Private Members

        private static Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState> _blendstates = new Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState>();

        private Vector2 _scale = Vector2.One;

        #endregion Private Members

        #region Constructors

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

        #endregion Constructors

        #region Public Properties

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

        #endregion Public Properties

        #region Internal Properties

        internal SpriteBatch SpriteBatch { get; private set; }
        internal Matrix World { get; set; } = Matrix.Identity;
        internal Matrix View { get; set; } = Matrix.Identity;
        internal Matrix Projection { get; set; } = Matrix.Identity;

        #endregion Internal Properties

        #region Public Methods

        public Vector2 Transform(Vector2 position, Surface surface) {
            return (position * surface.Scale) / Scale;
        }

        #region Draw Texture on Destination Rectangle

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), color);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), sourceRectangle, color);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), sourceRectangle, color, Util.Math.ToRadians(rotation), origin, (SpriteEffects) flip, 0f);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null) {
            Draw(texture, position, color, Vector2.One, shader);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, color);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, sourceRectangle, color);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, sourceRectangle, color, Util.Math.ToRadians(rotation), origin, scale, (SpriteEffects) flip, 0f);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.Draw(texture.XNATexture, Microsoft.Xna.Framework.Vector2.Zero, sourceRectangle, color, Util.Math.ToRadians(rotation), origin, scale, (SpriteEffects) flip, 0f);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, Util.Math.ToRadians(rotation), origin, scale, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, Util.Math.ToRadians(rotation), origin, scale, (SpriteEffects) flip, 0f);
        }

        #endregion Draw Text from String

        #region Draw Text from StringBuilder

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, Util.Math.ToRadians(rotation), origin, scale, (SpriteEffects) flip, 0f);
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, scroll, shader);
            SpriteBatch.DrawString(font.SpriteFont, text, Microsoft.Xna.Framework.Vector2.Zero, color, Util.Math.ToRadians(rotation), origin, scale, (SpriteEffects) flip, 0f);
        }

        #endregion Draw Text from StringBuilder

        #endregion Public Methods

        #region Internal Methods

        internal void Prepare(Vector2 position, Vector2 scrollFactor, Shader shader = null) {
            Vector2 scroll = scrollFactor;

            // HACK: to ensure graphic will be rendered
            if (scroll.X == 0f && scroll.Y == 0f) {
                scroll = new Vector2(Util.Math.Epsilon);
            }

            position = new Vector2(System.Math.Round(position.X), System.Math.Round(position.Y)); // HACK: Do this using a pixel-perfect shader

            Matrix scrollMatrix = Matrix.CreateScale(scroll.X, scroll.Y, 1f);
            Game.Instance.Core.BasicEffect.World = Matrix.CreateTranslation(position.X, position.Y, 0f) * World;
            Game.Instance.Core.BasicEffect.View = Matrix.Invert(scrollMatrix) * View * scrollMatrix;
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

        #endregion Internal Methods
    }
}
