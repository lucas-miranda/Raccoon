using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public enum CanvasUsage {
        DiscardContents = RenderTargetUsage.DiscardContents,
        PreserveContents = RenderTargetUsage.PreserveContents,
        PlatformContents = RenderTargetUsage.PlatformContents
    }

    public enum SurfaceFormat {
        Color = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color,
        Bgr565 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr565,
        Bgra5551 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra5551,
        Bgra4444 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra4444,
        Dxt1 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt1,
        Dxt3 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt3,
        Dxt5 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt5,
        NormalizedByte2 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.NormalizedByte2,
        NormalizedByte4 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.NormalizedByte4,
        Rgba1010102 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Rgba1010102,
        Rg32 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Rg32,
        Rgba64 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Rgba64,
        Alpha8 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Alpha8,
        Single = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Single,
        Vector2 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Vector2,
        Vector4 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Vector4,
        HalfSingle = Microsoft.Xna.Framework.Graphics.SurfaceFormat.HalfSingle,
        HalfVector2 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.HalfVector2,
        HalfVector4 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.HalfVector4,
        HdrBlendable = Microsoft.Xna.Framework.Graphics.SurfaceFormat.HdrBlendable,
        Bgr32 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32,
        Bgra32 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra32,
        ColorSRgb = Microsoft.Xna.Framework.Graphics.SurfaceFormat.ColorSRgb,
        Bgr32SRgb = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgr32SRgb,
        Bgra32SRgb = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra32SRgb,
        Dxt1SRgb = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt1SRgb,
        Dxt3SRgb = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt3SRgb,
        Dxt5SRgb = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt5SRgb,
        RgbPvrtc2Bpp = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbPvrtc2Bpp,
        RgbPvrtc4Bpp = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbPvrtc4Bpp,
        RgbaPvrtc2Bpp = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbaPvrtc2Bpp,
        RgbaPvrtc4Bpp = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbaPvrtc4Bpp,
        RgbEtc1 = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbEtc1,
        Dxt1a = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Dxt1a,
        RgbaAtcExplicitAlpha = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbaAtcExplicitAlpha,
        RgbaAtcInterpolatedAlpha = Microsoft.Xna.Framework.Graphics.SurfaceFormat.RgbaAtcInterpolatedAlpha
    }

    public enum DepthFormat {
        None = Microsoft.Xna.Framework.Graphics.DepthFormat.None,
        Depth16 = Microsoft.Xna.Framework.Graphics.DepthFormat.Depth16,
        Depth24 = Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24,
        Depth24Stencil8 = Microsoft.Xna.Framework.Graphics.DepthFormat.Depth24Stencil8
    }

    public class Canvas : Image {
        public Canvas(int width, int height) : base(new Texture(new RenderTarget2D(Game.Instance.Core.GraphicsDevice, width, height))) { }
        public Canvas(int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat) : base(new Texture(new RenderTarget2D(Game.Instance.Core.GraphicsDevice, width, height, mipMap, (Microsoft.Xna.Framework.Graphics.SurfaceFormat) surfaceFormat, (Microsoft.Xna.Framework.Graphics.DepthFormat) depthFormat))) { }
        public Canvas(int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int multiSampleCount, CanvasUsage usage) : base(new Texture(new RenderTarget2D(Game.Instance.Core.GraphicsDevice, width, height, mipMap, (Microsoft.Xna.Framework.Graphics.SurfaceFormat) surfaceFormat, (Microsoft.Xna.Framework.Graphics.DepthFormat) depthFormat, multiSampleCount, (RenderTargetUsage) usage))) { }
        public Canvas(int width, int height, bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int multiSampleCount, CanvasUsage usage, bool shared, int arraySize) : base(new Texture(new RenderTarget2D(Game.Instance.Core.GraphicsDevice, width, height, mipMap, (Microsoft.Xna.Framework.Graphics.SurfaceFormat) surfaceFormat, (Microsoft.Xna.Framework.Graphics.DepthFormat) depthFormat, multiSampleCount, (RenderTargetUsage) usage, shared, arraySize))) { }

        public DepthFormat DepthStencilFormat { get { return (DepthFormat) XNARenderTarget.DepthStencilFormat; } }
        public int MultiSampleCount { get { return XNARenderTarget.MultiSampleCount; } }
        public CanvasUsage Usage { get { return (CanvasUsage) XNARenderTarget.RenderTargetUsage; } }

        internal RenderTarget2D XNARenderTarget { get { return Texture.XNATexture as RenderTarget2D; } }

        public void Begin() {
            Game.Instance.Core.GraphicsDevice.SetRenderTarget(XNARenderTarget);
            Game.Instance.Core.RenderTargetStack.Push(XNARenderTarget);
        }

        public void End() {
            Game.Instance.Core.RenderTargetStack.Pop();
            Game.Instance.Core.GraphicsDevice.SetRenderTarget(Game.Instance.Core.RenderTargetStack.Peek());
        }

        public void Clear(Color color) {
            Game.Instance.Core.GraphicsDevice.Clear(color);
        }

        public override void Dispose() {
            XNARenderTarget.Dispose();
        }
    }
}
