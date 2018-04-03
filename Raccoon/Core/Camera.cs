using System;

using Microsoft.Xna.Framework;

namespace Raccoon {
    public class Camera {
        #region Public Members

        public event Action OnUpdate;

        #endregion Public Members

        #region Private Members

        private Vector2 _position;
        private float _zoom = 1f;
        private bool _needViewRefresh;

        #endregion Private Members

        #region Constructors

        public Camera() {
        }

        #endregion Constructors

        #region Public Properties

        public static Camera Current { get; private set; }

        public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
        public Size Size { get { return Game.Instance.ScreenSize; } }
        public float Width { get { return Game.Instance.ScreenWidth; } }
        public float Height { get { return Game.Instance.ScreenHeight; } }
        public float Left { get { return X; } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public bool UseBounds { get; set; }
        public bool ClampValues { get; set; }
        public Rectangle Bounds { get; set; }
        public Vector2 Center { get { return Position + Game.Instance.ScreenSize / (2f * _zoom); } set { Position = value - Game.Instance.ScreenSize / (2f * _zoom); } }

        public Vector2 Position {
            get {
                return _position;
            }

            set {
                if (ClampValues) {
                    value = new Vector2((float) Math.Round(value.X), (float) Math.Round(value.Y));
                }

                _position = !UseBounds ? value : Util.Math.Clamp(value, Bounds);
                _needViewRefresh = true;
            }
        }

        public float Zoom {
            get {
                return _zoom;
            }

            set {
                _zoom = value;
                _needViewRefresh = true;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Start() {
            Current = this;
            _needViewRefresh = true;
        }
        
        public virtual void Begin() {
            if (Current != this) {
                Current = this;
            }

            _needViewRefresh = true;
        }

        public virtual void End() { }

        public virtual void Update(int delta) {
            OnUpdate?.Invoke();
        }

        public virtual void PrepareRender() {
            if (_needViewRefresh) {
                Refresh();
                _needViewRefresh = false;
            }
        }

        public virtual void DebugRender() { }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnZoom(float previousZoom, float newZoom) {
        }

        #endregion Protected Methods

        #region Private Members

        private void Refresh() {
            Game.Instance.MainSurface.Scale = new Vector2(Zoom * Game.Instance.Scale);
            Game.Instance.MainSurface.View = Matrix.CreateTranslation(-X, -Y, 0f) * Game.Instance.MainSurface.View;

#if DEBUG
            Game.Instance.DebugSurface.View = Matrix.CreateTranslation(-X * Game.Instance.Scale * Zoom, -Y * Game.Instance.Scale * Zoom, 0);
#endif
        }

        #endregion Private Members
    }
}
