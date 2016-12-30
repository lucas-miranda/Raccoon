using System;

namespace Raccoon.Components {
    public class CircleCollider : Collider {
        #region Constructors

        public CircleCollider(int radius) : base() {
            Initialize(radius);
        }

        public CircleCollider(int radius, params string[] tags) : base(tags) {
            Initialize(radius);
        }

        public CircleCollider(int radius, params Enum[] tags) : base(tags) {
            Initialize(radius);
        }

        #endregion Constructors

        #region Public Properties

        public int Radius { get; private set; }
        public Vector2 Center { get { return Position + Radius; } }

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender() {
            Graphic.Position = (Position + Origin) * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Origin = Origin * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom;
            Graphic.Render();
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(int radius) {
            Radius = radius;
            Size = new Size(radius);

#if DEBUG
            Graphic = new Graphics.Primitives.Circle((int) (Radius * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom), Color);
#endif
        }

        #endregion Private Methods
    }
}
