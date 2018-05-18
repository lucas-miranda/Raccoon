namespace Raccoon.Graphics {
    public abstract class Graphic {
        #region Private Members

        private float _opacity = 1f;
        private Color _color = Color.White;

#if DEBUG
        private Vector2 _lastPosition, _lastScale;
        private float _lastRotation;
        private ImageFlip _lastFlip;
        private Color _lastColor;
        private Vector2 _lastScroll;
#endif

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
        public Vector2 Center { get { return Position + Size / 2f; } set { Position = value - Size / 2f; } }
        public Vector2 Origin { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public float ScaleXY { get { return Scale.X; } set { Scale = new Vector2(value); } }
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

        public static int LayerComparer(Graphic a, Graphic b) {
            return System.Math.Sign(a.Layer - b.Layer);
        }

        public virtual void Update(int delta) {
            if (NeedsReload) {
                Load();
                NeedsReload = false;
            }
        }

        public void Render() {
            Render(Vector2.Zero, 0, Vector2.One, ImageFlip.None, Color.White, Vector2.Zero, Shader);
        }

        public void Render(Vector2 position) {
            Render(position, 0, Vector2.One, ImageFlip.None, Color.White, Vector2.Zero, Shader);
        }

        public void Render(Vector2 position, float rotation) {
            Render(position, rotation, Vector2.One, ImageFlip.None, Color.White, Vector2.Zero, Shader);
        }

        public void Render(Vector2 position, float rotation, Vector2 scale) {
            Render(position, rotation, scale, ImageFlip.None, Color.White, Vector2.Zero, Shader);
        }

        public void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip) {
            Render(position, rotation, scale, flip, Color.White, Vector2.Zero, Shader);
        }

        public void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color) {
            Render(position, rotation, scale, flip, color, Vector2.Zero, Shader);
        }

        public virtual void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null) {
#if DEBUG
            _lastPosition = position;
            _lastRotation = rotation;
            _lastScale = scale;
            _lastFlip = flip;
            _lastColor = color;
            _lastScroll = scroll;
#endif
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void DebugRender() {
            DebugRender(_lastPosition, _lastRotation, _lastScale, _lastFlip, _lastColor, _lastScroll);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public virtual void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
        }

        public abstract void Dispose();

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Load() { }

        #endregion Protected Methods
    }
}
