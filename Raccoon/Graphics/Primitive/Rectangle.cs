namespace Raccoon.Graphics.Primitive {
    public class Rectangle : Image {
        public Rectangle(int width, int height, Color color) {
            Width = width;
            Height = height;
            Color = color;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public override void Render() {
            Game.Instance.Core.SpriteBatch.Draw(Texture, new Raccoon.Rectangle(X, Y, Width, Height), Color);
        }

        internal override void Load() {
            Texture = Game.Instance.Core.SpriteBatch.BlankTexture();
        }
    }
}
