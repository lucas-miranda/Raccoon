using System;

namespace Raccoon.Components {
    public class CircleCollider : Collider {
        #region Constructors

        public CircleCollider(int radius, params string[] tags) : base(tags) {
            Initialize(radius);
        }

        public CircleCollider(int radius, params Enum[] tags) : base(tags) {
            Initialize(radius);
        }

        #endregion Constructors

        #region Public Properties

        public int Radius { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender() {
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render(Graphic.Surface.Transform(Position - Radius, Game.Instance.MainSurface));
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(int radius) {
            Radius = radius;
            Size = new Size(2 * Radius);

#if DEBUG
            Graphic = new Graphics.Primitives.CircleShape((int) (Radius * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom), Color) {
                Surface = Game.Instance.Core.DebugSurface
            };
#endif
        }

        #endregion Private Methods
    }
}
