namespace Raccoon.Graphics {
    public class Font : System.IDisposable {
        private float _size;

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

        internal Font(SharpFont.Face fontFace) {
            Face = fontFace;
        }

        #endregion Constructors

        #region Public Properties

        public string FamilyName { get { return Face.FamilyName; } }
        public SharpFont.Face Face { get; private set; }
        public float LineSpacing { get { return 0; } }
        public float Spacing { get { return 0; } }
        public bool IsDisposed { get; private set; }

        public float Size {
            get {
                return _size;
            }

            set {
                _size = value;
                Face.SetCharSize(0, _size, 0, 96);
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal static FontService Service { get; }

        #endregion Internal Properties

        #region Public Methods

        public Vector2 MeasureText(string text) {
            return FontService.MeasureString(Face, text).ToVector2();
        }

        public Texture Rasterize(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, string text, out Size textSize) {
            System.Drawing.Bitmap bitmap = Service.RasterizeString(Face, text, System.Drawing.Color.White, out textSize);

            if (bitmap == null) {
                throw new System.InvalidOperationException("FontService can't create a valid Bitmap.");
            }

            return SpriteBatch.ConvertBitmapToTexture(graphicsDevice, bitmap);
        }

        public Texture Rasterize(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, string text) {
            return Rasterize(graphicsDevice, text, out _);
        }

        public void Dispose() {
            if (Face == null || IsDisposed) {
                return;
            }

            if (!Face.IsDisposed) {
                Face.Dispose();
                Face = null;
            }

            IsDisposed = true;
        }

        #endregion Public Methods

    }
}
