﻿using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon {
    public class CircleShape : IShape {
        private Vector2 _origin;

        public CircleShape(int radius) {
            Radius = radius;
            BoundingBox = new Rectangle(new Vector2(-Radius), new Size(Radius + Radius));
            Area = (int) (Math.PI * Radius * Radius);
        }

        public int Radius { get; }
        public int Area { get; }
        public Rectangle BoundingBox { get; private set; }

        public Vector2 Origin {
            get {
                return _origin;
            }

            set {
                _origin = value;
                BoundingBox = new Rectangle(-Radius - Origin, new Size(Radius + Radius));
            }
        }

        public void DebugRender(Vector2 position, Color color) {
            // boundingBox
            Debug.Draw.PhysicsBodiesLens.Rectangle.AtWorld(
                new Rectangle(position - Radius, BoundingBox.Size),
                false,
                Color.Indigo,
                0f,
                Vector2.One,
                Origin
            );

            Debug.Draw.PhysicsBodiesLens.Circle.AtWorld(
                new Circle(position, Radius), false, 0, Vector2.Zero, color, 0f, 1f, Origin
            );
        }

        public bool ContainsPoint(Vector2 point) {
            throw new System.NotImplementedException();
        }

        public bool Intersects(Line line) {
            throw new System.NotImplementedException();
        }

        public float ComputeMass(float density) {
            //float mass = (float) (Math.PI * Radius * Radius * density);
            //float inertia = mass * Radius * Radius;
            return density;
        }

        public Range Projection(Vector2 position, Vector2 axis) {
            float p = Vector2.Dot(position, axis);
            return new Range(p - Radius, p + Radius);
        }

        public Vector2[] CalculateAxes() {
            return new Vector2[] { Vector2.Right };
        }

        public (Vector2 MaxProjectionVertex, Line Edge) FindBestClippingEdge(Vector2 shapePosition, Vector2 normal) {
            return (Vector2.Zero, new Line(Vector2.Zero, Vector2.Zero));
        }
    }
}
