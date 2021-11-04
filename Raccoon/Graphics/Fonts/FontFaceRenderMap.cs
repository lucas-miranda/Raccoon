#define TIGHT_PACK_FACE_RENDER_MAP

using SharpFont;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon.Fonts {
    public class FontFaceRenderMap : FontRenderMap {
        public enum SizeStrategyKind {
            Regenerate = 0,
            Rescale
        }

        #region Private Members

        private static Library LibraryInstance;

        #endregion

        #region Constructors

        public FontFaceRenderMap(SharpFont.Face face, float size) {
            if (face == null) {
                throw new System.ArgumentNullException(nameof(face));
            }

            Face = face;
            Face.SetPixelSizes((uint) Util.Math.Round(size), (uint) Util.Math.Round(size));

            NominalWidth = Face.Size.Metrics.NominalWidth;
            NominalHeight = Face.Size.Metrics.NominalHeight;

            Load();
        }

        public FontFaceRenderMap(string filename, float size)
            : this(
                new SharpFont.Face(Library, filename),
                size
            )
        {
            Filename = filename;
        }

        public FontFaceRenderMap(byte[] file, int faceIndex, float size)
            : this(
                new SharpFont.Face(Library, file, faceIndex),
                size
            )
        {
        }

        #endregion Constructors

        #region Public Properties

        public static Library Library {
            get {
                if (LibraryInstance == null) {
                    LibraryInstance = new Library();
                }

                return LibraryInstance;
            }
        }

        public string Filename { get; private set; }
        public SharpFont.Face Face { get; private set; }
        public override bool HasKerning { get { return Face.HasKerning; } }
        public SizeStrategyKind SizeStrategy { get; set; }
        public override Shader Shader { get { return null; } }

        #endregion Public Properties

        #region Public Methods

        public override void Setup(float size) {
            base.Setup(size);

            switch (SizeStrategy) {
                case SizeStrategyKind.Regenerate:
                    Face.SetPixelSizes((uint) Util.Math.Round(size), (uint) Util.Math.Round(size));

                    NominalWidth = Face.Size.Metrics.NominalWidth;
                    NominalHeight = Face.Size.Metrics.NominalHeight;

                    Load();
                    break;

                case SizeStrategyKind.Rescale:
                    break;

                default:
                    throw new System.NotImplementedException($"Size strategy '{SizeStrategy}' isn't implemented.");
            }
        }

        public override void Reload() {
            if (Filename != null) {
                Face?.Dispose();
                Face = new SharpFont.Face(Library, Filename);
            }

            Face.SetCharSize(NominalWidth, NominalHeight, 96, 96);
            Load();
        }

        public override IShaderParameters CreateShaderParameters() {
            return null;
        }

        #endregion Public Methods

        #region Protected Methods

        protected override double Kerning(uint leftCharCode, uint rightCharCode) {
            uint leftGlyphIndex = Face.GetCharIndex(leftCharCode),
                 rightGlyphIndex = Face.GetCharIndex(rightCharCode);

            return Face.GetKerning(leftGlyphIndex, rightGlyphIndex, SharpFont.KerningMode.Default).X.ToDouble();
        }

        protected override void Disposed() {
            if (Texture != null && !Texture.IsDisposed) {
                Texture.Dispose();
                Texture = null;
            }

            if (Face != null) {
                Face.Dispose();
                Face = null; // Font owns SharpFont.Face
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private void Load() {
            //SizeMetrics fontSizeMetrics = Face.Size.Metrics;
            //Size glyphSlotSize = new Size(fontSizeMetrics.NominalWidth, fontSizeMetrics.Height.ToSingle());

            /*
            FontFaceRenderMap renderMap = new FontFaceRenderMap(
                face,
                glyphSlotSize,
                fontSizeMetrics.NominalWidth,
                fontSizeMetrics.NominalHeight,
                fontSizeMetrics.Height.ToSingle(),
                fontSizeMetrics.Ascender.ToSingle(),
                fontSizeMetrics.Descender.ToSingle()
            );
            */

            // adjust signs to respect Raccoon y-origin direction
            LineHeight = ConvertPxToEm(Face.Size.Metrics.Height.ToSingle(), Size);
            Ascender = -ConvertPxToEm(Face.Size.Metrics.Ascender.ToSingle(), Size);
            Descender = Math.Abs(ConvertPxToEm(Face.Size.Metrics.Descender.ToSingle(), Size));
            UnderlinePosition = Math.Abs(Face.UnderlinePosition / (float) Face.UnitsPerEM);
            UnderlineThickness = Face.UnderlineThickness / (float) Face.UnitsPerEM;

            // prepare texture
            int sideSize = (int) (Util.Math.Ceiling(System.Math.Sqrt(Face.GlyphCount)) * GlyphSlotSize.Width),
                textureSideSize = Util.Math.CeilingPowerOfTwo(sideSize);

            if (Texture == null) {
                Texture = new Graphics.Texture(textureSideSize, textureSideSize);
            } else if (Texture.Width != textureSideSize || Texture.Height != textureSideSize) {
                Texture.Dispose();
                Texture = new Graphics.Texture(textureSideSize, textureSideSize);
            }

            // prepare texture data copying glyphs bitmaps
            Graphics.Color[] textureData = new Graphics.Color[textureSideSize * textureSideSize];

            Vector2 glyphPosition = Vector2.Zero;

            uint charCode = Face.GetFirstChar(out uint glyphIndex);

            while (glyphIndex != 0) {
                Face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                Face.Glyph.RenderGlyph(RenderMode.Normal);

                FTBitmap ftBitmap = Face.Glyph.Bitmap;

                if (ftBitmap != null && ftBitmap.Width > 0 && ftBitmap.Rows > 0) {
                    if (ftBitmap.PixelMode != PixelMode.Gray) {
                        throw new System.NotImplementedException("Supported PixelMode formats are: Gray");
                    }

                    // tests if glyph bitmap actually fits on current row
#if TIGHT_PACK_FACE_RENDER_MAP
                    if (glyphPosition.X + ftBitmap.Width >= textureSideSize) {
                        glyphPosition.Y += GlyphSlotSize.Height;
                        glyphPosition.X = 0;
                    }
#else
                    if (glyphPosition.X + GlyphSlotSize.Width >= textureSideSize) {
                        glyphPosition.Y += GlyphSlotSize.Height;
                        glyphPosition.X = 0;
                    }
#endif

                    CopyBitmapToDestinationArea(textureData, textureSideSize, glyphPosition, ftBitmap);

                    RegisterGlyph(
                        charCode,
                        new Rectangle(
                            glyphPosition,
                            new Size(ftBitmap.Width, ftBitmap.Rows)
                        ),
                        -ConvertPxToEm(Face.Glyph.Metrics.HorizontalBearingX.ToDouble(), Size),
                        -ConvertPxToEm(Face.Glyph.Metrics.HorizontalBearingY.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Metrics.Width.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Metrics.Height.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Advance.X.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Advance.Y.ToDouble(), Size)
                    );

                    // advance to next glyph area
#if TIGHT_PACK_FACE_RENDER_MAP
                    glyphPosition.X += ftBitmap.Width;
#else
                    glyphPosition.X += GlyphSlotSize.Width;
#endif
                } else {
                    RegisterGlyph(
                        charCode,
                        Rectangle.Empty,
                        -ConvertPxToEm(Face.Glyph.Metrics.HorizontalBearingX.ToDouble(), Size),
                        -ConvertPxToEm(Face.Glyph.Metrics.HorizontalBearingY.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Metrics.Width.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Metrics.Height.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Advance.X.ToDouble(), Size),
                        ConvertPxToEm(Face.Glyph.Advance.Y.ToDouble(), Size)
                    );
                }

                charCode = Face.GetNextChar(charCode, out glyphIndex);
            }

            // render glyphs
            Texture.SetData(textureData);
        }

        private void CopyBitmapToDestinationArea(
            Graphics.Color[] data,
            int dataRowSize,
            Vector2 destinationTopleft,
            FTBitmap ftBitmap
        ) {
            byte[] buffer = ftBitmap.BufferData;
            int dataOffset,
                bufferOffset,
                pitch = Util.Math.Abs(ftBitmap.Pitch);

            for (int row = 0; row < ftBitmap.Rows; row++) {
                dataOffset = (int) ((destinationTopleft.Y + row) * dataRowSize + destinationTopleft.X);
                bufferOffset = row * pitch;

                for (int column = 0; column < ftBitmap.Width; column++, dataOffset++) {
                    byte px = buffer[bufferOffset + column];

                    data[dataOffset] = new Graphics.Color(px, px, px, px);
                }
            }
        }

        #endregion Private Methods
    }
}
