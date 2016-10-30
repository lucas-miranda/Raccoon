using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitives {
    public class Circle : Graphic {
        #region Constructors

        public Circle(int radius, Color color) {
            Radius = radius;
            Size = new Size(Radius * 2);
            Color = color;
            Load();
        }

        #endregion Constructors

        #region Public Properties

        public Vector2 Center { get { return Position + Radius - Origin; } set { Position = value - Radius + Origin; } }
        public int Radius { get; private set; }
        public Texture Texture { get; set; }

        #endregion Public Properties

        #region Public Methods
        
        public override void Render() {
            Game.Instance.Core.SpriteBatch.Draw(Texture.XNATexture, Position, null, null, Origin, Rotation, Scale, FinalColor, (SpriteEffects) Flipped, LayerDepth);
        }

        public override void Dispose() {
            if (Texture != null) {
                Texture.Dispose();
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal override void Load() {
            if (Game.Instance.Core.SpriteBatch == null) {
                return;
            }

            int w = (int) Width, h = (int) Height;
            Color[] data = new Color[(w + 1) * (h + 1)];
            Texture circleTexture = new Texture(w + 1, h + 1);

            // midpoint circle algorithm
            int x = Radius, y = 0, err = 0, x0 = Radius, y0 = Radius;
            while (x >= y) {
                data[x0 + x + (y0 + y) * (w + 1)] = Color.White;
                data[x0 + y + (y0 + x) * (w + 1)] = Color.White;
                data[x0 - y + (y0 + x) * (w + 1)] = Color.White;
                data[x0 - x + (y0 + y) * (w + 1)] = Color.White;
                data[x0 - x + (y0 - y) * (w + 1)] = Color.White;
                data[x0 - y + (y0 - x) * (w + 1)] = Color.White;
                data[x0 + y + (y0 - x) * (w + 1)] = Color.White;
                data[x0 + x + (y0 - y) * (w + 1)] = Color.White;

                y += 1;
                err += 1 + 2 * y;
                if (2 * (err - x) + 1 > 0) {
                    x -= 1;
                    err += 1 - 2 * x;
                }
            }

            circleTexture.SetData(data);
            Texture = circleTexture;
        }

        #endregion Internal Methods
    }
}
