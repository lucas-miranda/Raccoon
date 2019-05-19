using Microsoft.Xna.Framework;

using Raccoon.Util;

namespace Raccoon {
    public class Camera {
        #region Public Members

        public event System.Action OnUpdate;

        #endregion Public Members

        #region Private Members

        private Vector2 _position;
        private float _zoom = 1f;
        private bool _needViewRefresh;
        private Vector3 _cameraUpVector = Vector3.Up;

        #endregion Private Members

        #region Constructors

        public Camera() {
        }

        #endregion Constructors

        #region Public Properties

        public static Camera Current { get; private set; }

        public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
        public Size Size { get { return Game.Instance.Size; } }
        public float Width { get { return Game.Instance.Width; } }
        public float Height { get { return Game.Instance.Height; } }
        public float Left { get { return X; } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public bool UseBounds { get; set; }
        public bool ClampValues { get; set; }
        public Rectangle Bounds { get; set; }
        public Vector2 Center { get { return Position + Game.Instance.Size / (2f * _zoom); } set { Position = value - Game.Instance.Size / (2f * _zoom); } }

        public Vector2 Position {
            get {
                return _position;
            }

            set {
                if (ClampValues) {
                    value = Math.Round(value);
                }

                _position = !UseBounds ? value : Math.Clamp(value, new Rectangle(Bounds.Position, Bounds.Size - Size));
                _needViewRefresh = true;
            }
        }

        public float Zoom {
            get {
                return _zoom;
            }

            set {
                if (_zoom == value) {
                    return;
                }

                float previousZoom = _zoom;
                _zoom = Math.Max(value, Math.Epsilon);
                _needViewRefresh = true;
                OnZoom(previousZoom, _zoom);
            }
        }

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

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
            Projection = Game.Instance.MainRenderer.RecalculateProjection();
            Game.Instance.DebugRenderer.RecalculateProjection();

            Vector3 cameraPos = new Vector3(Position, 0f),
                    cameraTarget = cameraPos + Vector3.Forward;

            Matrix.CreateLookAt(ref cameraPos, ref cameraTarget, ref _cameraUpVector, out Matrix _view);
            Game.Instance.MainRenderer.View = View = Matrix.CreateScale(Zoom, Zoom, 1f) * _view;
        }

        #endregion Private Members
    }
}
