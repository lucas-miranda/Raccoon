using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitives {
    public class Rectangle : Graphic {
        #region Private Members

        private Texture2D _texture;
        private bool _filled;

        #endregion Private Members

        #region Constructors

        public Rectangle(float width, float height, Color color, bool filled = true) {
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
                base.Size = new Size(Math.Max(0, value.Width), Math.Max(0, value.Height));
                if (!Filled) {
                    if (_texture != null) {
                        _texture.Dispose();
                    }

                    NeedsReload = true;
                }
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
                    _texture.Dispose();
                }

                _filled = value;
                NeedsReload = true;
            }
        }

        #endregion Public Properties

        #region Public Methods
        
        public override void Render() {
            if (Filled) {
                Game.Instance.Core.SpriteBatch.Draw(_texture, new Microsoft.Xna.Framework.Rectangle((int) X, (int) Y, (int) Width, (int) Height), null, FinalColor, Rotation, Origin, (SpriteEffects) Flipped, LayerDepth);
            } else {
                Game.Instance.Core.SpriteBatch.Draw(_texture, Position, null, null, Origin, Rotation, Scale, FinalColor, (SpriteEffects) Flipped, LayerDepth);
            }
        }

        public override void Dispose() {
            if (_texture != null) {
                _texture.Dispose();
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal override void Load() {
            if (Game.Instance.Core.SpriteBatch == null) {
                return;
            }

            if (Filled) {
                _texture = Game.Instance.Core.SpriteBatch.BlankTexture();
            } else {
                int w = (int) Width, h = (int) Height;
                Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[w * h];
                Texture2D unfilledRectTexture = new Texture2D(Game.Instance.Core.GraphicsDevice, w, h);

                for (int x = 0; x < Width; x++) {
                    data[x] = Microsoft.Xna.Framework.Color.White;
                    data[x + (h - 1) * w] = Microsoft.Xna.Framework.Color.White;
                }

                for (int y = 1; y < Height - 1; y++) {
                    data[y * w] = Microsoft.Xna.Framework.Color.White;
                    data[w - 1 + y * w] = Microsoft.Xna.Framework.Color.White;
                }

                unfilledRectTexture.SetData(data);
                _texture = unfilledRectTexture;
            }
        }

        #endregion Internal Methods
    }
}
