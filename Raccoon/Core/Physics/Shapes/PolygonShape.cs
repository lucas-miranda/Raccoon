using Raccoon.Components;
using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class PolygonShape : IShape {
        private Polygon _normalizedPolygon;
        private Vector2 _origin;
        private float _rotation;

        public PolygonShape(params Vector2[] points) {
            _normalizedPolygon = new Polygon(points);
            _normalizedPolygon.Translate(-_normalizedPolygon.Center);
            Shape = new Polygon(_normalizedPolygon);
            BoundingBox = Shape.BoundingBox();
        }

        public PolygonShape(Polygon polygon) {
            _normalizedPolygon = polygon;
            _normalizedPolygon.Translate(-_normalizedPolygon.Center);
            Shape = new Polygon(_normalizedPolygon);
            BoundingBox = Shape.BoundingBox();
        }

        public Body Body { get; set; }
        public int Area { get { return (int) (BoundingBox.Width * BoundingBox.Height); } }
        public Size BoundingBox { get; private set; }
        public Polygon Shape { get; private set; }

        public Vector2 Origin {
            get {
                return _origin;
            }

            set {
                _origin = value;
                Shape = new Polygon(_normalizedPolygon);
                Shape.RotateAround(Rotation, Shape.Center - _origin);
                BoundingBox = Shape.BoundingBox();
            }
        }

        public float Rotation {
            get {
                return _rotation;
            }

            set {
                _rotation = value;
                Shape = new Polygon(_normalizedPolygon);
                Shape.RotateAround(_rotation, Shape.Center - Origin);
                BoundingBox = Shape.BoundingBox();
            }
        }

        internal Polygon Polygon {
            get {
                Polygon polygon = new Polygon(Shape);
                polygon.Translate(Body.Position);
                return polygon;
            }
        }

        public void DebugRender(Vector2 position, Color color) {
            // bounding box
            Debug.DrawRectangle(new Rectangle(position - Origin - BoundingBox / 2f, Debug.Transform(BoundingBox)), Color.Indigo);

            Polygon polygon = new Polygon(Shape);
            polygon.Translate(position - Origin);
            Debug.DrawPolygon(polygon, color);

            // normals
            int i = 0;
            foreach (Line edge in polygon.Edges()) {
                Debug.DrawLine(edge.GetPointNormalized(.5f), edge.GetPointNormalized(.5f) + polygon.Normals[i] * 4f, Color.Yellow);
                i++;
            }

            // centroid
            Debug.DrawCircle(position - Origin, 1, 10, Color.White);
        }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }

        public float ComputeMass(float density) {
            return density;
        }

        public Range Projection(Vector2 axis) {
            return Polygon.Projection(axis);
        }

        public void Rotate(float degrees) {
            _rotation += degrees;
            Shape.RotateAround(degrees, Shape.Center - Origin);
            BoundingBox = Shape.BoundingBox();
        }
    }
}
