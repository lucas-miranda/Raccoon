using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitive {
    public class Rectangle : Image {
        private bool _filled;

        public Rectangle(int width, int height, Color color, bool filled = true) {
            Width = width;
            Height = height;
            Color = color;
            _filled = filled;
            if (Game.Instance.IsRunning)
                Load();
        }

        public bool Filled {
            get {
                return _filled;
            }

            set {
                if (value == _filled)
                    return;

                if (!_filled)
                    Texture.Dispose();

                _filled = value;
                if (Game.Instance.Core.Graphics != null)
                    Load();
            }
        }

        public override void Render() {
            if (Filled) {
                Game.Instance.Core.SpriteBatch.Draw(Texture, new Raccoon.Rectangle(X, Y, Width, Height), Color);
            } else {
                Game.Instance.Core.SpriteBatch.Draw(Texture, Position, Color);
            }
        }

        internal override void Load() {
            if (Filled) {
                Texture = Game.Instance.Core.SpriteBatch.BlankTexture();
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
                Texture = unfilledRectTexture;
            }
        }
    }
}
