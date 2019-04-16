#region MIT License
/*Copyright (c) 2016 Robert Rouhani <robert.rouhani@gmail.com>

SharpFont based on Tao.FreeType, Copyright (c) 2003-2007 Tao Framework Team

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/
#endregion

//#define PRINT_RENDER_DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

using SharpFont;

using Raccoon.Fonts;

namespace Raccoon {
	internal class FontService : IDisposable {
        #region Private Members

		private bool _disposedValue; // To detect redundant calls

        #endregion Private Members

		#region Constructors

		/// <summary>
		/// If multithreading, each thread should have its own FontService.
		/// </summary>
		public FontService() {
			Library = new Library();
			SupportedFormats = new FontFormatCollection();
			AddFormat("TrueType", "ttf");
			AddFormat("OpenType", "otf");
			// Not so sure about these...
			//AddFormat("TrueType Collection", "ttc");
			//AddFormat("Type 1", "pfa"); // pfb?
			//AddFormat("PostScript", "pfm"); // ext?
			//AddFormat("FNT", "fnt");
			//AddFormat("X11 PCF", "pcf");
			//AddFormat("BDF", "bdf");
			//AddFormat("Type 42", "");
		}

        #endregion Constructors

        #region Public Properties

        public static int TabulationWhitespacesSize { get; set; } = 4;

        public Library Library { get; private set; }
		public FontFormatCollection SupportedFormats { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public static float ConvertEMToPx(Face face, int em) {
            return em / (float) face.UnitsPerEM * face.Size.Metrics.NominalWidth;
        }

        public static Size MeasureString(Face face, string text) {
            ProcessGlyphs(face, text, out Size size, out _);
            return size;
        }

        public static Size MeasureString(Face face, string text, out int lines) {
            ProcessGlyphs(face, text, out Size size, out lines);
            return size;
        }

		/// <summary>
		/// Render the string into a bitmap with <see cref="SystemColors.ControlText"/> text color and a transparent background.
		/// </summary>
		/// <param name="text">The string to render.</param>
		public virtual Bitmap RasterizeString(Face face, string text, out Size size) {
            return RasterizeString(face, text, SystemColors.ControlText, Color.Transparent, out size);
		}

		/// <summary>
		/// Render the string into a bitmap with a transparent background.
		/// </summary>
		/// <param name="text">The string to render.</param>
		/// <param name="foreColor">The color of the text.</param>
		/// <returns></returns>
		public virtual Bitmap RasterizeString(Face face, string text, Color foreColor, out Size size) {
			return RasterizeString(face, text, foreColor, Color.Transparent, out size);
		}

        /// <summary>
        /// Render the string into a bitmap with an opaque background.
        /// </summary>
        /// <param name="face">The face used to font rasterization.</param>
        /// <param name="text">The string to render.</param>
        /// <param name="foreColor">The color of the text.</param>
        /// <param name="backColor">The color of the background behind the text.</param>
        /// <returns></returns>
        public virtual Bitmap RasterizeString(Face face, string text, Color foreColor, Color backColor, out Size size) {
            ProcessGlyphs(face, text, out size, out int _, out List<GlyphRenderData> glyphs);

            if (size.Area == 0f) {
                return null;
            }

			Bitmap bitmap = new Bitmap((int) Math.Ceiling(size.Width), (int) Math.Ceiling(size.Height));
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap)) {
                #region Set up graphics

                // HighQuality and GammaCorrected both specify gamma correction be applied (2.2 in sRGB)
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms534094(v=vs.85).aspx
                g.CompositingQuality = CompositingQuality.HighQuality;

                // HighQuality and AntiAlias both specify antialiasing
                g.SmoothingMode = SmoothingMode.HighQuality;

                // If a background color is specified, blend over it.
                g.CompositingMode = CompositingMode.SourceOver;

                g.Clear(backColor);

                #endregion

                foreach (GlyphRenderData glyph in glyphs) {
                    if (glyph.Size.Area == 0f) {
                        continue;
                    }

                    g.DrawImageUnscaled(glyph.Bitmap, (int) glyph.X, (int) glyph.Y);
                }
            }

            return bitmap;
        }

		public IEnumerable<FileInfo> GetFontFiles(DirectoryInfo folder, bool recurse) {
			List<FileInfo> files = new List<FileInfo>();
			SearchOption option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

			foreach (FileInfo file in folder.GetFiles("*.*", option)) {
				if (SupportedFormats.ContainsExt(file.Extension)) {
					//yield return file;
					files.Add(file);
				}
			}

			return files;
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing).
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
                }

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~FontService() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

        #endregion Protected Methods

        #region Private Methods

        public static FontFaceRenderMap CreateFaceRenderMap(Face face) {
            FontFaceRenderMap renderMap = new FontFaceRenderMap(face, new Size(face.Size.Metrics.NominalWidth, face.Size.Metrics.NominalHeight));

            float underrun = 0,
                  overrun = 0,
                  x = 0, 
                  y = 0;

            // prepare texture
            int sideSize = (int) Math.Ceiling(System.Math.Sqrt(face.GlyphCount)) * face.Size.Metrics.NominalWidth;
            int textureSideSize = Util.Math.CeilingPowerOfTwo(sideSize);

            Graphics.Texture texture = new Graphics.Texture(textureSideSize, textureSideSize);

            // prepare texture data copying glyphs bitmaps
            Graphics.Color[] textureData = new Graphics.Color[textureSideSize * textureSideSize];

            Rectangle glyphArea = new Rectangle(renderMap.GlyphSlotSize);

            uint charCode = face.GetFirstChar(out uint glyphIndex);

            while (glyphIndex != 0) {
                ProcessGlyph(face, charCode, generateRenderData: true, renderTimes: 1, ref underrun, trackingUnderrun: true, ref x, ref y, ref overrun, isEndOfLine: true);

                face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);

                FTBitmap ftBitmap = face.Glyph.Bitmap;

                if (ftBitmap != null && ftBitmap.Width > 0 && ftBitmap.Rows > 0) {
                    if (ftBitmap.PixelMode != PixelMode.Gray) {
                        throw new NotImplementedException("Supported PixelMode formats are: Gray");
                    }

                    CopyBitmapToDestinationArea(textureData, textureSideSize, glyphArea.Position, ftBitmap);

                    renderMap.RegisterGlyph(
                        charCode, 
                        new Rectangle(glyphArea.Position, new Size(ftBitmap.Width, ftBitmap.Rows)),
                        face.Glyph.Metrics.HorizontalBearingX.ToSingle(),
                        face.Glyph.Metrics.HorizontalBearingY.ToSingle(),
                        new Vector2(face.Glyph.Advance.X.ToSingle(), face.Glyph.Advance.Y.ToSingle())
                    );

                    glyphArea.X += glyphArea.Width;

                    if (glyphArea.X >= textureSideSize) {
                        glyphArea.Y += glyphArea.Height;
                        glyphArea.X = 0;
                    }
                } else {
                    renderMap.RegisterGlyph(
                        charCode, 
                        Rectangle.Empty,
                        face.Glyph.Metrics.HorizontalBearingX.ToSingle(),
                        face.Glyph.Metrics.HorizontalBearingY.ToSingle(),
                        new Vector2(face.Glyph.Advance.X.ToSingle(), face.Glyph.Advance.Y.ToSingle())
                    );
                }

                charCode = face.GetNextChar(charCode, out glyphIndex);
            }

            // render glyphs
            texture.SetData(textureData);
            renderMap.Texture = texture;

            return renderMap;

            void CopyBitmapToDestinationArea(Graphics.Color[] data, int dataRowSize, Vector2 destinationTopleft, FTBitmap ftBitmap) {
                byte[] buffer = ftBitmap.BufferData;
                int dataOffset,
                    bufferOffset,
                    pitch = Math.Abs(ftBitmap.Pitch);

                for (int row = 0; row < ftBitmap.Rows; row++) {
                    dataOffset = (int) ((destinationTopleft.Y + row) * dataRowSize + destinationTopleft.X);
                    bufferOffset = row * pitch;

                    for (int column = 0; column < ftBitmap.Width; column++, dataOffset++) {
                        byte px = buffer[bufferOffset + column];

                        data[dataOffset] = new Graphics.Color(px, px, px, px);
                    }
                }
            }
        }

        private static void ProcessGlyphs(Face face, string text, out Size size, out int lines) {
            ProcessGlyphs(face, text, generateRenderData: false, out size, out lines, out _);
        }

        private static void ProcessGlyphs(Face face, string text, out Size size, out int lines, out List<GlyphRenderData> glyphs) {
            ProcessGlyphs(face, text, generateRenderData: true, out size, out lines, out glyphs);
        }

        private static void ProcessGlyphs(Face face, string text, bool generateRenderData, out Size size, out int lines, out List<GlyphRenderData> glyphs) {
            glyphs = new List<GlyphRenderData>();

            float stringWidth = 0,
                  overrun = 0,
                  underrun = 0,
                  kern,
                  lineHeight = face.Size.Metrics.NominalHeight,
                  x = 0,
                  y = 0;

            lines = 0;

            bool isEndOfLine = false,
                 trackingUnderrun = true;

            int renderTimes;

            for (int i = 0; i < text.Length; i++) {
                char charCode = text[i];

                if (charCode == '\n') { // new line
                    y += lineHeight;
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
                    renderTimes = TabulationWhitespacesSize;
                } else {
                    renderTimes = 1;
                }

                GlyphRenderData[] currentGlyphs = ProcessGlyph(face, charCode, generateRenderData, renderTimes, ref underrun, trackingUnderrun, ref x, ref y, ref overrun, isEndOfLine);

                glyphs.AddRange(currentGlyphs);

                #region Kerning (for NEXT character)

                // Adjust for kerning between this character and the next.
                if (face.HasKerning && !isEndOfLine) {
                    char nextCharCode = text[i + 1];
                    kern = face.GetKerning(face.GetCharIndex(charCode), face.GetCharIndex(nextCharCode), KerningMode.Default).X.ToSingle();

                    if (Math.Abs(kern) > face.Glyph.Advance.X.ToSingle() * 5f) {
                        kern = 0;
                    }

                    if (generateRenderData) {
                        currentGlyphs[currentGlyphs.Length - 1].Kern = kern;
                    }

                    x += kern;
                }

                #endregion Kerning (for NEXT character)

                if (isEndOfLine) {
                    isEndOfLine = false;
                    lines++;

                    stringWidth = Math.Max(stringWidth, x);

                    underrun = overrun = x = 0;
                }
            }

            size = new Size(stringWidth, y + lineHeight);
        }

        private static GlyphRenderData[] ProcessGlyph(Face face, uint charCode, bool generateRenderData, int renderTimes, ref float underrun, bool trackingUnderrun, ref float x, ref float y, ref float overrun, bool isEndOfLine) {
            #region Load character

            FTBitmap ftbmp = null;
            float ascent = ConvertEMToPx(face, face.BBox.Top);

            uint glyphIndex = face.GetCharIndex(charCode);
            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

            if (generateRenderData) {
                face.Glyph.RenderGlyph(RenderMode.Normal);
                ftbmp = face.Glyph.Bitmap;
            }

            float gAdvanceX = face.Glyph.Advance.X.ToSingle();
            float gBearingX = face.Glyph.Metrics.HorizontalBearingX.ToSingle();
            float gWidth = face.Glyph.Metrics.Width.ToSingle();

            #endregion Load character

            GlyphRenderData[] glyphs =  new GlyphRenderData[generateRenderData ? renderTimes : 0];

            for (int j = 0; j < renderTimes; j++) {
                #region Underrun

                underrun += -gBearingX;

                if (x == 0) {
                    x += underrun;
                }

                float currentUnderrun = 0;
                if (trackingUnderrun) {
                    currentUnderrun = underrun;
                }

                if (trackingUnderrun && underrun <= 0) {
                    underrun = 0;
                    trackingUnderrun = false;
                }

                #endregion Underrun

                #region Prepare GlyphRenderData

                GlyphRenderData glyphRenderData = null;

                if (generateRenderData) {
                    glyphRenderData = new GlyphRenderData((char) charCode, gAdvanceX, gBearingX) {
                        Underrun = currentUnderrun
                    };

                    glyphs[j] = glyphRenderData;

                    if (ftbmp.Width > 0 && ftbmp.Rows > 0) {
                        glyphRenderData.Size = new Size(ftbmp.Width, ftbmp.Rows);
                        glyphRenderData.Position = new Vector2(
                            Math.Round(x + face.Glyph.BitmapLeft),
                            Math.Round(y + ascent - face.Glyph.Metrics.HorizontalBearingY.ToSingle())
                        );

                        glyphRenderData.Bitmap = ftbmp.ToGdipBitmap(System.Drawing.Color.White);

                        glyphRenderData.HorizontalBearingX = face.Glyph.BitmapLeft;
                    }
                }

                #endregion Prepare GlyphRenderData

                #region Overrun

                if (gBearingX + gWidth > 0 || gAdvanceX > 0) {
                    overrun -= Math.Max(gBearingX + gWidth, gAdvanceX);
                    if (overrun <= 0) {
                        overrun = 0;
                    }
                }

                overrun += gBearingX == 0 && gWidth == 0 ? 0 : gBearingX + gWidth - gAdvanceX;

                if (isEndOfLine && j == renderTimes - 1) {
                    x += overrun;
                }

                if (generateRenderData) {
                    glyphRenderData.Overrun = overrun;
                }

                #endregion Overrun

                // Advance pen positions for drawing the next character.
                x += face.Glyph.Advance.X.ToSingle(); // same as Metrics.HorizontalAdvance?
                y += face.Glyph.Advance.Y.ToSingle();

                if (generateRenderData) {
                    glyphRenderData.RightEdge = x;
                }

                #if DEBUG && PRINT_RENDER_DEBUG
                spacingError = bmp.Width - (int) Math.Round(rc.RightEdge);
                renderedChars.Add(rc);
                #endif

                #region Kerning (for NEXT character)

                // Adjust for kerning between this character and the next.
                if (face.HasKerning && !isEndOfLine && renderTimes > 1) {
                    float kern = face.GetKerning(glyphIndex, glyphIndex, KerningMode.Default).X.ToSingle(); // get kerning to the same character, since renderTimes is greater than 1

                    if (Math.Abs(kern) > face.Glyph.Advance.X.ToSingle() * 5f) {
                        kern = 0;
                    }

                    if (generateRenderData) {
                        glyphRenderData.Kern = kern;
                    }

                    x += kern;
                }

                #endregion Kerning (for NEXT character)
            }

            return glyphs;
        }

        private void AddFormat(string name, string ext) {
			SupportedFormats.Add(name, ext);
		}

        #endregion Private Methods
	}
}
