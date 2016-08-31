using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;

namespace Raccoon.Graphics {
    public class Font {
        #region Constructors

        public Font(string name) {
            Name = name;
            if (Game.Instance.IsRunning) {
                Load();
            } else {
                Game.Instance.Core.OnLoadContent += new Core.GeneralHandler(() => { Load(); });
            }
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; private set; }
        public ReadOnlyCollection<char> Characters { get { return SpriteFont.Characters; } }
        public char? DefaultCharacter { get { return SpriteFont.DefaultCharacter; } set { SpriteFont.DefaultCharacter = value; } }
        public int LineSpacing { get { return SpriteFont.LineSpacing; } set { SpriteFont.LineSpacing = value; } }
        public float Spacing { get { return SpriteFont.Spacing; } set { SpriteFont.Spacing = value; } }

        #endregion Public Properties

        #region Internal Properties

        internal SpriteFont SpriteFont { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public Vector2 MeasureText(string text) {
            Microsoft.Xna.Framework.Vector2 dimensions = SpriteFont.MeasureString(text);
            return new Vector2(dimensions.X, dimensions.Y);

        }

        #endregion Public Methods

        #region Internal Methods

        internal void Load() {
            SpriteFont = Game.Instance.Core.Content.Load<SpriteFont>(Name);
        }

        #endregion Internal Methods
    }
}
