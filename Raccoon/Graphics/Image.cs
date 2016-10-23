using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Image : Graphic {
        #region Constructors

        public Image() {
            ClippingRegion = Rectangle.Empty;
        }

        public Image(string filename) {
            Name = filename;
            Load();
        }

        #endregion Constructors

        #region Public Properties

        public Rectangle ClippingRegion { get; set; }

        #endregion Public Propeties

        #region Internal Properties

        internal Texture2D Texture { get; set; }

        #endregion Internal Properties

        #region Public Methods

        public override void Update(int delta) {
        }

        public override void Render() {
            Game.Instance.Core.SpriteBatch.Draw(
                Texture,
                Position,
                null,
                ClippingRegion,
                Origin,
                Rotation,
                Scale,
                FinalColor,
                (SpriteEffects) Flipped,
                LayerDepth
            );
        }

        public override void Dispose() {
            if (Texture != null) {
                Texture.Dispose();
            }
        }

        #endregion

        #region Internal Methods

        internal override void Load() {
            if (Game.Instance.Core.SpriteBatch == null) {
                return;
            }

            Texture = Game.Instance.Core.Content.Load<Texture2D>(Name);
            Debug.Assert(Texture != null, $"Texture with name '{Name}' not found.");
            ClippingRegion = new Rectangle(0, 0, Texture.Width, Texture.Height);
        }

        #endregion Internal Methods
    }
}
