using Microsoft.Xna.Framework;

namespace Raccoon {
    public class Camera {
        private Vector2 _position;
        private float _zoom = 1f;

        public float X { get { return Position.X; } set { Position = new Vector2(value, Position.Y); } }
        public float Y { get { return Position.Y; } set { Position = new Vector2(Position.X, value); } }
        public Size Size { get { return Game.Instance.ScreenSize; } }
        public float Width { get { return Game.Instance.ScreenWidth; } }
        public float Height { get { return Game.Instance.ScreenHeight; } }
        public float Left { get { return X; } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }
        public Vector2 Center { get { return Position + Game.Instance.ScreenSize / (2 * _zoom); } set { Position = value - Game.Instance.ScreenSize / (2 * _zoom); } }

        public Vector2 Position {
            get {
                return _position;
            }

            set {
                _position = value;
                Game.Instance.Core.ScreenTransform = Transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0) * Matrix.CreateScale(Zoom, Zoom, 1);
#if DEBUG
                Game.Instance.Core.ScreenDebugTransform = Matrix.CreateTranslation(-_position.X * Game.Instance.Scale * _zoom, -_position.Y * Game.Instance.Scale * _zoom, 0);
#endif
            }
        }

        public float Zoom {
            get {
                return _zoom;
            }

            set {
                _zoom = value;
                Game.Instance.Core.ScreenTransform = Transform = Matrix.CreateTranslation(-X, -Y, 0) * Matrix.CreateScale(_zoom, _zoom, 1);
#if DEBUG
                Game.Instance.Core.ScreenDebugTransform = Matrix.CreateTranslation(-X * Game.Instance.Scale * _zoom, -Y * Game.Instance.Scale * _zoom, 0);
#endif
            }
        }

        internal Matrix Transform { get; set; } = Matrix.Identity;
    }
}
