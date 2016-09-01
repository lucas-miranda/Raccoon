using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raccoon {
    public static class SpriteBatchExtensions {
        private static Texture2D _blankTexture;

        public static Texture2D BlankTexture(this SpriteBatch spriteBatch) {
            if (_blankTexture == null) {
                _blankTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _blankTexture.SetData(new Color[] { Color.White });
            }

            return _blankTexture;
        }
    }
}
