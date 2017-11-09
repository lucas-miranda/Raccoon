using System.Collections.Generic;

namespace Raccoon.Graphics {
    public abstract class Graphic {
        #region Private Members

        private float _opacity = 1f;
        private Color _color = Color.White;

        #endregion Private Members

        #region Constructors

        public Graphic() {
            Surface = Game.Instance.Core.MainSurface;
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public bool Visible { get; set; } = true;
        public bool IgnoreDebugRender { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public int Layer { get; set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }
        public float Rotation { get; set; }
        public Size Size { get; protected set; }
        public Surface Surface { get; set; }
        public Vector2 Scroll { get; set; } = Vector2.One;
        public Shader Shader { get; set; }
        public ImageFlip Flipped { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Opacity { get { return _opacity; } set { _opacity = Util.Math.Clamp(value, 0f, 1f); } }

        public bool FlippedBoth {
            get {
                return Flipped.HasFlag(ImageFlip.Both);
            }

            set {
                Flipped = value ? ImageFlip.Both : ImageFlip.None;
            }
        }

        public bool FlippedHorizontally {
            get {
                return Flipped.HasFlag(ImageFlip.Horizontal);
            }

            set {
                Flipped = value ? Flipped | ImageFlip.Horizontal : Flipped & ~ImageFlip.Horizontal;
            }
        }

        public bool FlippedVertically {
            get {
                return Flipped.HasFlag(ImageFlip.Vertical);
            }

            set {
                Flipped = value ? Flipped | ImageFlip.Vertical : Flipped & ~ImageFlip.Vertical;
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected bool NeedsReload { get; set; }

        #endregion Protected Properties

        #region Public Methods

        public void Render() {
            Render(Position, Color, Rotation);
        }

        public void Render(Vector2 position) {
            Render(position, Color, Rotation);
        }

        public void Render(Vector2 position, Color color) {
            Render(position, color, Rotation);
        }

        public void Render(Vector2 position, float rotation) {
            Render(position, Color, rotation);
        }

        #endregion Public Methods

        #region Public Virtual Methods

        public virtual void Update(int delta) {
            if (NeedsReload) {
                Load();
                NeedsReload = false;
            }
        }

        public virtual void DebugRender() { }

        #endregion Public Virtual Methods

        #region Public Abstract Methods

        public abstract void Render(Vector2 position, Color color, float rotation);
        public abstract void Dispose();

        #endregion Public Abstract Methods

        #region Protected Methods

        protected virtual void Load() { }

        #endregion Protected Methods

        #region Layer Comparer

        public class LayerComparer : IComparer<Graphic> {
            public int Compare(Graphic x, Graphic y) {
                return System.Math.Sign(x.Layer - y.Layer);
            }
        }

        #endregion Layer Comparer
    }
}
