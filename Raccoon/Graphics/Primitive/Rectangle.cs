namespace Raccoon.Graphics.Primitive {
    public class Rectangle : Image {
        public Rectangle(int width, int height, Color color) {
            Width = width;
            Height = height;
            Color = color;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Raccoon.Rectangle Rect { get { return new Raccoon.Rectangle(X, Y, Width, Height); } }

        public override void Render() {
            Game.Instance.Core.SpriteBatch.Draw(Texture, Rect, Color);
        }

        internal override void Load() {
            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[Width * Height];
            Texture = Game.Instance.Core.SpriteBatch.BlankTexture();
        }
    }
}
