using System;

using Raccoon.Graphics;

namespace Raccoon.Components {
    public class BoxCollider : Collider {
        #region Constructors

        public BoxCollider(float width, float height, params string[] tags) : base(tags) {
            Initialize(width, height);
        }

        public BoxCollider(float width, float height, params Enum[] tags) : base(tags) {
            Initialize(width, height);
        }

        #endregion Constructors

        #region Public Properties

        public float Rotation { get; set; }

        public Polygon Polygon {
            get {
                Polygon shape = new Polygon(new Vector2(0, 0), new Vector2(Width, 0), new Vector2(Width, Height), new Vector2(0, Height));
                shape.RotateAround(Rotation, Origin);
                return shape;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender() {
            Graphic.Position = (Position + Origin) * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom;
            (Graphic as Graphics.Primitives.Rectangle).Size = new Size((float) Math.Floor(Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom), (float) Math.Floor(Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom));
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Origin = Origin * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom;
            Graphic.Rotation = Rotation;
            Graphic.Render();
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(float width, float height) {
            Size = new Size(width, height);

#if DEBUG
            Graphic = new Graphics.Primitives.Rectangle(Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, Color, false);
#endif
        }

        #endregion Private Methods
    }
}
