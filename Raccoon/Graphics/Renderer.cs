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
        #region Public Members

        public static readonly System.Func<Size> DefaultRecalculateProjectionSize = () => Game.Instance.WindowSize;

        public System.Func<Size> RecalculateProjectionSize = DefaultRecalculateProjectionSize;

        #endregion Public Members

        #region Private Members

        private static Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState> _blendstates = new Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState>();

        private Size _previousProjectionSize;

        private Matrix _world = Matrix.Identity, 
                       _view = Matrix.Identity, 
                       _projection = Matrix.Identity;

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
            RecalculateProjection();
        }

        #endregion Constructors

        #region Public Properties

        public BlendState BlendState { get; private set; }
        public bool IsBatching { get; private set; }

        #endregion Public Properties

        #region Internal Properties

        internal SpriteBatch SpriteBatch { get; private set; }
        internal SpriteSortMode LastSortMode { get; private set; }
        internal SamplerState LastSamplerState { get; private set; }
        internal DepthStencilState LastDepthStencilState { get; private set; }
        internal RasterizerState LastRasterizerState { get; private set; }
        internal Effect LastEffect { get; private set; }
        internal Matrix? LastTransform { get; private set; }

        internal Matrix World {
            get {
                return _world;
            }

            set {
                _world = value;
            }
        }

        internal Matrix View {
            get {
                return _view;
            }

            set {
                _view = value;
            }
        }

        internal Matrix Projection {
            get {
                return _projection;
            }

            set {
                _projection = value;
            }
        }

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

        public void RecalculateProjection() {
            Size size = RecalculateProjectionSize();

            if (size == _previousProjectionSize) {
                return;
            }

            Matrix.CreateOrthographicOffCenter(0f, size.Width, size.Height, 0f, 0f, -1f, out _projection);
            _previousProjectionSize = size;
        }

        #region Draw Texture on Destination Rectangle

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                destinationRectangle,
                sourceRectangle, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                destinationRectangle,
                sourceRectangle, 
                color
            );
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                destinationRectangle,
                color
            );
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                position, 
                sourceRectangle, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                position, 
                sourceRectangle, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                position, 
                sourceRectangle, 
                color
            );
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            SpriteBatch.Draw(
                texture.XNATexture, 
                position, 
                color
            );
        }

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null) {
            Draw(texture, position, color, Vector2.One, shader);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color
            );
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from String

        #region Draw Text from StringBuilder

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color
            );
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from StringBuilder

        #endregion Public Methods

        #region Internal Methods

        internal void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transform = null) {
            LastSortMode = sortMode;
            LastSamplerState = samplerState;
            LastDepthStencilState = depthStencilState;
            LastRasterizerState = rasterizerState;
            LastEffect = effect;
            LastTransform = transform;

            Begin();
        }

        internal void Begin() {
            IsBatching = true;
            SpriteBatch.Begin(
                LastSortMode,
                _blendstates[BlendState], 
                LastSamplerState,
                LastDepthStencilState,
                LastRasterizerState,
                LastEffect,
                LastTransform
            );
        }

        internal void End() {
            SpriteBatch.End();

            IsBatching = false;
        }

        #endregion Internal Methods
    }
}
