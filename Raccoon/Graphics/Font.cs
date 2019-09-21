using Raccoon.Fonts;

namespace Raccoon.Graphics {
    public class Font : System.IDisposable {
        #region Private Memebers

        private float _size;

        #endregion Private Memebers

        #region Constructors

        static Font() {
            Service = new FontService();
        }

        public Font(string name, float size = 12f) {
            Face = new SharpFont.Face(Service.Library, System.IO.Path.Combine(Game.Instance.ContentDirectory, name));
            Size = size;
        }

        public Font(byte[] file, int faceIndex, float size = 12f) {
            Face = new SharpFont.Face(Service.Library, file, faceIndex);
            Size = size;
        }

        public Font(SharpFont.Face fontFace) {
            Face = fontFace;
            PrepareRenderMap();
        }

        #endregion Constructors

        #region Public Properties

        public string FamilyName { get { return Face.FamilyName; } }
        public SharpFont.Face Face { get; private set; }
        public Texture Texture { get { return RenderMap?.Texture; } }
        public float LineSpacing { get { return RenderMap.NominalHeight; } }
        public float MaxGlyphWidth { get { return FontService.ConvertEMToPx(Face.BBox.Right - Face.BBox.Left, RenderMap.NominalWidth, Face.UnitsPerEM); } }
        public bool IsDisposed { get; private set; }

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

        internal FontFaceRenderMap RenderMap { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public Vector2 MeasureText(string text) {
            return RenderMap.MeasureString(text).ToVector2();
        }

        public bool CanRenderCharacter(char c) {
            return RenderMap.Glyphs.ContainsKey(c);
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

        #endregion Private Methods
    }
}
