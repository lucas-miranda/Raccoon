using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    // TODO: Include a PrimitiveBatch and methods that suppports primitives drawing

    /// <summary>
    /// An all-in-one provider, containing means to draw anything that Raccoon.Graphics can offer.
    /// Also aims to centralize the rendering setups to make everything looks consistently.
    /// </summary>
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
        public BatchMode SpriteBatchMode { get; set; }
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

        /// <summary>
        /// Forces every batcher to render stored batches.
        /// </summary>
        /// <param name="reinitializeBatches">True, if reinitilizing batchers after flushing is intended, False otherwise.</param>
        public void Flush(bool reinitializeBatches = true) {
            if (!SpriteBatch.IsBatching) {
                return;
            }

            InternalFlush();

            if (reinitializeBatches) {
                Begin(SpriteBatch.BatchMode, SpriteBatch.BlendState, SpriteBatch.SamplerState, SpriteBatch.DepthStencilState, SpriteBatch.RasterizerState, SpriteBatch.Transform);
            }
        }

        #region Draw Texture on Destination Rectangle

        public void Draw(Canvas canvas, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(canvas.Texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(canvas.Texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, rotation, Vector2.One, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, destinationRectangle, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, destinationRectangle, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, destinationRectangle, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Rectangle destinationRectangle, Color color, Shader shader = null, float layerDepth = 1f) {
            Draw(texture, destinationRectangle, color, Vector2.One, shader, layerDepth);
        }

        #endregion Draw Texture on Destination Rectangle

        #region Draw Texture with Position

        public void Draw(Canvas canvas, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(canvas.Texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(canvas.Texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, position, sourceRectangle, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, position, sourceRectangle, rotation, new Vector2(scale), flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, position, sourceRectangle, rotation, new Vector2(scale), flip, color, origin, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Rectangle? sourceRectangle, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, position, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, position, sourceRectangle, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.Draw(texture, position, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.Draw(texture, position, null, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void Draw(Texture texture, Vector2 position, Color color, Shader shader = null, float layerDepth = 1f) {
            Draw(texture, position, color, Vector2.One, shader, layerDepth);
        }

        #endregion Draw Texture with Position

        #region Draw Text from String

        public void DrawString(Font font, string text, Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.DrawString(font, text, position, rotation, scale, flip, color, origin, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, float rotation, float scale, ImageFlip flip, Color color, Vector2 origin, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.DrawString(font, text, position, rotation, new Vector2(scale), flip, color, origin, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.DrawString(font, text, position, rotation, new Vector2(scale), flip, color, origin, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Vector2 scroll, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, scroll, shader, layerDepth);
        }

        public void DrawString(Font font, string text, Vector2 position, Color color, Shader shader = null, float layerDepth = 1f) {
            if (SpriteBatch.BatchMode == BatchMode.Immediate) {
                PrepareBeforeRender();
                SpriteBatch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, Vector2.One, shader, layerDepth);
                AfterRender();
                return;
            }

            SpriteBatch.DrawString(font, text, position, 0f, Vector2.One, ImageFlip.None, color, Vector2.Zero, Vector2.One, shader, layerDepth);
        }

        #endregion Draw Text from String

        #endregion Public Methods

        #region Protected Methods

        protected virtual void BeforeRender() {
            OnBeforeRender();
        }

        protected virtual void AfterRender() {
            OnAfterRender();
        }

        #endregion Protected Methods

        #region Private Methods

        public void InternalFlush() {
            if (!SpriteBatch.IsBatching) {
                return;
            }

            PrepareBeforeRender();
            SpriteBatch.End();
            AfterRender();
        }

        #endregion Private Methods

        #region Internal Methods

        internal void Begin(BatchMode? batchMode = null, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Matrix? transform = null) {
            SpriteBatch.Begin(
                batchMode ?? SpriteBatchMode,
                blendState ?? BlendState, 
                samplerState ?? SamplerState,
                depthStencilState ?? DepthStencilState,
                rasterizerState ?? RasterizerState,
                transform
            );
        }

        internal void End() {
            InternalFlush();
        }

        #endregion Internal Methods

        #region Private Methods

        private void PrepareBeforeRender() {
            if (!IsBatching) {
                throw new System.InvalidOperationException("SpriteBatch must be initialized and Begin() called previously.");
            }

            BeforeRender();

            if (Shader != null && SpriteBatch.Shader != Shader) {
                SpriteBatch.Shader = Shader;
            }

            // pass along transfom matrices
            if (SpriteBatch.Shader is IShaderTransform spriteBatch_Shader_Transform) {
                spriteBatch_Shader_Transform.World = World;
                spriteBatch_Shader_Transform.View = View;
                spriteBatch_Shader_Transform.Projection = Projection;
            }
        }

        #endregion Private Methods
    }
}
