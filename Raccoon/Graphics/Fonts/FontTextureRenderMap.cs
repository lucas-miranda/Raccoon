using System.Collections.Generic;
using Raccoon.Graphics;

namespace Raccoon.Fonts {
    public class FontTextureRenderMap : FontRenderMap {
        private static FontMTSDFShader MTSDFShaderInstance;

        public FontTextureRenderMap(Texture texture, string dataFilepath, float size) {
            Texture = texture;
            DataFilepath = dataFilepath;

            FontTextureData data = new FontTextureData(DataFilepath);
            NominalWidth = NominalHeight = data.Atlas.Size;
            Load(data);
        }

        public static FontMTSDFShader FontMTSDFShader {
            get {
                if (MTSDFShaderInstance == null) {
                    MTSDFShaderInstance = new FontMTSDFShader(Resource.FontMTSDFShader);
                }

                return MTSDFShaderInstance;
            }
        }

        public string DataFilepath { get; }
        public override bool HasKerning { get { return false; } }
        public FontTextureAtlasKind AtlasKind { get; private set; }
        public float PixelDistanceRange { get; private set; }
        public override Shader Shader { get { return FontMTSDFShader; } }

        public override void Setup(float size) {
            base.Setup(size);
            //NominalWidth = NominalHeight = size;
        }

        public override void Reload() {
            Texture.Reload();

            FontTextureData data = new FontTextureData(DataFilepath);
            Load(data);
        }

        public override IShaderParameters CreateShaderParameters() {
            return new FontMTSDFShaderParameters(Size, PixelDistanceRange);
        }

        protected override double Kerning(uint leftCharCode, uint rightCharCode) {
            return 0.0;
        }

        protected override void Disposed() {
            if (Texture != null && !Texture.IsDisposed) {
                Texture.Dispose();
                Texture = null;
            }
        }

        private void Load(FontTextureData data) {
            // validate texture size
            if (Texture.Width != data.Atlas.Width || Texture.Height != data.Atlas.Height) {
                throw new System.ArgumentException(
                    $"Font texture data expects a size {data.Atlas.Width}x{data.Atlas.Height}, but a texture with size {Texture.Width}x{Texture.Height} was given."
                );
            }

            LineHeight = data.Metrics.LineHeight;
            Ascender = data.Metrics.Ascender;
            Descender = data.Metrics.Descender;
            UnderlinePosition = data.Metrics.UnderlineY;
            UnderlineThickness = data.Metrics.UnderlineThickness;

            AtlasKind = data.Atlas.Kind;
            //GeneratedSize = data.Atlas.Size;
            PixelDistanceRange = data.Atlas.DistanceRange;

            foreach (KeyValuePair<uint, FontTextureDataGlyph> entry in data.Glyphs) {
                RegisterGlyph(
                    // charCode
                    entry.Key,

                    // source area
                    new Rectangle(
                        new Vector2(
                            entry.Value.AtlasBounds.Left,
                            entry.Value.AtlasBounds.Top
                        ),
                        new Vector2(
                            entry.Value.AtlasBounds.Right,
                            entry.Value.AtlasBounds.Bottom
                        )
                    ),

                    // bearing X
                    entry.Value.PlaneBounds.Left,

                    // bearing Y
                    entry.Value.PlaneBounds.Top,

                    // width
                    entry.Value.PlaneBounds.Right - entry.Value.PlaneBounds.Left,

                    // height
                    entry.Value.PlaneBounds.Bottom - entry.Value.PlaneBounds.Top,

                    // advance X
                    entry.Value.Advance,

                    // advance Y
                    0.0
                );
            }
        }
    }
}
