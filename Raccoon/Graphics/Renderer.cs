using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Raccoon.Util;

namespace Raccoon.Graphics {
    public enum BlendState {
        AlphaBlend,
        Additive,
        NonPremultiplied,
        Opaque
    }

    public class Renderer {
        #region Private Members

        private static Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState> _blendstates = new Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState>();

        #endregion Private Members

        #region Constructors

        static Renderer() {
            _blendstates.Add(BlendState.AlphaBlend, Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend);
            _blendstates.Add(BlendState.Additive, Microsoft.Xna.Framework.Graphics.BlendState.Additive);
            _blendstates.Add(BlendState.NonPremultiplied, Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied);
            _blendstates.Add(BlendState.Opaque, Microsoft.Xna.Framework.Graphics.BlendState.Opaque);
        }

        public Renderer(BlendState blendState = BlendState.AlphaBlend) {
            if (Game.Instance.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Renderer needs a valid graphics device. Maybe are you using before first Scene.Start() is called?");
            }

            BlendState = blendState;
            SpriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
            Projection = Matrix.CreateOrthographicOffCenter(0f, Game.Instance.WindowWidth, Game.Instance.WindowHeight, 0f, 0f, 1f);
        }

        #endregion Constructors

        #region Public Properties

        public BlendState BlendState { get; private set; }
        public int UnitToPixels { get; set; } = 1;
        public float PixelScale { get; set; } = 1f;

        #endregion Public Properties

        #region Internal Properties

        internal SpriteBatch SpriteBatch { get; private set; }
        internal Matrix World { get; set; } = Matrix.Identity;
        internal Matrix View { get; set; } = Matrix.Identity;
        internal Matrix Projection { get; set; } = Matrix.Identity;

        #endregion Internal Properties

        #region Public Methods

        public Vector2 ConvertScreenToWorld(Vector2 screenPosition) { 
            Vector3 worldPos = Game.Instance.GraphicsDevice.Viewport.Unproject( 
                new Vector3(screenPosition, 0f), 
                Projection, 
                View, 
                Matrix.Identity
            ); 
 
            return new Vector2(worldPos.X, worldPos.Y); 
        } 
 
        public Vector2 ConvertWorldToScreen(Vector2 worldPosition) { 
            Vector3 screenPos = Game.Instance.GraphicsDevice.Viewport.Project( 
                new Vector3(worldPosition, 0f), 
                Projection, 
                View, 
                Matrix.Identity
            ); 
 
            return new Vector2(screenPos.X, screenPos.Y); 
        } 

        #region Draw Texture on Destination Rectangle

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, Math.ToRadians(rotation), new Vector2(PixelScale), origin, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), 
                sourceRectangle, 
                color, 
                0f, 
                Vector2.Zero, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, 0f, new Vector2(PixelScale), Vector2.Zero, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), 
                sourceRectangle, 
                color
            );
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(destinationRectangle.Position, 0f, new Vector2(PixelScale), Vector2.Zero, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                new Microsoft.Xna.Framework.Rectangle(0, 0, (int) destinationRectangle.Width, (int) destinationRectangle.Height), 
                color
            );
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, Math.ToRadians(rotation), scale * PixelScale, origin, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                sourceRectangle, 
                color, 
                0f, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                Microsoft.Xna.Framework.Vector2.One, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, Math.ToRadians(rotation), new Vector2(scale * PixelScale), origin, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                sourceRectangle, 
                color, 
                0f, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                1f, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, 0f, new Vector2(PixelScale), Vector2.Zero, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                sourceRectangle, 
                color
            );
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, 0f, new Vector2(PixelScale), Vector2.Zero, scroll, shader);
            SpriteBatch.Draw(
                texture.XNATexture, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color
            );
        }

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null) {
            Draw(texture, position, color, Vector2.One, shader);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, Math.ToRadians(rotation), scale * PixelScale, origin, scroll, shader);
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color, 
                0f, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                Microsoft.Xna.Framework.Vector2.One, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, Math.ToRadians(rotation), new Vector2(scale * PixelScale), origin, scroll, shader);
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color, 
                0f, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                1f, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, 0f, new Vector2(PixelScale), Vector2.Zero, scroll, shader);
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color
            );
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from String

        #region Draw Text from StringBuilder

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, Math.ToRadians(rotation), scale * PixelScale, origin, scroll, shader);
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color, 
                0f, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                Microsoft.Xna.Framework.Vector2.One, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            Prepare(position, Math.ToRadians(rotation), new Vector2(scale * PixelScale), origin, scroll, shader);
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color, 
                0f, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                1f, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            Prepare(position, 0f, new Vector2(PixelScale), Vector2.Zero, scroll, shader);
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                color
            );
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from StringBuilder

        #endregion Public Methods

        #region Internal Methods

        internal void Prepare(Vector2 position, float rotation, Vector2 scale, Vector2 origin, Vector2 scrollFactor, Shader shader = null) {
            Vector2 scroll = scrollFactor;

            // HACK: to ensure graphic will be rendered
            if (scroll.X == 0f && scroll.Y == 0f) {
                scroll = new Vector2(Math.Epsilon);
            }

            //position = Math.Round(position); // HACK: Do this using a pixel-perfect shader

            //Matrix scrollMatrix = Matrix.CreateScale(scroll.X, scroll.Y, 1f);

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            scale = new Vector2(scale.X / (UnitToPixels * PixelScale), scale.Y / (UnitToPixels * PixelScale));
            bs.World = Matrix.CreateScale(scale.X, scale.Y, 1f)
                * Matrix.CreateTranslation(-origin.X * scale.X, -origin.Y * scale.Y, 0f)
                * Matrix.CreateRotationZ(rotation)
                * Matrix.CreateTranslation(position.X, position.Y, 0f) 
                * World;

            //effect.View = Matrix.Invert(scrollMatrix) * View * scrollMatrix;
            bs.View = View;

            bs.Projection = Projection;
                //* Matrix.CreateScale(1f / Scale.X, 1f / Scale.Y, 1f);

            bs.TextureEnabled = true;
            bs.Apply();
            bs.ResetParameters();

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
