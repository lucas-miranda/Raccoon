using System.IO;

using Raccoon.Fonts;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public class Font : IAsset {
        #region Private Members

        private float _size;

        #endregion Private Members

        #region Constructors

        static Font() {
        }

        private Font() {
        }

        public Font(Texture texture, string dataFilepath, float size = 12f) {
            RenderMap = new FontTextureRenderMap(texture, dataFilepath, size);
            ShaderParameters = RenderMap.CreateShaderParameters();
            Size = size;
        }

        public Font(string filename, float size = 12f) {
            if (string.IsNullOrEmpty(filename)) {
                throw new System.ArgumentException($"Invalid font filename '{filename}'");
            }

            Filename = filename;
            if (filename.Contains(".") && !Filename.Contains(Game.Instance.ContentDirectory)) {
                Filename = Path.Combine(Game.Instance.ContentDirectory, Filename);
            }

            RenderMap = new FontFaceRenderMap(Filename, size);
            ShaderParameters = RenderMap.CreateShaderParameters();
            Size = size;
        }

        public Font(byte[] file, int faceIndex, float size = 12f) {
            RenderMap = new FontFaceRenderMap(file, faceIndex, size);
            ShaderParameters = RenderMap.CreateShaderParameters();
            Size = size;
        }

        public Font(SharpFont.Face fontFace, float size = 12f) {
            RenderMap = new FontFaceRenderMap(fontFace, size);
            ShaderParameters = RenderMap.CreateShaderParameters();
            Size = size;
        }

        public Font(Stream fontStream, int faceIndex, float size = 12f) {
            Load(fontStream, faceIndex, size);
            Size = size;
        }

        #endregion Constructors

        #region Public Properties

        public static int TabulationWhitespacesSize { get; set; } = 4;

        public string Name { get; set; } = "Font";
        //public string FamilyName { get { return Face.FamilyName; } }
        public string[] Filenames { get; private set; }
        public Texture Texture { get { return RenderMap?.Texture; } }

        /// <summary>
        /// Distance from a baseline to the next one.
        /// </summary>
        public float BaselinesDistance { get { return Math.Floor(ConvertEmToPx(RenderMap.LineHeight)); } }

        /// <summary>
        /// Distance to the upmost point relative to baseline.
        /// It should be negative, to respect Raccoon y-origin top-bottom direction.
        /// </summary>
        public float Ascender { get { return ConvertEmToPx(RenderMap.Ascender); } } //Math.Floor(ConvertEmToPx(RenderMap.Ascender)); } }

        /// <summary>
        /// Distance to the downmost point relative to baseline.
        /// </summary>
        public float Descender { get { return ConvertEmToPx(RenderMap.Descender); } } //Math.Ceiling(ConvertEmToPx(RenderMap.Descender)); } }

        /// <summary>
        /// Distance from the upmost point to the downmost point.
        /// It doesn't includes spacing between lines, to include it, use LineSpacing.
        /// </summary>
        public float LineHeight { get { return ConvertEmToPx(RenderMap.LineHeight); } }//Math.Floor(ConvertEmToPx(RenderMap.LineHeight)); } }

        public float SpaceBetweenLines {
            get {
                return BaselinesDistance - (Descender + Math.Abs(Ascender));
            }
        }

        /// <summary>
        /// Size ratio compared to render map nominal.
        /// </summary>
        public float SizeRatio { get { return Size / RenderMap.NominalWidth; } }

        /// <summary>
        /// Max size which a glyph can occupy.
        /// </summary>
        public Size MaxGlyphSize {
            get {
                return new Size(
                    Math.Floor(SizeRatio * RenderMap.GlyphSlotSize.Width),
                    Math.Floor(SizeRatio * RenderMap.GlyphSlotSize.Height)
                );
            }
        }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// A shareable render map.
        /// Multiple fonts instance can reference and use it to render glyphs at different settings.
        /// </summary>
        public FontRenderMap RenderMap { get; private set; }

        public Shader Shader { get { return RenderMap?.Shader; } }
        public IShaderParameters ShaderParameters { get; private set; }

        public string Filename {
            get {
                return Filenames?[0] ?? "";
            }

            private set {
                if (Filenames == null) {
                    Filenames = new string[1];
                }

                Filenames[0] = value;
            }
        }

        public float Size {
            get {
                return _size;
            }

            set {
                _size = value;

                if (ShaderParameters != null) {
                    if (ShaderParameters is IFontSizeShaderParameter fontSizeShaderParameter) {
                        fontSizeShaderParameter.FontSize = _size;
                    }
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public float ConvertEmToPx(float em) {
            return em * Size;
        }

        public double ConvertEmToPx(double em) {
            return em * Size;
        }

        public float ConvertPxToEm(float px) {
            return px / Size;
        }

        public double ConvertPxToEm(double px) {
            return px / Size;
        }

        public Text.RenderData PrepareTextRenderData(string text, out double textEmWidth, out double textEmHeight) {
            return RenderMap.PrepareTextRenderData(text, out textEmWidth, out textEmHeight);
        }

        public Text.RenderData PrepareTextRenderData(string text) {
            return RenderMap.PrepareTextRenderData(text, out _, out _);
        }

        public Vector2 MeasureText(string text) {
            RenderMap.PrepareTextRenderData(text, out double textEmWidth, out double textEmHeight);
            return new Vector2(textEmWidth, textEmHeight);
        }

        public bool CanRenderCharacter(uint c) {
            return RenderMap.Glyphs.ContainsKey(c);
        }

        public void Reload() {
            try {
                RenderMap?.Reload();
            } catch(System.Exception e) {
                throw e;
            }
        }

        public void Reload(Stream fontStream) {
            throw new System.NotImplementedException();

            /*
            try {
                SharpFont.Face currentFace = Face;
                Load(fontStream, FaceIndex);

                if (currentFace != null) {
                    currentFace.Dispose();
                }
            } catch(System.Exception e) {
                throw e;
            }
            */
        }

        /*
        public void Reload(Stream fontStream, int faceIndex) {
            try {
                SharpFont.Face currentFace = Face;
                Load(fontStream, faceIndex);

                if (currentFace != null) {
                    currentFace.Dispose();
                }
            } catch(System.Exception e) {
                throw e;
            }
        }
        */

        public Font Clone(float? size = null) {
            string[] filenames;

            if (Filenames != null) {
                filenames = new string[Filenames.Length];
                Filenames.CopyTo(filenames, 0);
            } else {
                filenames = null;
            }

            RenderMap?.AddReferenceCount();

            Font font = new Font() {
                Name = Name,
                Filenames = filenames,
                RenderMap = RenderMap,
                ShaderParameters = ShaderParameters?.Clone()
            };

            if (size.HasValue) {
                font.Size = size.Value;
            } else {
                font.Size = _size;
            }

            return font;
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            if (RenderMap != null) {
                RenderMap.Dispose();
                RenderMap = null;
            }

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Private Methods

        private void Load(Stream fontStream, int faceIndex, float size) {
            if (fontStream is FileStream fileStream) {
                Filename = fileStream.Name;
            }

            fontStream.Seek(0, SeekOrigin.End);
            long streamSize = fontStream.Position;
            fontStream.Seek(0, SeekOrigin.Begin);

            byte[] file = new byte[streamSize];

            if (streamSize > int.MaxValue) {
                byte[] buffer = new byte[int.MaxValue];
                int count;
                long offset = 0L;

                while (offset < streamSize) {
                    if (streamSize >= int.MaxValue) {
                        count = int.MaxValue;
                    } else {
                        count = (int) streamSize;
                    }

                    fontStream.Read(buffer, 0, count);
                    buffer.CopyTo(file, offset);
                    offset += count;
                }
            } else {
                fontStream.Read(file, 0, (int) streamSize);
            }

            RenderMap = new FontFaceRenderMap(file, faceIndex, (uint) Math.Round(size));
            ShaderParameters = RenderMap.CreateShaderParameters();
        }

        #endregion Private Methods
    }
}
