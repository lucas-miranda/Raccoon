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

        public PolygonShape(Polygon polygon) {
            _normalizedPolygon = polygon;
            _normalizedPolygon.Translate(-_normalizedPolygon.Center);
            Recalculate();
        }

        public int Area { get { return (int) (BoundingBox.Width * BoundingBox.Height); } }
        public Size BoundingBox { get; private set; }
        public Rectangle ShapeBoundingBox { get; private set; }
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
            Debug.DrawRectangle(new Rectangle(position - Origin + ShapeBoundingBox.Position, ShapeBoundingBox.Size), Color.Indigo, 0f);

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

        public void Rotate(float degrees) {
            _rotation += degrees;
            Shape.RotateAround(degrees, Shape.Center + Origin);
            BoundingBox = Shape.BoundingBox();
        }

        private void Recalculate() {
            Shape = new Polygon(_normalizedPolygon);
            Shape.RotateAround(Rotation, Shape.Center + Origin);
            BoundingBox = Shape.BoundingBox();

            Rectangle shapeBoundingBox = Rectangle.Empty;
            foreach (Vector2 point in Shape) {
                if (point.X < shapeBoundingBox.Left) {
                    shapeBoundingBox.Left = point.X;
                } else if (point.X > shapeBoundingBox.Right) {
                    shapeBoundingBox.Right = point.X;
                }

                if (point.Y < shapeBoundingBox.Top) {
                    shapeBoundingBox.Top = point.Y;
                } else if (point.Y > shapeBoundingBox.Bottom) {
                    shapeBoundingBox.Bottom = point.Y;
                }
            }

            ShapeBoundingBox = shapeBoundingBox;
        }
    }
}
