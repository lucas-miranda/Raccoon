using System.IO;
using Raccoon.Fonts;

namespace Raccoon.Graphics {
    public class Font : IAsset {
        #region Private Members

        private float _size;

        #endregion Private Members

        #region Constructors

        static Font() {
            Service = new FontService();
        }

        public Font(string filename, float size = 12f) {
            if (string.IsNullOrEmpty(filename)) {
                throw new System.ArgumentException($"Invalid font filename '{filename}'");
            }

            Filename = filename;
            if (filename.Contains(".") && !Filename.Contains(Game.Instance.ContentDirectory)) {
                Filename = Path.Combine(Game.Instance.ContentDirectory, Filename);
            }

            Load();
            Size = size;
        }

        public Font(byte[] file, int faceIndex, float size = 12f) {
            FaceIndex = faceIndex;
            Face = new SharpFont.Face(Service.Library, file, faceIndex);
            Size = size;
        }

        public Font(SharpFont.Face fontFace) {
            Face = fontFace;
            FaceIndex = fontFace.FaceIndex;
            PrepareRenderMap();
        }

        public Font(Stream fontStream, int faceIndex) {
            Load(fontStream, faceIndex);
            PrepareRenderMap();
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; } = "Font";
        public string FamilyName { get { return Face.FamilyName; } }
        public string[] Filenames { get; private set; }
        public SharpFont.Face Face { get; private set; }
        public Texture Texture { get { return RenderMap?.Texture; } }

        /// <summary>
        /// Distance from a baseline to the next one.
        /// </summary>
        public float LineSpacing { get { return RenderMap.LineSpacing; } }

        /// <summary>
        /// Distance to the upmost point.
        /// </summary>
        public float LineAscent { get { return RenderMap.LineAscent; } }

        /// <summary>
        /// Distance to the downmost point.
        /// </summary>
        public float LineDescent { get { return RenderMap.LineDescent; } }

        /// <summary>
        /// Max size which a glyph can occupy.
        /// </summary>
        public Size MaxGlyphSize { get { return RenderMap.GlyphSlotSize; } }

        public bool IsDisposed { get; private set; }
        public int FaceIndex { get; private set; }
        public FontFaceRenderMap RenderMap { get; private set; }

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
                Face.SetCharSize(0, _size, 0, 96);
                PrepareRenderMap();
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal static FontService Service { get; }

        #endregion Internal Properties

        #region Public Methods

        public Text.RenderData GetRenderData(string text, out Size textSize) {
            return RenderMap.PrepareText(text, out textSize);
        }

        public Vector2 MeasureText(string text) {
            RenderMap.PrepareText(text, out Size textSize);
            return textSize.ToVector2();
        }

        public bool CanRenderCharacter(char c) {
            return RenderMap.Glyphs.ContainsKey(c);
        }

        public void Reload() {
            try {
                SharpFont.Face currentFace = Face;
                Load();

                if (currentFace != null) {
                    currentFace.Dispose();
                }
            } catch(System.Exception e) {
                throw e;
            }
        }

        public void Reload(Stream fontStream) {
            try {
                SharpFont.Face currentFace = Face;
                Load(fontStream, FaceIndex);

                if (currentFace != null) {
                    currentFace.Dispose();
                }
            } catch(System.Exception e) {
                throw e;
            }
        }

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

        public void Dispose() {
            if (Face == null || IsDisposed) {
                return;
            }

            if (Face != null && !Face.IsDisposed) {
                Face.Dispose();
                Face = null;
            }

            if (RenderMap != null && !RenderMap.IsDisposed) {
                RenderMap.Dispose();
                RenderMap = null;
            }

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Private Methods

        private void PrepareRenderMap() {
            if (RenderMap != null) {
                RenderMap.Dispose();
            }

            RenderMap = FontService.CreateFaceRenderMap(Face);
        }

        private void Load() {
            Face = new SharpFont.Face(Service.Library, Filename);
        }

        private void Load(Stream fontStream, int faceIndex) {
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

            FaceIndex = faceIndex;
            Face = new SharpFont.Face(Service.Library, file, faceIndex);
        }

        #endregion Private Methods
    }
}
