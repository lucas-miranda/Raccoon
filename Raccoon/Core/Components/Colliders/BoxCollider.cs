using Raccoon.Graphics;
using System;

namespace Raccoon.Components {
    public class BoxCollider : Collider {
        public BoxCollider(float width, float height, string tag) : base(tag) {
            Size = new Size(width, height);

#if DEBUG
            Graphic = new Graphics.Primitives.Rectangle(Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, Color, false);
#endif
        }

        public BoxCollider(float width, float height, Enum tag) : this(width, height, tag.ToString()) { }

        public override void DebugRender() {
            Graphic.Position = Position * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom;
            (Graphic as Graphics.Primitives.Rectangle).Size = new Size((float) Math.Floor(Width * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom), (float) Math.Floor(Height * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom));
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render();
        }
    }
}
