using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitives {
    public class Rectangle : Graphic {
        #region Private Members

        private bool _filled;

        #endregion Private Members

        #region Constructors

        public Rectangle(float width, float height, Color color, bool filled = true) : base() {
            Size = new Size(width, height);
            Color = color;
            _filled = filled;
            Load();
        }

        #endregion Constructors

        #region Public Properties

        public new Size Size {
            get {
                return base.Size;
            }

            set {
                if (!Filled && Size != value) {
                    NeedsReload = true;
                }

                base.Size = value;
            }
        }
        
        public bool Filled {
            get {
                return _filled;
            }

            set {
                if (value == _filled) {
                    return;
                }

                if (!_filled) {
                    Texture.Dispose();
                }

                _filled = value;
                NeedsReload = true;
            }
        }

        public Texture Texture { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public override void Render(Vector2 position, float rotation) {
            if (Filled) {
                Surface.Draw(Texture, new Raccoon.Rectangle(position, Size * Scale), null, FinalColor, rotation * Util.Math.DegToRad, Origin / Size.ToVector2(), Flipped, Scroll, Shader);
                return;
            }

            Surface.Draw(Texture, position, null, FinalColor, rotation * Util.Math.DegToRad, Origin, Scale, Flipped, Scroll, Shader);
        }

        public override void Dispose() {
            if (!_filled && Texture != null) {
                Texture.Dispose();
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            if (Game.Instance.Core.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Rectangle needs a valid graphics device. Maybe are you creating before Scene.Start() is called?");
            }

            if (Filled) {
                Texture = Texture.White;
            } else {
                if (Texture != null && Texture != Texture.White && Texture != Texture.Black) {
                    Texture.Dispose();
                }

                int w = (int) Width, h = (int) Height;
                Color[] data = new Color[w * h];
                Texture unfilledRectTexture = new Texture(w, h);

                // left & right columns
                for (int x = 0; x < Width; x++) {
                    data[x] = Color.White;
                    data[x + (h - 1) * w] = Color.White;
                }

                // top & bottom rows
                for (int y = 1; y < Height - 1; y++) {
                    data[y * w] = Color.White;
                    data[w - 1 + y * w] = Color.White;
                }

                unfilledRectTexture.SetData(data);
                Texture = unfilledRectTexture;
            }
        }

        #endregion Protected Methods
    }
}
