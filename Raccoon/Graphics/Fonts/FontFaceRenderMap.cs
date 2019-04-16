using System.Collections.Generic;
using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon.Fonts {
    internal class FontFaceRenderMap {
        #region Constructors

        public FontFaceRenderMap(SharpFont.Face face, Size glyphSlotSize) {
            Face = face;
            GlyphSlotSize = glyphSlotSize;
        }

        #endregion Constructors

        #region Public Properties

        public SharpFont.Face Face { get; }
        public Texture Texture { get; set; }
        public Size GlyphSlotSize { get; }
        public Dictionary<uint, Glyph> Glyphs { get; } = new Dictionary<uint, Glyph>();

        #endregion Public Properties

        #region Public Methods

        public Glyph RegisterGlyph(uint charCode, Rectangle sourceArea, float horizontalBearingX, float horizontalBearingY, Vector2 advance) {
            Glyph glyph = new Glyph(sourceArea, horizontalBearingX, horizontalBearingY, advance);
            Glyphs.Add(charCode, glyph);
            return glyph;
        }

        public List<(Vector2 Position, Rectangle SourceArea)> PrepareText(string text) {
            List<(Vector2 Position, Rectangle SourceArea)> preparedText = new List<(Vector2, Rectangle)>();

            float ascent = FontService.ConvertEMToPx(Face, Face.BBox.Top),
                  overrun = 0,
                  kern,
                  lineHeight = Face.Size.Metrics.NominalHeight;

            Vector2 penPosition = Vector2.Zero;
            bool isEndOfLine = false;

            int renderTimes;

            // TODO: add support to unicode characters
            for (int i = 0; i < text.Length; i++) {
                char charCode = text[i];

                if (charCode == '\n') { // new line
                    penPosition.Y += lineHeight;
                    continue;
                } else if (charCode == '\r') { // carriage return
                    // do nothing, just ignore
                    // TODO: maybe add an option to detect when carriage return handling is needed (older MAC OS systems)
                    continue;
                } else if (i + 1 == text.Length || text[i + 1] == '\n' || (i + 2 < text.Length && text[i + 1] == '\r' && text[i + 2] == '\n')) {
                    isEndOfLine = true;
                }

                if (charCode == '\t') { // tabulation
                    // will render a tabulation representation using whitespaces
                    charCode = ' ';
                    renderTimes = FontService.TabulationWhitespacesSize;
                } else {
                    renderTimes = 1;
                }

                Glyph glyph = Glyphs[charCode];

                for (int j = 0; j < renderTimes; j++) {
                    #region Underrun

                    if (penPosition.X == 0f) {
                        penPosition.X += -glyph.HorizontalBearingX;
                    }

                    #endregion Underrun

                    preparedText.Add((penPosition + new Vector2(glyph.HorizontalBearingX, ascent - glyph.HorizontalBearingY), glyph.SourceArea));

                    #region Overrun

                    if (glyph.HorizontalBearingX + glyph.SourceArea.Width > 0 || glyph.Advance.X > 0) {
                        overrun -= Math.Max(glyph.HorizontalBearingX + glyph.SourceArea.Width, glyph.Advance.X);

                        if (overrun <= 0) {
                            overrun = 0;
                        }
                    }

                    overrun += glyph.HorizontalBearingX == 0 && glyph.SourceArea.Width == 0 ? 0 : glyph.HorizontalBearingX + glyph.SourceArea.Width - glyph.Advance.X;

                    if (isEndOfLine && j == renderTimes - 1) {
                        penPosition.X += overrun;
                    }

                    #endregion Overrun

                    penPosition += glyph.Advance;

                    #region Kerning with Next Repeated Character

                    // Adjust for kerning between this character and the next (if repeatTimes > 1)
                    if (Face.HasKerning && !isEndOfLine && renderTimes > 1) {
                        uint glyphIndex = Face.GetCharIndex(charCode);
                        kern = Face.GetKerning(glyphIndex, glyphIndex, SharpFont.KerningMode.Default).X.ToSingle();

                        if (Math.Abs(kern) > glyph.Advance.X * 5f) {
                            kern = 0;
                        }

                        penPosition.X += kern;
                    }

                    #endregion Kerning with Next Repeated Character
                }

                #region Kerning with Next Character

                // Adjust for kerning between this character and the next.
                if (Face.HasKerning && !isEndOfLine) {
                    char nextCharCode = text[i + 1];
                    kern = Face.GetKerning(Face.GetCharIndex(charCode), Face.GetCharIndex(nextCharCode), SharpFont.KerningMode.Default).X.ToSingle();

                    if (Math.Abs(kern) > glyph.Advance.X * 5f) {
                        kern = 0;
                    }

                    penPosition.X += kern;
                }

                #endregion Kerning with Next Character

                if (isEndOfLine) {
                    isEndOfLine = false;
                    //lines++;

                    //stringWidth = Math.Max(stringWidth, penPosition.X);

                    /*underrun =*/ overrun = penPosition.X = 0;
                }
            }

            return preparedText;
        }

        #endregion Public Methods

        #region Class Glyph

        public class Glyph {
            public Glyph(Rectangle sourceArea, float horizontalBearingX, float horizontalBearingY, Vector2 advance) {
                SourceArea = sourceArea;
                HorizontalBearingX = horizontalBearingX;
                HorizontalBearingY = horizontalBearingY;
                Advance = advance;
            }

            public Rectangle SourceArea { get; }
            public float HorizontalBearingX { get; }
            public float HorizontalBearingY { get; }
            public Vector2 Advance { get; }
        }

        #endregion Class Glyph
    }
}
