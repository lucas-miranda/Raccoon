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

#define TIGHT_PACK_FACE_RENDER_MAP

using System.IO;
using System.Collections.Generic;

using SharpFont;

using Raccoon.Fonts;

namespace Raccoon {
	internal class FontService : System.IDisposable {
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

        public static float ConvertEMToPx(int em, ushort nominalWidth, ushort unitsPerEM) {
            return em / (float) unitsPerEM * nominalWidth;
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
#if TIGHT_PACK_FACE_RENDER_MAP
            FTSize fontSize = face.Size;
            SizeMetrics fontSizeMetrics = fontSize.Metrics;
            ushort nominalWidth = fontSizeMetrics.NominalWidth,
                   nominalHeight = fontSizeMetrics.NominalHeight;
            Size glyphSlotSize = new Size(ConvertEMToPx(face.BBox.Right - face.BBox.Left, nominalWidth, face.UnitsPerEM), nominalHeight);
#else
            Size glyphSlotSize = new Size(face.Size.Metrics.NominalWidth, face.Size.Metrics.NominalHeight);
#endif

            FontFaceRenderMap renderMap = new FontFaceRenderMap(face, glyphSlotSize, nominalWidth, nominalHeight);

            // prepare texture
            int sideSize = (int) (Util.Math.Ceiling(System.Math.Sqrt(face.GlyphCount)) * glyphSlotSize.Width);
            int textureSideSize = Util.Math.CeilingPowerOfTwo(sideSize);

            Graphics.Texture texture = new Graphics.Texture(textureSideSize, textureSideSize);

            // prepare texture data copying glyphs bitmaps
            Graphics.Color[] textureData = new Graphics.Color[textureSideSize * textureSideSize];

            Vector2 glyphPosition = Vector2.Zero;

            uint charCode = face.GetFirstChar(out uint glyphIndex);

            while (glyphIndex != 0) {
                face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);

                FTBitmap ftBitmap = face.Glyph.Bitmap;

                if (ftBitmap != null && ftBitmap.Width > 0 && ftBitmap.Rows > 0) {
                    if (ftBitmap.PixelMode != PixelMode.Gray) {
                        throw new System.NotImplementedException("Supported PixelMode formats are: Gray");
                    }

                    // tests if glyph bitmap actually fits on current row
#if TIGHT_PACK_FACE_RENDER_MAP
                    if (glyphPosition.X + ftBitmap.Width >= textureSideSize) {
                        glyphPosition.Y += glyphSlotSize.Height;
                        glyphPosition.X = 0;
                    }
#else
                    if (glyphPosition.X + glyphSlotSize.Width >= textureSideSize) {
                        glyphPosition.Y += glyphSlotSize.Height;
                        glyphPosition.X = 0;
                    }
#endif

                    CopyBitmapToDestinationArea(textureData, textureSideSize, glyphPosition, ftBitmap);

                    renderMap.RegisterGlyph(
                        charCode,
                        new Rectangle(glyphPosition, new Size(ftBitmap.Width, ftBitmap.Rows)),
                        face.Glyph.Metrics.HorizontalBearingX.ToSingle(),
                        face.Glyph.Metrics.HorizontalBearingY.ToSingle(),
                        new Vector2(face.Glyph.Advance.X.ToSingle(), face.Glyph.Advance.Y.ToSingle())
                    );

                    // advance to next glyph area
#if TIGHT_PACK_FACE_RENDER_MAP
                    glyphPosition.X += ftBitmap.Width;
#else
                    glyphPosition.X += glyphSlotSize.Width;
#endif
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
        }

        private void AddFormat(string name, string ext) {
			SupportedFormats.Add(name, ext);
		}

        #endregion Private Methods
	}
}
