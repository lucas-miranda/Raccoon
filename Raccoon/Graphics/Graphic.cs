using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public abstract class Graphic {
        #region Private Members

        private float opacity = 1f;
        private Color color = Color.White;
        private Color finalColor = Color.White;

        #endregion Private Members

        #region Public Properties

        public string Name { get; set; }
        
        public Vector2 Position { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Layer { get; set; }
        //public Shader Shader { get; set; }

        public Color Color {
            get {
                return finalColor;
            }
            set {
                color = value;
                finalColor = color * opacity;
            }
        }

        public float Opacity {
            get {
                return opacity;
            }
            set {
                opacity = Math.Clamp(value, 0, 1);
                finalColor = color * opacity;
            }
        }

        public bool FlippedBoth {
            get {
                return (Flipped & SpriteEffects.FlipHorizontally & SpriteEffects.FlipVertically) == (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
            }
            set {
                Flipped = (value ? SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically : SpriteEffects.None);
            }
        }

        public bool FlippedHorizontally {
            get {
                return (Flipped & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
            }
            set {
                Flipped |= (value ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
        }

        public bool FlippedVertically {
            get {
                return (Flipped & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            }
            set {
                Flipped |= (value ? SpriteEffects.FlipVertically : SpriteEffects.None);
            }
        }

        #endregion

        #region Internal Properties

        internal SpriteEffects Flipped { get; set; }

        #endregion

        #region Public Abstract Methods

        public abstract void Update(int delta);
        public abstract void Draw();

        #endregion

        #region Internal Abstract Methods

        internal abstract void Load();

        #endregion
    }
}
