using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Renderer {
        #region Public Members

        public static readonly System.Func<Size> DefaultRecalculateProjectionSize = () => Game.Instance.WindowSize;

        public System.Func<Size> RecalculateProjectionSize = DefaultRecalculateProjectionSize;

        public System.Action OnBeforeRender = delegate { },
                             OnAfterRender = delegate { };

        #endregion Public Members

        #region Private Members

        private Size _previousProjectionSize;

        private Matrix _projection = Matrix.Identity;

        #endregion Private Members

        #region Constructors

        public Renderer(bool autoHandleAlphaBlendedSprites = false) {
            if (Game.Instance.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Renderer needs a valid graphics device. Maybe are you using before first Scene.Start() is called?");
            }

            SpriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice, autoHandleAlphaBlendedSprites);
            RecalculateProjection();
        }

        #endregion Constructors

        #region Public Properties

        public bool IsBatching { get { return SpriteBatch.IsBatching; } }
        public SpriteBatch SpriteBatch { get; private set; }
        public BlendState BlendState { get; set; }
        public SamplerState SamplerState { get; set; }
        public DepthStencilState DepthStencilState { get; set; }
        public RasterizerState RasterizerState { get; set; }
        public Shader Shader { get; set; }
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

        #region Public Methods

        public virtual void BeforeRender() {
            OnBeforeRender();
        }

        public virtual void AfterRender() {
            OnAfterRender();
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

        public ref readonly Matrix RecalculateProjection() {
            Size size = RecalculateProjectionSize();

            if (size == _previousProjectionSize) {
                return ref _projection;
            }

            Matrix.CreateOrthographicOffCenter(0f, size.Width, size.Height, 0f, 0f, 1f, out _projection);
            _previousProjectionSize = size;

            return ref _projection;
        }

        #region Draw Texture on Destination Rectangle

        public void Draw(Canvas canvas, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(canvas.Texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, destinationRectangle, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null, float layerDepth = 1f) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader, layerDepth);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Canvas canvas, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(canvas.Texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, position, sourceRectangle, rotation, new Vector2(scale), flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, position, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.Draw(texture, position, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null, float layerDepth = 1f) {
            Draw(texture, position, color, Vector2.One, shader, layerDepth);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.DrawString(font, text, position, rotation, new Vector2(scale), flip, color, origin, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null, float layerDepth = 1f) {
            SpriteBatch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, Vector2.One, shader, layerDepth);
        }

        #endregion Draw Text from String

        #region Draw Text from StringBuilder

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            /*
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
            */
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null) {
            /*
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
            */
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Vector2 scroll, Shader shader = null) {
            /*
            SpriteBatch.DrawString(
                font.SpriteFont, 
                text, 
                position, 
                color
            );
            */
        }

        public void DrawString(Font font, StringBuilder text, Vector2 position, Color color, Shader shader = null) {
            DrawString(font, text, position, color, Vector2.One, shader);
        }

        #endregion Draw Text from StringBuilder

        #endregion Public Methods

        #region Internal Methods

        internal void Begin(BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Matrix? transform = null) {
            SpriteBatch.Begin(
                blendState ?? BlendState, 
                samplerState ?? SamplerState,
                depthStencilState ?? DepthStencilState,
                rasterizerState ?? RasterizerState,
                transform
            );
        }

        internal void End() {
            BeforeRender();

            if (Shader != null && SpriteBatch.Shader != Shader) {
                SpriteBatch.Shader = Shader;
            }

            // pass along transfom matrices
            if (SpriteBatch.Shader is IBasicShader spriteBatchBasicShader) {
                spriteBatchBasicShader.World = World;
                spriteBatchBasicShader.View = View;
                spriteBatchBasicShader.Projection = Projection;
            }

            SpriteBatch.End();

            AfterRender();

            if (SpriteBatch.Shader is BasicShader batch) {
                batch.ResetParameters();
            }
        }

        #endregion Internal Methods
    }
}
