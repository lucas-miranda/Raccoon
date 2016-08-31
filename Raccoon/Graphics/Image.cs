using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Image : Graphic {
        #region Constructors

        public Image() {
            TextureRect = Rectangle.Empty;
        }

        public Image(string path) : this() {
            Name = path;
            if (Game.Instance.IsRunning) {
                Load();
            }
        }

        #endregion Constructors

        #region Public Properties

        public Rectangle TextureRect { get; set; }

        #endregion Public Propeties

        #region Internal Properties

        internal Texture2D Texture { get; set; }

        #endregion Internal Properties

        #region Public Methods

        public override void Update(int delta) {
        }

        public override void Draw() {
            Game.Instance.Core.SpriteBatch.Draw(
                Texture,
                Position,
                null,
                TextureRect,
                Origin,
                Rotation,
                Scale,
                Color,
                Flipped,
                Layer
            );
        }

        #endregion

        #region Internal Methods

        internal override void Load() {
            Texture = Game.Instance.Core.Content.Load<Texture2D>(Name);
            TextureRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
        }

        #endregion Internal Methods
    }
}
