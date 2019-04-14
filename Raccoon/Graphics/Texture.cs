using System.IO;

using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Texture : System.IDisposable {
        #region Private Static Members

        private static Texture _white, _black;

        #endregion Private Static  Members

        #region Private Members

        private bool _isFromContentManager;

        #endregion Private Members

        #region Constructors

        public Texture() {
        }

        public Texture(int width, int height) {
            if (width <= 0) {
                throw new System.ArgumentException("Value must be greater than 0", "width");
            }

            if (height <= 0) {
                throw new System.ArgumentException("Value must be greater than 0", "height");
            }

            Load(width, height);
        }

        public Texture(string filename) {
            Load(filename);
        }

        /*public Texture(Texture texture) {
            XNATexture = texture.XNATexture;
            Name = texture.Name;
            Bounds = texture.Bounds;
            Size = texture.Size;
        }*/

        internal Texture(Texture2D texture) {
            XNATexture = texture;
            Name = XNATexture.Name;
            Bounds = new Rectangle(0, 0, XNATexture.Width, XNATexture.Height);
            Size = Bounds.Size;
        }

        #endregion Constructors

        #region Public Static Properties

        public static Texture White {
            get {
                if (_white == null) {
                    _white = new Texture(1, 1);
                    _white.SetData(new Color[] { Color.White });
                }

                return _white;
            }
        }

        public static Texture Black {
            get {
                if (_black == null) {
                    _black = new Texture(1, 1);
                    _black.SetData(new Color[] { new Color(0, 0, 0, 0) });
                }

                return _black;
            }
        }

        #endregion Public Static Properties

        #region Public Properties

        public string Name { get; set; }
        public Rectangle Bounds { get; private set; }
        public Size Size { get; private set; }
        public int Width { get { return (int) Size.Width; } }
        public int Height { get { return (int) Size.Height; } }
        public string Filename { get; private set; }
        public Texture2D XNATexture { get; private set; }
        public bool IsDisposed { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void GetData<T>(T[] data) where T : struct {
            XNATexture.GetData(data);
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct {
            XNATexture.GetData(data, startIndex, elementCount);
        }

        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct {
            XNATexture.GetData(level, rect, data, startIndex, elementCount);
        }

        public void GetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct {
            XNATexture.GetData(level, arraySlice, rect, data, startIndex, elementCount);
        }

        public void SetData<T>(T[] data) where T : struct {
            XNATexture.SetData(data);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct {
            XNATexture.SetData(data, startIndex, elementCount);
        }

        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct {
            XNATexture.SetData(level, rect, data, startIndex, elementCount);
        }

        public void SetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct {
            XNATexture.SetData(level, arraySlice, rect, data, startIndex, elementCount);
        }

        public void Reload(Stream textureStream) {
            XNATexture.Reload(textureStream);
        }

        public void SaveAsJpeg(Stream stream, int width, int height) {
            XNATexture.SaveAsJpeg(stream, width, height);
        }

        public void SaveAsPng(Stream stream, int width, int height) {
            XNATexture.SaveAsPng(stream, width, height);
        }

        public void Reload() {
            if (string.IsNullOrWhiteSpace(Filename)) {
                return;
            }

            if (_isFromContentManager) {
                XNATexture = Game.Instance.XNAGameWrapper.Content.Load<Texture2D>(Filename);
            } else {
                XNATexture.Dispose();

                using (FileStream stream = File.Open(Game.Instance.ContentDirectory + Filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    XNATexture = Texture2D.FromStream(Game.Instance.XNAGameWrapper.GraphicsDevice, stream);
                }
            }

            if (XNATexture == null) {
                throw new System.NullReferenceException($"Texture '{Filename}' not found");
            }

            Bounds = new Rectangle(0, 0, XNATexture.Width, XNATexture.Height);
            Size = Bounds.Size;
        }

        public void Dispose() {
            if (_isFromContentManager || XNATexture == null || IsDisposed) {
                return;
            }

            if (!XNATexture.IsDisposed) {
                XNATexture.Dispose();
            }

            XNATexture = null;
            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void Load(int width, int height) {
            if (Game.Instance.XNAGameWrapper.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Texture needs a valid graphics device initialized. Maybe are you creating before first Scene.Start() is called?");
            }

            XNATexture = new Texture2D(Game.Instance.XNAGameWrapper.GraphicsDevice, width, height);
            Bounds = new Rectangle(0, 0, XNATexture.Width, XNATexture.Height);
            Size = Bounds.Size;
        }

        protected void Load(string filename) {
            if (Game.Instance.XNAGameWrapper.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Texture needs a valid graphics device initialized. Maybe are you creating before first Scene.Start() is called?");
            }

            Filename = filename;
            if (Filename.EndsWith(".bmp") || Filename.EndsWith(".gif") || Filename.EndsWith(".jpg") || Filename.EndsWith(".png") || Filename.EndsWith(".tif") || Filename.EndsWith(".dds")) {
                using (FileStream stream = File.Open(Game.Instance.ContentDirectory + Filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    XNATexture = Texture2D.FromStream(Game.Instance.XNAGameWrapper.GraphicsDevice, stream);
                }
            } else {
                _isFromContentManager = true;
                XNATexture = Game.Instance.XNAGameWrapper.Content.Load<Texture2D>(Filename);
            }

            if (XNATexture == null) {
                throw new System.NullReferenceException($"Texture '{Filename}' not found");
            }

            Bounds = new Rectangle(0, 0, XNATexture.Width, XNATexture.Height);
            Size = Bounds.Size;
        }

        #endregion Internal Methods
    }
}
