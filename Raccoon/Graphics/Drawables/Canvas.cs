using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Canvas : Image {
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
        public Renderer InternalRenderer { get; set; } = new Renderer();
        public RenderTarget2D XNARenderTarget { get { return Texture.XNATexture as RenderTarget2D; } }

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
            if (InternalRenderer != null) {
                InternalRenderer.End();
            }

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
            if ((width == XNARenderTarget.Width && height == XNARenderTarget.Height) || XNARenderTarget == null) {
                return;
            }

            RenderTarget2D renderTarget2D = new RenderTarget2D(
                Game.Instance.GraphicsDevice, 
                width, 
                height, 
                mipmap, 
                XNARenderTarget.Format, 
                XNARenderTarget.DepthStencilFormat, 
                XNARenderTarget.MultiSampleCount, 
                XNARenderTarget.RenderTargetUsage
            );

            InternalRenderer?.RecalculateProjection();

            Texture = new Texture(renderTarget2D);
        }

        public void Resize(Size size) {
            Resize((int) size.Width, (int) size.Height);
        }

        public override void Dispose() {
            XNARenderTarget.Dispose();
        }

        #endregion Public Methods
    }
}
