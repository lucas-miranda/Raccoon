using System;
using System.Collections.Generic;

namespace Raccoon.Components {
    public class PolygonCollider : Collider {
        #region Construcors

        public PolygonCollider(Polygon polygon) : base() {
            Initialize(polygon);
        }

        public PolygonCollider(Polygon polygon, params string[] tags) : base(tags) {
            Initialize(polygon);
        }

        public PolygonCollider(Polygon polygon, params Enum[] tags) : base(tags) {
            Initialize(polygon);
        }

        public PolygonCollider(IEnumerable<Vector2> points) : base() {
            Initialize(new Polygon(points));
        }

        public PolygonCollider(IEnumerable<Vector2> points, params string[] tags) : base(tags) {
            Initialize(new Polygon(points));
        }

        public PolygonCollider(IEnumerable<Vector2> points, params Enum[] tags) : base(tags) {
            Initialize(new Polygon(points));
        }

        #endregion Construcors

        #region Public Properties

        public Polygon Polygon { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void DebugRender() {
            Graphic.Position = Position + Origin;
            Graphic.Layer = Entity.Layer + 1;
            Graphic.Origin = Origin;
            Graphic.Render();
        }

        #endregion Public Methods

        #region Private Methods

        private void Initialize(Polygon polygon) {
            Polygon = polygon;

#if DEBUG
            Graphic = new Graphics.Primitives.Polygon(Polygon, Color);
#endif
        }

        #endregion Private Methods
    }
}
