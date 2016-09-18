using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitive {
    public class Circle : Image {
        public Circle(int radius, Color color) {
            Radius = radius;
            Width = Height = Radius * 2;
            Color = color;
        }

        public Vector2 Center { get { return Position + Radius; } set { Position = value - Radius; } }
        public int Radius { get; private set; }

        public override void Render() {
            Game.Instance.Core.SpriteBatch.Draw(Texture, Position, Color);
        }

        internal override void Load() {
            int w = (int) Width, h = (int) Height;
            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[(w + 1) * (h + 1)];
            Texture2D circleTexture = new Texture2D(Game.Instance.Core.GraphicsDevice, w + 1, h + 1);

            // midpoint circle algorithm
            int x = Radius, y = 0, err = 0, x0 = Radius, y0 = Radius;
            while (x >= y) {
                data[x0 + x + (y0 + y) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 + y + (y0 + x) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 - y + (y0 + x) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 - x + (y0 + y) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 - x + (y0 - y) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 - y + (y0 - x) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 + y + (y0 - x) * (w + 1)] = Microsoft.Xna.Framework.Color.White;
                data[x0 + x + (y0 - y) * (w + 1)] = Microsoft.Xna.Framework.Color.White;

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
    }
}
