using Raccoon.Graphics;
using System;

namespace Raccoon.Components {
    public class BoxCollider : Collider {
        public BoxCollider(float width, float height, string tag) : base(tag) {
            Size = new Size(width, height);

#if DEBUG
            Graphic = new Graphics.Primitives.Rectangle(Width * Game.Instance.Scale, Height * Game.Instance.Scale, Color, false);
#endif
        }

        public BoxCollider(float width, float height, Enum tag) : this(width, height, tag.ToString()) { }

        public override void DebugRender() {
            Graphic.Position = Position * Game.Instance.Scale;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render();
        }
    }
}
