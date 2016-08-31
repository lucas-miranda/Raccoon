using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics.Primitive {
    public class Rectangle : Image {
        public Rectangle(int width, int height, Color color) {
            Width = width;
            Height = height;
            Color = color;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public new Color Color { get; private set; }

        internal override void Load() {
            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[Width * Height];
            Texture = new Texture2D(Game.Instance.Core.GraphicsDevice, Width, Height);
            Microsoft.Xna.Framework.Color pxColor = new Microsoft.Xna.Framework.Color(Color.R, Color.G, Color.B, Color.A);
            for (int i = 0; i < data.Length; ++i) {
                data[i] = pxColor;
            }

            Texture.SetData(data);
            TextureRect = new Raccoon.Rectangle(0, 0, Texture.Width, Texture.Height);
        }
    }
}
