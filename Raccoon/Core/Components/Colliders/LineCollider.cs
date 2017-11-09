using System;

namespace Raccoon.Components {
    public class LineCollider : Collider {
        #region Constructors

        public LineCollider(Vector2 from, Vector2 to) : base() {
            Initialize(from, to);
        }

        public LineCollider(Vector2 from, Vector2 to, params string[] tags) : base(tags) {
            Initialize(from, to);
        }

        public LineCollider(Vector2 from, Vector2 to, params Enum[] tags) : base(tags) {
            Initialize(from, to);
        }

        public LineCollider(Vector2 length) : base() {
            Initialize(Vector2.Zero, length);
        }

        public LineCollider(Vector2 length, params string[] tags) : base(tags) {
            Initialize(Vector2.Zero, length);
        }

        public LineCollider(Vector2 length, params Enum[] tags) : base(tags) {
            Initialize(Vector2.Zero, length);
        }

        #endregion Constructors

        #region Public Properties

        public Vector2 From { get; set; }
        public Vector2 To { get; set; }
        public Vector2 Distance { get { return To - From; } set { To = value + From; } }
        public new Size Size { get { return new Size(Distance); } }
        public Line Equation { get { return new Line(From, To); } }

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender() {
            Graphics.Primitives.Line line = Graphic as Graphics.Primitives.Line;
            Graphic.Origin = Origin * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom;
            line.From = Position + From;
            line.To = Position + To;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Render();
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(Vector2 from, Vector2 to) {
            From = from;
            To = to;

#if DEBUG
            Graphic = new Graphics.Primitives.Line(From * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, To * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom, Color) {
                Surface = Game.Instance.Core.DebugSurface
            };
#endif
        }

        #endregion Private Methods
    }
}
