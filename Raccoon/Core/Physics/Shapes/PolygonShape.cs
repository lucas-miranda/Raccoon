using System.Collections.Generic;

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
            Recalculate();
        }

        public PolygonShape(IEnumerable<Vector2> points, bool centralize = true) {
            _normalizedPolygon = new Polygon(points);

            if (centralize) {
                _normalizedPolygon.Translate(-_normalizedPolygon.Center);
            }

            Recalculate();
        }

        public PolygonShape(Polygon polygon) {
            _normalizedPolygon = polygon;
            Recalculate();
        }

        public int Area { get { return (int) (BoundingBox.Width * BoundingBox.Height); } }
        public Rectangle BoundingBox { get; private set; }
        public Polygon Shape { get; private set; }

        public Vector2 Origin {
            get {
                return _origin;
            }

            set {
                _origin = value;
                Recalculate();
            }
        }

        public float Rotation {
            get {
                return _rotation;
            }

            set {
                _rotation = value;
                Recalculate();
            }
        }

        public void DebugRender(Vector2 position, Color color) {
            // bounding box
            Debug.DrawRectangle(BoundingBox + position, Color.Indigo, 0f, Vector2.One, Origin);

            Polygon polygon = new Polygon(Shape);
            polygon.Translate(position - Origin);
            Debug.DrawPolygon(polygon, color);

            // normals
            /*int i = 0;
            foreach (Line edge in polygon.Edges()) {
                Debug.DrawLine(edge.GetPointNormalized(.5f), edge.GetPointNormalized(.5f) + polygon.Normals[i] * 4f, Color.Yellow);
                i++;
            }*/

            // centroid
            Debug.DrawCircle(position - Origin, 1, Color.White, 10);

            /*
            if (!polygon.IsConvex) {
                foreach (Vector2[] component in polygon.ConvexComponents()) {
                    Debug.DrawLines(component, Color.Cyan, Origin - position);
                }
            }
            */
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

        public Range Projection(Vector2 shapePosition, Vector2 axis) {
            Polygon polygon = new Polygon(Shape);
            polygon.Translate(shapePosition);
            return polygon.Projection(axis);
        }

        public Vector2[] CalculateAxes() {
            Vector2[] axes = new Vector2[Shape.Normals.Length];

            int i = 0; //1;

            foreach (Vector2 normal in Shape.Normals) {
                axes[i] = normal;
                i++;
            }

            return axes;
        }

        public (Vector2 MaxProjectionVertex, Line Edge) FindBestClippingEdge(Vector2 shapePosition, Vector2 normal) {
            Polygon polygon = new Polygon(Shape);
            polygon.Translate(shapePosition);
            return SAT.FindBestEdge(polygon, normal);
        }

        public void Rotate(float degrees) {
            _rotation += degrees;
            Shape.RotateAround(degrees, Shape.Center + Origin);
            BoundingBox = Shape.BoundingBox();
        }

        private void Recalculate() {
            Shape = new Polygon(_normalizedPolygon);
            Shape.RotateAround(Rotation, Shape.Center + Origin);
            BoundingBox = Shape.BoundingBox();
        }
    }
}
