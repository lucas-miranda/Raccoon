using Microsoft.Xna.Framework;

using Raccoon.Graphics;
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
        private Matrix _projection, _view;
        private Size _previousProjectionSize;

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

                _position = !UseBounds ? value : Util.Math.Clamp(value, Bounds);
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

        public Vector2 ConvertScreenToWorld(Vector2 screenPosition) { 
            Vector3 worldPos = Game.Instance.GraphicsDevice.Viewport.Unproject( 
                new Vector3(screenPosition, 0f), 
                _projection, 
                _view, 
                Matrix.Identity
            ); 
 
            return new Vector2(worldPos.X, worldPos.Y); 
        } 
 
        public Vector2 ConvertWorldToScreen(Vector2 worldPosition) { 
            Vector3 screenPos = Game.Instance.GraphicsDevice.Viewport.Project( 
                new Vector3(worldPosition, 0f), 
                _projection, 
                _view, 
                Matrix.Identity
            ); 
 
            return new Vector2(screenPos.X, screenPos.Y); 
        } 

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnZoom(float previousZoom, float newZoom) {
        }

        #endregion Protected Methods

        #region Private Members

        private void Refresh() {
            Vector2 scale = new Vector2(Zoom);
            float scaleFactor = 1f / (Zoom * Game.Instance.PixelScale);

            Size projectionSize = new Size(Game.Instance.WindowWidth * scaleFactor, Game.Instance.WindowHeight * scaleFactor);
            if (projectionSize != _previousProjectionSize) {
                Matrix.CreateOrthographicOffCenter(0f, projectionSize.Width, projectionSize.Height, 0f, 0f, -1f, out _projection);
                _previousProjectionSize = projectionSize;
                
                Game.Instance.MainRenderer.Projection = _projection;
            }

            Vector3 cameraPos = new Vector3(Position, 0f),
                    cameraTarget = cameraPos + Vector3.Forward;

            Matrix.CreateLookAt(ref cameraPos, ref cameraTarget, ref _cameraUpVector, out _view);

            Game.Instance.MainRenderer.View = _view;
        }

        #endregion Private Members
    }
}
