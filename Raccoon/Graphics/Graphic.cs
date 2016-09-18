﻿namespace Raccoon.Graphics {
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
        public Size Size { get; set; }
        public float Width { get { return Size.Width; } set { Size = new Size(value, Height); } }
        public float Height { get { return Size.Height; } set { Size = new Size(Width, value); } }
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
                return Flipped.HasFlag(ImageFlip.Horizontal | ImageFlip.Vertical);
            }

            set {
                Flipped = (value ? ImageFlip.Horizontal | ImageFlip.Vertical : ImageFlip.None);
            }
        }

        public bool FlippedHorizontally {
            get {
                return Flipped.HasFlag(ImageFlip.Horizontal);
            }

            set {
                Flipped |= (value ? ImageFlip.Horizontal : ImageFlip.None);
            }
        }

        public bool FlippedVertically {
            get {
                return Flipped.HasFlag(ImageFlip.Vertical);
            }

            set {
                Flipped |= (value ? ImageFlip.Vertical : ImageFlip.None);
            }
        }

        public ImageFlip Flipped { get; set; }

        #endregion

        #region Public Abstract Methods

        public abstract void Update(int delta);
        public abstract void Render();

        #endregion

        #region Internal Abstract Methods

        internal abstract void Load();

        #endregion
    }
}
