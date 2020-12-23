using Microsoft.Xna.Framework;

using Raccoon.Util;

namespace Raccoon {
    public class Camera : System.IDisposable {
        #region Public Members

        public event System.Action OnUpdate;

        #endregion Public Members

        #region Private Members

        private Vector2 _position, _displacement;
        private float _zoom = 1f;
        private bool _needViewRefresh, _isZoomViewingOutOfBounds;
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

        /// <summary>
        /// Camera Size, in world space, value is always the same as Game size.
        /// </summary>
        public Size Size { get { return Game.Instance.Size; } }

        /// <summary>
        /// Camera Size, in screen space, it's value is modified by zoom.
        /// </summary>
        public Size VirtualSize { get { return Size / Zoom; } }
        public float Width { get { return Game.Instance.Width; } }
        public float Height { get { return Game.Instance.Height; } }
        public float Left { get { return X; } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public bool UseBounds { get; set; }
        public bool ClampValues { get; set; }
        public bool IsDisposed { get; private set; }
        public Rectangle Bounds { get; set; }

        public Vector2 Center { 
            get { 
                return Position + Game.Instance.Size / (2f * _zoom); 
            } 

            set { 
                Position = value - Game.Instance.Size / (2f * _zoom); 
            } 
        }

        public Vector2 Position {
            get {
                return _position;
            }

            set {
                if (ClampValues) {
                    value = Math.Round(value);
                }

                if (UseBounds) {
                    float moveSpaceWidth = Bounds.Width - VirtualSize.Width,
                          moveSpaceHeight = Bounds.Height - VirtualSize.Height;

                    if (moveSpaceWidth > Math.Epsilon || moveSpaceHeight > Math.Epsilon) {
                        _position = Math.Clamp(
                            value, 
                            new Rectangle(
                                Bounds.Position, 
                                new Size(Math.Max(0f, moveSpaceWidth), Math.Max(0f, moveSpaceHeight))
                            )
                        );
                    } else {
                        _position = Bounds.Position;
                    }
                } else {
                    _position = value;
                }

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

        /// <summary>
        /// A special value to apply some kind of motion effect at Camera without messing with Position value directly.
        /// It doesn't respects Bounds or anything similar.
        /// </summary>
        public Vector2 Displacement {
            get {
                return _displacement;
            }

            set {
                _displacement = value;
                _needViewRefresh = true;
            }
        }

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public virtual void Start() {
            Current = this;
            _needViewRefresh = true;
            Displacement = Vector2.Zero;
        }

        public virtual void Begin() {
            if (Current != this) {
                Current = this;
            }

            _needViewRefresh = true;
        }

        public virtual void End() { 
        }

        public virtual void Update(int delta) {
            OnUpdate?.Invoke();
        }

        public virtual void PrepareRender() {
            if (_needViewRefresh) {
                Refresh();
                _needViewRefresh = false;
            }
        }

        public virtual void DebugRender() { 
        }

        public virtual void Dispose() {
            if (IsDisposed) {
                return;
            }

            OnUpdate = null;

            IsDisposed = true;
        }

        public void ClearEvents() {
            OnUpdate = null;
        }

        public virtual void Reset() {
            Position = Bounds.TopLeft;
            Displacement = Vector2.Zero;
        }

        public bool CanSee(Entity entity) {
            if (entity == null) {
                throw new System.ArgumentNullException(nameof(entity));
            }

            return new Rectangle(Position, VirtualSize)
                       .Contains(entity.Transform.Position + CalculateEntityBounds(entity));
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Calculate an Entity bounds (at local space) using it's Graphic or Body, at this exact order.
        /// Has an optional fallback size, when everyelse fails.
        /// </summary>
        /// <param name="entity">Entity to calculate it's bounds.</param>
        /// <param name="customBody">An optional custom body to use, instead using first entity's Body found.</param>
        /// <param name="defaultSize">Default fallback size when there is no Graphic or Body to use.</param>
        /// <returns>Entity bounds at local space.</returns>
        protected Rectangle CalculateEntityBounds(Entity entity, Components.Body customBody = null, Size? defaultSize = null) {
            if (customBody != null && customBody.Entity != entity) {
                throw new System.ArgumentException($"Custom body must belongs to provided entity '{entity}'.");
            }

            if (entity.Graphic != null && entity.Graphic.Visible) {
                Raccoon.Graphics.Graphic g = entity.Graphic;

                if (g is Raccoon.Graphics.Animation animation) {
                    return new Rectangle(-(g.Origin + (animation.CurrentTrack?.CurrentFrameDestination.Position ?? Vector2.Zero)) + g.Position, g.Size * g.ScaleXY);
                }

                return new Rectangle(-g.Origin + g.Position, g.Size * g.ScaleXY);
            } else {
                Raccoon.Components.Body body = customBody ?? entity.GetComponent<Raccoon.Components.Body>();
                if (body != null && body.Active && body.Shape != null) {
                    return body.Shape.BoundingBox;
                }
            }

            if (defaultSize.HasValue) {
                return new Rectangle(- (defaultSize.Value / 2f).ToVector2(), defaultSize.Value);
            }

            return new Rectangle(Vector2.Zero, Size.Unit);
        }

        protected virtual void OnZoom(float previousZoom, float newZoom) {
#if DEBUG
            // camera can unlock position bounds by zooming reaaaally out
            if (UseBounds || _isZoomViewingOutOfBounds) {
                if (newZoom <= 1/2f) {
                    if (UseBounds) {
                        UseBounds = false;
                        _isZoomViewingOutOfBounds = true;
                    }
                } else {
                    if (_isZoomViewingOutOfBounds) {
                        UseBounds = true;
                        _isZoomViewingOutOfBounds = false;
                    }
                }
            }
#endif
        }

        #endregion Protected Methods

        #region Private Members

        private void Refresh() {
            Projection = Game.Instance.MainRenderer.RecalculateProjection();
            Game.Instance.InterfaceRenderer.RecalculateProjection();
#if DEBUG
            Game.Instance.DebugRenderer.RecalculateProjection();
#endif

            Vector3 cameraPos = new Vector3(Math.Round(Position + Displacement), 0f),
                    cameraTarget = cameraPos + Vector3.Forward;

            Matrix.CreateLookAt(ref cameraPos, ref cameraTarget, ref _cameraUpVector, out Matrix _view);
            Game.Instance.MainRenderer.View = View = _view * Matrix.CreateScale(Zoom, Zoom, 1f);
        }

        #endregion Private Members
    }
}
