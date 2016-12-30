using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public class Polygon : IEnumerable {
        private List<Vector2> _vertices = new List<Vector2>();

        public Polygon() { }

        public Polygon(IEnumerable<Vector2> points) {
            AddVertices(points);
        }

        public Polygon(params Vector2[] points) {
            AddVertices(points);
        }

        public int VertexCount { get { return _vertices.Count; } }

        public Vector2 this [int index] {
            get {
                return _vertices[index];
            }

            set {
                _vertices[index] = value;
            }
        }

        public static Polygon RotateAround(Polygon polygon, float degrees, Vector2 origin) {
            List<Vector2> _rotatedVertices = new List<Vector2>(polygon._vertices.Count);
            foreach (Vector2 vertex in polygon._vertices) {
                _rotatedVertices.Add(Util.Math.RotateAround(vertex, origin, degrees));
            }

            return new Polygon(_rotatedVertices);
        }

        public static Polygon Rotate(Polygon polygon, float degrees) {
            return RotateAround(polygon, degrees, polygon[0]);
        }

        public void AddVertex(Vector2 vertex) {
            _vertices.Add(vertex);
        }

        public void AddVertices(IEnumerable<Vector2> vertex) {
            _vertices.AddRange(vertex);
        }

        public void RemoveVertex(Vector2 vertex) {
            _vertices.Remove(vertex);
        }

        public void RemoveVertex(int index) {
            _vertices.RemoveAt(index);
        }

        public void Translate(Vector2 dist) {
            for (int i = 0; i < VertexCount; i++) {
                _vertices[i] += dist;
            }
        }

        public void Translate(float x, float y) {
            Translate(new Vector2(x, y));
        }

        public void RotateAround(float degrees, Vector2 origin) {
            if (degrees == 0) {
                return;
            }

            List<Vector2> _rotatedVertices = new List<Vector2>(_vertices.Count);
            foreach (Vector2 vertex in _vertices) {
                _rotatedVertices.Add(Util.Math.RotateAround(vertex, origin, degrees));
            }

            _vertices = _rotatedVertices;
        }

        public void Rotate(float degrees) {
            RotateAround(degrees, _vertices[0]);
        }

        public float[] Projection(Vector2 axis) {
            return Util.Math.Projection(axis, _vertices);
        }

        public Polygon Clone() {
            Polygon clone = new Polygon();
            clone.AddVertices(_vertices);
            return clone;
        }

        public IEnumerator GetEnumerator() {
            return _vertices.GetEnumerator();
        }

        public override string ToString() {
            string s = "[Polygon | ";
            foreach (Vector2 vertex in _vertices) {
                s +=  vertex + " ";
            }

            return s + "]";
        }
    }
}
