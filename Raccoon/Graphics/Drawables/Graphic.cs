﻿namespace Raccoon.Graphics {
    public abstract class Graphic {
        #region Public Members

        public readonly int LayerMin = -LayerLimit / 2;
        public readonly int LayerMax = LayerLimit / 2;

        #endregion Public Members

        #region Protected Members

        protected const int LayerLimit = 20000;

        #endregion Protected Members

        #region Private Members

        private float _opacity = 1f;
        private Color _color = Color.White;

        #endregion Private Members

        #region Public Properties

        public string Name { get; set; }
        public bool Visible { get; set; } = true;
        public Vector2 Position { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public Size Size { get; protected set; }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public int Layer { get { return (int) System.Math.Round((LayerDepth * LayerLimit) - LayerMax); } set { LayerDepth = (float) (value + LayerMax) / LayerLimit; } }
        public float LayerDepth { get; set; }
        public Color FinalColor { get; set; } = Color.White;
        //public Shader Shader { get; set; }

        public Color Color {
            get {
                return _color;
            }

            set {
                _color = new Color(value.R, value.G, value.B);
                FinalColor = _color * Opacity;
            }
        }

        public float Opacity {
            get {
                return _opacity;
            }

            set {
                _opacity = Util.Math.Clamp(value, 0, 1);
                FinalColor = Color * _opacity;
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
                if (value) {
                    Flipped |= ImageFlip.Horizontal;
                } else {
                    Flipped &= ~ImageFlip.Horizontal;
                }
            }
        }

        public bool FlippedVertically {
            get {
                return Flipped.HasFlag(ImageFlip.Vertical);
            }

            set {
                if (value) {
                    Flipped |= ImageFlip.Vertical;
                } else {
                    Flipped &= ~ImageFlip.Vertical;
                }
            }
        }

        public ImageFlip Flipped { get; set; }

        #endregion Public Properties

        #region Protected Properties

        protected bool NeedsReload { get; set; }

        #endregion Protected Properties

        #region Public Methods

        public void Render() {
            Render(Position, Rotation);
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void Update(int delta) {
            if (NeedsReload) {
                Load();
                NeedsReload = false;
            }
        }

        #endregion Public Virtual Methods

        #region Public Abstract Methods

        public abstract void Render(Vector2 position, float rotation);
        public abstract void Dispose();

        #endregion Public Abstract Methods

        #region Public Virtual Methods

        public virtual void DebugRender() { }

        #endregion Public Virtual Methods

        #region Protected Methods

        protected virtual void Load() { }

        #endregion Protected Methods
    }
}