using Raccoon.Graphics;
using System;

namespace Raccoon.Components {
    public class BoxCollider : ColliderComponent {
        public BoxCollider(float width, float height, string tag) : base(ColliderType.Box, tag) {
            Size = new Size(width, height);
        }

        public BoxCollider(float width, float height, Enum tag) : this(width, height, tag.ToString()) { }
        
        public override void DebugRender() {
            if (Graphic == null) {
                Graphic = new Graphics.Primitives.Rectangle(Width * Game.Instance.Scale, Height * Game.Instance.Scale, Color, false);
            }

            Graphic.Position = Position * Game.Instance.Scale;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render();
        }
    }
}
