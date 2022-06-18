using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class BoxShape : IShape {
        private Polygon _normalizedPolygon;
        private Vector2 _origin;

        public BoxShape(int width, int height) {
            Axes = new Vector2[] {
                Vector2.Right,
                Vector2.Up
            };

            Setup(width, height);
        }

        public BoxShape(int wh) : this(wh, wh) {
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Area { get { return Width * Height; } }
        public Size Size { get { return new Size(Width, Height); } }
        public Rectangle BoundingBox { get; private set; }
        public Vector2[] Axes { get; }
        public Vector2 HalwidthExtents { get; private set; }
        public float[] Extents { get; private set; }
        public Polygon Shape { get; private set; }

            public Vector2 Origin {
                get {
                    return _origin;
                }

                set {
                    _origin = value;
                    Shape = new Polygon(_normalizedPolygon);

                    if (!Math.EqualsEstimate(Rotation, 0f)) {
                        Shape.RotateAround(Rotation, Shape.Center + Origin);
                    }

                    BoundingBox = Shape.BoundingBox();
                }
            }

            public float Rotation {
                get {
                    return Math.Angle(Axes[0]);
                }

                set {
                    Axes[0] = Math.Rotate(Vector2.Right, value);
                    Axes[1] = Math.Rotate(Vector2.Up, value);

                    Shape = new Polygon(_normalizedPolygon);
                    Shape.RotateAround(value, Shape.Center + Origin);
                    BoundingBox = Shape.BoundingBox();
                }
            }

            public void DebugRender(Vector2 position, Color color) {
                // bounding box
                if (!Math.EqualsEstimate(Rotation, 0f)) {
                    Debug.Draw.PhysicsBodiesLens.Rectangle.AtWorld(
                        BoundingBox, false, position, Color.Indigo, 0f, Vector2.One, Origin
                    );
                }

                // draw using Polygon
                Debug.Draw.PhysicsBodiesLens.Polygon.AtWorld(
                    Shape, false, position, color, 0f, Vector2.One, Origin
                );

                // normals
                /*int i = 0;
                foreach (Line edge in polygon.Edges()) {
                    Debug.DrawLine(edge.GetPointNormalized(.5f), edge.GetPointNormalized(.5f) + polygon.Normals[i] * 4f, Color.Yellow);
                    i++;
                }*/

                // centroid
                if (Rotation != 0f) {
                    Debug.Draw.PhysicsBodiesLens.Circle.AtWorld(
                        new Circle(
                            position - Math.RotateAround(Origin, Vector2.Zero, Rotation) + BoundingBox.Center,
                            1
                        ),
                        false, 10, Color.White, 0f, 1f, Vector2.Zero
                    );
                } else {
                    Debug.Draw.PhysicsBodiesLens.Circle.AtWorld(
                        new Circle(position - Origin + BoundingBox.Center, 1),
                        false, 10, Color.White, 0f, 1f, Vector2.Zero
                    );
                }

                Debug.Draw.PhysicsBodiesLens.Circle.AtWorld(
                    new Circle(position - Origin + BoundingBox.Center, 1), false, 10
                );
            }

            public void Setup(int width, int height) {
                if (width == Width && height == Height) {
                    return;
                }

                float rotation;
                if (Axes != null) {
                    rotation = Rotation;
                } else {
                    rotation = 0f;
                }

                Width = width;
                Height = height;
                BoundingBox = new Rectangle(-(Size / 2f).ToVector2(), Size);

                if (Extents == null) {
                Extents = new float[] {
                    width / 2f,
                    height / 2f
                };
            } else {
                Extents[0] = width / 2f;
                Extents[1] = height / 2f;
            }

            HalwidthExtents = new Vector2(Extents[0], Extents[1]);

            _normalizedPolygon = new Polygon(
                Axes[0] * -HalwidthExtents + Axes[1] * HalwidthExtents,
                Axes[0] * HalwidthExtents + Axes[1] * HalwidthExtents,
                Axes[0] * HalwidthExtents + Axes[1] * -HalwidthExtents,
                Axes[0] * -HalwidthExtents + Axes[1] * -HalwidthExtents
            );

            Shape = new Polygon(_normalizedPolygon);

            if (rotation != 0f) {
                Shape.RotateAround(rotation, Shape.Center + Origin);
            }
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
            Shape.Rotate(degrees);
            for (int i = 0; i < Axes.Length; i++) {
                Axes[i] = Math.Rotate(Axes[i], degrees);
            }

            BoundingBox = Shape.BoundingBox();
        }

        public Vector2 ClosestPoint(Vector2 shapePosition, Vector2 point) {
            Vector2 diff = point - shapePosition,
                    closestPoint = shapePosition;

            for (int i = 0; i < Axes.Length; i++) {
                Vector2 axis = Axes[i];
                float extent = Extents[i];
                float dist = Vector2.Dot(diff, axis);
                if (dist > extent) {
                    dist = extent;
                }

                if (dist < -extent) {
                    dist = -extent;
                }

                closestPoint += dist * axis;
            }

            return closestPoint;
        }

        public float DistanceSquared(Vector2 shapePosition, Vector2 point) {
            Vector2 closestPoint = ClosestPoint(shapePosition, point);
            return Vector2.Dot(closestPoint - point, closestPoint - point);
        }
    }
}
