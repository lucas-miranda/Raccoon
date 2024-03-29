﻿using Raccoon.Util;

namespace Raccoon.Graphics {
    public abstract class Graphic : IUpdatable, IRenderable, System.IDisposable {
        #region Public Members

        public static int LayersCount = 65536; // 16 bits layers [-32768, 32768]

        #endregion Public Members

        #region Private Members

        private float _opacity = 1f;

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
            Renderer = Game.Instance.MainRenderer;
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
#if DEBUG
        public bool IgnoreDebugRender { get; set; }
#endif
        public bool IsDisposed { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 Center { get { return Position + Size / 2f; } set { Position = value - Size / 2f; } }
        public Vector2 Origin { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;
        public float ScaleXY { get { return Math.Max(Scale.X, Scale.Y); } set { Scale = new Vector2(value); } }
        public int Order { get; set; }
        public int Layer { get; set; }
        public ControlGroup ControlGroup { get; private set; }
        public float X { get { return Position.X; } set { Position = new Vector2(value, Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(X, value); } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }

        /// <summary>
        /// Clockwise rotation in degrees.
        /// </summary>
        public float Rotation { get; set; }

        public virtual Size Size { get; protected set; }
        public virtual Renderer Renderer { get; set; }
        public Vector2 Scroll { get; set; } = Vector2.One;
        public Shader Shader { get; set; }
        public IShaderParameters ShaderParameters { get; set; }
        public ImageFlip Flipped { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Opacity { get { return _opacity; } set { _opacity = Math.Clamp(value, 0f, 1f); } }

        public float LayerDepth {
            get {
                return ConvertLayerToLayerDepth(Layer);
            }

            set {
                Layer = ConvertLayerDepthToLayer(value);
            }
        }

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
                Flipped = value ? (Flipped | ImageFlip.Horizontal) : (Flipped & ~ImageFlip.Horizontal);
            }
        }

        public bool FlippedVertically {
            get {
                return Flipped.HasFlag(ImageFlip.Vertical);
            }

            set {
                Flipped = value ? (Flipped | ImageFlip.Vertical) : (Flipped & ~ImageFlip.Vertical);
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected bool NeedsReload { get; set; }

        #endregion Protected Properties

        #region Public Methods

        public static int LayerComparer(Graphic a, Graphic b) {
            return a.Layer.CompareTo(b.Layer);
        }

        public static float ConvertLayerToLayerDepth(int layer) {
            return (LayersCount / 2 - layer) / (float) LayersCount;
        }

        public static int ConvertLayerDepthToLayer(float layerDepth) {
            return (LayersCount / 2) - (int) (layerDepth * LayersCount);
        }

        public virtual void Update(int delta) {
            if (NeedsReload) {
                Load();
                NeedsReload = false;
            }
        }

        public void Render() {
            InternalRender(
                Position,
                Rotation,
                Scale,
                Flipped,
                Color,
                Scroll,
                Shader,
                Layer
            );
        }

        public void Render(Vector2 position, Shader shader = null, int? layer = null) {
            InternalRender(
                position,
                Rotation,
                Scale,
                Flipped,
                Color,
                Scroll,
                shader ?? Shader,
                layer ?? Layer
            );
        }

        public void Render(Vector2 position, float rotation, Shader shader = null, int? layer = null) {
            InternalRender(
                position,
                rotation,
                Scale,
                Flipped,
                Color,
                Scroll,
                shader ?? Shader,
                layer ?? Layer
            );
        }

        public void Render(Vector2 position, float rotation, Vector2 scale, Shader shader = null, int? layer = null) {
            InternalRender(
                position,
                rotation,
                scale,
                Flipped,
                Color,
                Scroll,
                shader ?? Shader,
                layer ?? Layer
            );
        }

        public void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Shader shader = null, int? layer = null) {
            InternalRender(
                position,
                rotation,
                scale,
                flip,
                Color,
                Scroll,
                shader ?? Shader,
                layer ?? Layer
            );
        }

        public void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Shader shader = null, int? layer = null) {
            InternalRender(
                position,
                rotation,
                scale,
                flip,
                color,
                Scroll,
                shader ?? Shader,
                layer ?? Layer
            );
        }

        public void Render(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader = null, int? layer = null) {
            InternalRender(
                position,
                rotation,
                scale,
                flip,
                color,
                scroll,
                shader ?? Shader,
                layer ?? Layer
            );
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void DebugRender() {
            DebugRender(_lastPosition, _lastRotation, _lastScale, _lastFlip, _lastColor, _lastScroll);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public virtual void DebugRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll) {
        }

        public void CentralizeOrigin() {
            Origin = new Vector2(Size.Width / 2f, Size.Height / 2f);
        }

        public void CentralizeOriginRounded() {
            Origin = new Vector2(Math.Round(Size.Width / 2f), Math.Round(Size.Height / 2f));
        }

        public virtual void Paused() {
        }

        public virtual void Resumed() {
        }

        public virtual void ControlGroupRegistered(ControlGroup controlGroup) {
            ControlGroup = controlGroup;
        }

        public virtual void ControlGroupUnregistered() {
            ControlGroup = null;
        }

        public virtual void Dispose() {
            if (IsDisposed) {
                return;
            }

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Load() {
        }

        protected virtual void BeforeDraw() {
        }

        protected abstract void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth);

        protected virtual void AfterDraw() {
        }

        #endregion Protected Methods

        #region Private Methods

        private void InternalRender(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, int layer) {
#if DEBUG
            _lastPosition = position;
            _lastRotation = rotation;
            _lastScale = scale;
            _lastFlip = flip;
            _lastColor = color;
            _lastScroll = scroll;
#endif

            if (NeedsReload) {
                Load();
                NeedsReload = false;
            }

            BeforeDraw();
            Draw(position, rotation, scale, flip, color, scroll, shader, ShaderParameters, Origin, ConvertLayerToLayerDepth(layer));
            AfterDraw();
        }

        #endregion Private Methods
    }
}
