using System;
using Microsoft.Xna.Framework;

namespace Raccoon {
    public class Camera {
        public event Action OnUpdate;

        private Vector2 _position;
        private float _zoom = 1f;

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
        public bool ClampValues { get; set; } = true;
        public Rectangle Bounds { get; set; }
        public Vector2 Center { get { return Position + Game.Instance.ScreenSize / (2 * _zoom); } set { Position = value - Game.Instance.ScreenSize / (2 * _zoom); } }

        public Vector2 Position {
            get {
                return _position;
            }

            set {
                if (ClampValues) {
                    value = new Vector2((float) Math.Round(value.X), (float) Math.Round(value.Y));
                }

                _position = !UseBounds ? value : Util.Math.Clamp(value, Bounds.Position, new Vector2(Bounds.Right - Width, Bounds.Bottom - Height));
                Game.Instance.Core.DefaultSurface.Scale = Game.Instance.Scale * new Vector2(Zoom);
                Game.Instance.Core.DefaultSurface.View = Matrix.CreateTranslation(-X, -Y, 0) * Game.Instance.Core.DefaultSurface.View;

#if DEBUG
                Game.Instance.Core.DebugSurface.View = Matrix.CreateTranslation(-X * Game.Instance.Scale * _zoom, -Y * Game.Instance.Scale * _zoom, 0);
#endif
            }
        }

        public float Zoom {
            get {
                return _zoom;
            }

            set {
                _zoom = value;
                Game.Instance.Core.DefaultSurface.Scale = Game.Instance.Scale * new Vector2(Zoom);
                Game.Instance.Core.DefaultSurface.View = Matrix.CreateTranslation(-X, -Y, 0) * Game.Instance.Core.DefaultSurface.View;

#if DEBUG
                Game.Instance.Core.DebugSurface.View = Matrix.CreateTranslation(-X * Game.Instance.Scale * _zoom, -Y * Game.Instance.Scale * _zoom, 0);
#endif
            }
        }

        public virtual void OnAdded() {
            Current = this;
        }

        public virtual void Update(int delta) {
            OnUpdate?.Invoke();
        }
    }
}
