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

        public System.Action OnBeforeRender, OnAfterRender;

        #endregion Public Members

        #region Private Members

        private static Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState> _blendstates = new Dictionary<BlendState, Microsoft.Xna.Framework.Graphics.BlendState>();

        private Size _previousProjectionSize;

        private Matrix _projection = Matrix.Identity;

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
        public Shader Shader { get; set; }
        public SpriteSortMode SpriteSortMode { get; set; } = SpriteSortMode.Texture;
        public Matrix World { get; set; } = Matrix.Identity;
        public Matrix View { get; set; } = Matrix.Identity;

        public Matrix Projection {
            get {
                return _projection;
            }

            set {
                _projection = value;
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal SpriteBatch SpriteBatch { get; private set; }
        internal SpriteSortMode LastSortMode { get; private set; }
        internal SamplerState LastSamplerState { get; private set; }
        internal DepthStencilState LastDepthStencilState { get; private set; }
        internal RasterizerState LastRasterizerState { get; private set; }
        internal Effect LastEffect { get; private set; }
        internal Matrix? LastTransform { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public virtual void BeforeRender() {
            OnBeforeRender?.Invoke();
        }

        public virtual void AfterRender() {
            OnAfterRender?.Invoke();
        }

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

        public Matrix RecalculateProjection() {
            Size size = RecalculateProjectionSize();

            if (size == _previousProjectionSize) {
                return _projection;
            }

            Matrix.CreateOrthographicOffCenter(0f, size.Width, size.Height, 0f, 0f, -1f, out _projection);
            _previousProjectionSize = size;

            return _projection;
        }

        #region Draw Texture on Destination Rectangle

        public void Draw(Canvas canvas, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.Draw(
                canvas.XNARenderTarget, 
                destinationRectangle,
                sourceRectangle, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                (SpriteEffects) flip, 
                0f
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.Draw(
                texture.XNATexture, 
                destinationRectangle,
                sourceRectangle, 
                color
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.Draw(
                texture.XNATexture, 
                destinationRectangle,
                color
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Canvas canvas, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.Draw(
                canvas.XNARenderTarget, 
                position, 
                sourceRectangle, 
                color, 
                Math.ToRadians(rotation), 
                origin, 
                scale, 
                (SpriteEffects) flip, 
                0f
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.Draw(
                texture.XNATexture, 
                position, 
                sourceRectangle, 
                color
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.Draw(
                texture.XNATexture, 
                position, 
                color
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null) {
            Draw(texture, position, color, Vector2.One, shader);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from String

        #region Draw Text from StringBuilder

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

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

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            if (SpriteSortMode == SpriteSortMode.Immediate) {
                BeforeRender();
            }

            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color
            );

            if (SpriteSortMode == SpriteSortMode.Immediate) {
                AfterRender();
            }
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from StringBuilder

        #endregion Public Methods

        #region Internal Methods

        internal void Begin(SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Matrix? transform = null) {
            LastSortMode = SpriteSortMode;
            LastSamplerState = samplerState;
            LastDepthStencilState = depthStencilState;
            LastRasterizerState = rasterizerState;
            LastEffect = Shader == null ? Game.Instance.BasicShader.XNAEffect : Shader.XNAEffect;
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
            BeforeRender();

            SpriteBatch.End();
            IsBatching = false;

            AfterRender();
        }

        #endregion Internal Methods
    }
}
