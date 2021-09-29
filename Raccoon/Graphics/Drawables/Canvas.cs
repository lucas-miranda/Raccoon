using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Canvas : Image {
        #region Private Members

        private Renderer _internalRenderer;

        #endregion Private Members

        #region Constructors

        public Canvas(int width, int height)
          : base(new Texture(new RenderTarget2D(Game.Instance.GraphicsDevice, width, height))) {
        }

        public Canvas(int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat)
          : base(new Texture(new RenderTarget2D(Game.Instance.GraphicsDevice, width, height, mipMap, surfaceFormat, depthFormat))) {
        }

        public Canvas(int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int multiSampleCount, RenderTargetUsage usage)
          : base(new Texture(new RenderTarget2D(Game.Instance.GraphicsDevice, width, height, mipMap, surfaceFormat, depthFormat, multiSampleCount, usage))) {
        }

        #endregion Constructors

        #region Public Properties

        public DepthFormat DepthStencilFormat { get { return XNARenderTarget.DepthStencilFormat; } }
        public int MultiSampleCount { get { return XNARenderTarget.MultiSampleCount; } }
        public RenderTargetUsage Usage { get { return XNARenderTarget.RenderTargetUsage; } }
        public RenderTarget2D XNARenderTarget { get { return Texture.XNATexture as RenderTarget2D; } }

        public Renderer InternalRenderer {
            get {
                return _internalRenderer;
            }

            set {
                if (value == _internalRenderer) {
                    return;
                }

                if (value != null) {
                    if (DepthStencilFormat == DepthFormat.None) {
                        if (value.SpriteBatchMode == BatchMode.DepthBuffer || value.SpriteBatchMode == BatchMode.DepthBufferDescending) {
                            throw new System.ArgumentException($"Canvas isn't prepared to handle batch mode using depth buffer, depth format shouldn't be DepthFormat.None in this case.");
                        }

                        if (value.DepthStencilState != DepthStencilState.None) {
                            throw new System.ArgumentException($"Canvas isn't prepared to handle depth read, depth format shouldn't be DepthFormat.None in this case.");
                        }
                    }
                }

                _internalRenderer = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Begin(Color? clearColor = null) {
            Game.Instance.GraphicsDevice.SetRenderTarget(XNARenderTarget);
            Game.Instance.RenderTargetStack.Push(XNARenderTarget);

            if (clearColor.HasValue) {
                Clear(clearColor.Value);
            }

            InternalRenderer?.Begin();
        }

        public void End() {
            InternalRenderer?.End();
            Game.Instance.RenderTargetStack.Pop();

            if (Game.Instance.RenderTargetStack.Count == 0) {
                Game.Instance.GraphicsDevice.SetRenderTarget(null);
            } else {
                Game.Instance.GraphicsDevice.SetRenderTarget(Game.Instance.RenderTargetStack.Peek());
            }
        }

        public void Clear(Color color) {
            if (XNARenderTarget.DepthStencilFormat == DepthFormat.Depth24Stencil8) {
                Game.Instance.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, color, 1f, 0);
            } else if (XNARenderTarget.DepthStencilFormat != DepthFormat.None) {
                Game.Instance.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, color, 1f, 0);
            } else {
                Game.Instance.GraphicsDevice.Clear(color);
            }
        }

        public void Resize(int width, int height, bool mipmap = false) {
            if (XNARenderTarget == null) {
                throw new System.InvalidOperationException("Can't resize. Canvas internal state is invalid.");
            }

            if (width != XNARenderTarget.Width || height != XNARenderTarget.Height) {
                SurfaceFormat format = XNARenderTarget.Format;
                DepthFormat depthStencil = XNARenderTarget.DepthStencilFormat;
                int multiSampleCount = XNARenderTarget.MultiSampleCount;
                RenderTargetUsage renderTargetUsage = XNARenderTarget.RenderTargetUsage;

                Texture.Dispose();
                Texture = new Texture(new RenderTarget2D(
                    Game.Instance.GraphicsDevice,
                    width,
                    height,
                    mipmap,
                    format,
                    depthStencil,
                    multiSampleCount,
                    renderTargetUsage
                ));
            }

            InternalRenderer?.RecalculateProjection();
        }

        public void Resize(Size size) {
            Resize((int) size.Width, (int) size.Height);
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            if (_internalRenderer != null) {
                _internalRenderer.Dispose();
                _internalRenderer = null;
            }

            Texture?.Dispose();
            base.Dispose();
        }

        #endregion Public Methods
    }
}
