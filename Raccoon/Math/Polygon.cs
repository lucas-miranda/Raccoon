using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public class Polygon : IEnumerable {
        private List<Vector2> _vertices = new List<Vector2>();
        private List<Polygon> _convexComponents = new List<Polygon>();
        private bool _needRecalculateComponents = false;

        public Polygon() { }

        public Polygon(IEnumerable<Vector2> points) {
            AddVertices(points);
        }

        public Polygon(params Vector2[] points) {
            AddVertices(points);
        }

        public Polygon(Polygon polygon) {
            _vertices.AddRange(polygon._vertices);

            foreach (Polygon convexComponent in polygon.GetConvexComponents()) {
                _convexComponents.Add(new Polygon(convexComponent._vertices));
            }

            IsConvex = polygon.IsConvex;
            _needRecalculateComponents = polygon._needRecalculateComponents;
        }

        public int VertexCount { get { return _vertices.Count; } }
        public bool IsConvex { get; private set; }

        public Vector2 this [int index] {
            get {
                return _vertices[index];
            }

            set {
                _vertices[index] = value;
                Verify();
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
            Verify();
        }

        public void AddVertices(IEnumerable<Vector2> vertex) {
            _vertices.AddRange(vertex);
            Verify();
        }

        public void RemoveVertex(Vector2 vertex) {
            _vertices.Remove(vertex);
            Verify();
        }

        public void RemoveVertex(int index) {
            _vertices.RemoveAt(index);
            Verify();
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
            _needRecalculateComponents = true;
        }

        public void Rotate(float degrees) {
            RotateAround(degrees, _vertices[0]);
        }

        public void Reflect(Vector2 axis) {
            for (int i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];
                _vertices[i] = new Vector2(2 * axis.X - vertex.X, 2 * axis.Y - vertex.Y);
            }

            _needRecalculateComponents = true;
        }

        public void ReflectHorizontal(float axis) {
            for (int i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];
                _vertices[i] = new Vector2(2 * axis - vertex.X, vertex.Y);
            }

            _needRecalculateComponents = true;
        }

        public void ReflectVertical(float axis) {
            for (int i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];
                _vertices[i] = new Vector2(vertex.X, 2 * axis - vertex.Y);
            }

            _needRecalculateComponents = true;
        }

        public float[] Projection(Vector2 axis) {
            return Util.Math.Projection(axis, _vertices);
        }

        public List<Polygon> GetConvexComponents() {
            if (_needRecalculateComponents) {
                RecalculateComponents();
                _needRecalculateComponents = false;
            }

            return _convexComponents;
        }

        public List<Polygon> Triangulate() {
            // detemine if vertices are in clockwise or anti-clockwise format
            bool clockwise = true;
            int verticesSum = 0;

            Vector2 a, b;
            for (int i = 0; i < VertexCount; i++) {
                a = _vertices[i];
                b = _vertices[(i + 1) % VertexCount];
                verticesSum += (int) ((b.X - a.X) * (b.Y + a.Y));
            }

            clockwise = verticesSum < 0;

            // triangulate using ear clipping
            List<Polygon> triangles = new List<Polygon>();
            Stack<LinkedListNode<Vector2>> ears = new Stack<LinkedListNode<Vector2>>();

            LinkedList<Vector2> vertices = new LinkedList<Vector2>(_vertices);
            LinkedListNode<Vector2> current = vertices.First;
            while (current != null) {
                LinkedListNode<Vector2> previous = current.Previous == null ? vertices.Last : current.Previous,
                                        next = current.Next == null ? vertices.First : current.Next;

                // is current vertex an ear?
                if (IsEar(previous.Value, current.Value, next.Value, clockwise)) {
                    ears.Push(current);
                }

                current = current.Next;
            }

            while (true) {
                current = vertices.Count == 3 ? vertices.First : ears.Pop();

                LinkedListNode<Vector2> previous = current.Previous == null ? vertices.Last : current.Previous,
                                        next = current.Next == null ? vertices.First : current.Next;

                triangles.Add(new Polygon(previous.Value, current.Value, next.Value));

                if (vertices.Count == 3) {
                    break;
                }

                vertices.Remove(current);

                if (previous != next) {
                    // is previous vertex an ear?
                    if (IsEar((previous.Previous == null ? vertices.Last : previous.Previous).Value, previous.Value, next.Value, clockwise)) {
                        ears.Push(previous);
                    }

                    // is next vertex an ear?
                    if (IsEar(previous.Value, next.Value, (next.Next == null ? vertices.First : next.Next).Value, clockwise)) {
                        ears.Push(next);
                    }
                }
            }

            return triangles;
        }

        public Polygon Clone() {
            return new Polygon(this);
        }

        public IEnumerator GetEnumerator() {
            return _vertices.GetEnumerator();
        }

        public override string ToString() {
            string s = "[";
            foreach (Vector2 vertex in _vertices) {
                s += vertex + " ";
            }

            return s.Remove(s.Length - 1) + "]";
        }

        private void Verify() {
            if (VertexCount < 3) {
                return;
            }

            IsConvex = true;

            // algorithm to determine if it's convex: http://stackoverflow.com/a/1881201
            Vector2 previous = _vertices[_vertices.Count - 1], center = _vertices[0], next = _vertices[1];
            int sign = System.Math.Sign((center.X - previous.X) * (next.Y - center.Y) - (center.Y - previous.Y) * (next.X - center.X));

            previous = center;
            for (int i = 1; i < VertexCount; i++) {
                center = _vertices[i];
                next = _vertices[(i + 1) % _vertices.Count];
                if (sign != System.Math.Sign((center.X - previous.X) * (next.Y - center.Y) - (center.Y - previous.Y) * (next.X - center.X))) {
                    IsConvex = false;
                    break;
                }

                previous = center;
            }

            _needRecalculateComponents = true;
        }

        private void RecalculateComponents() {
            _convexComponents.Clear();
            if (IsConvex) {
                return;
            }

            List<Polygon> triangles = Triangulate();

            Polygon currentComponent;
            List<int> trianglesUsed = new List<int>();
            int verticesConnecting = 0, cadidateVertexId = -1;
            while (triangles.Count > 0) {
                currentComponent = new Polygon(triangles[0]);
                trianglesUsed.Add(0);

                for (int i = 1; i < triangles.Count; i++) {
                    Polygon triangle = triangles[i];
                    verticesConnecting = 0;

                    for (int j = 0; j < triangle.VertexCount; j++) {
                        for (int k = 0; k < currentComponent.VertexCount; k++) {
                            if (currentComponent[k] == triangle[j]) {
                                verticesConnecting++;
                            } else {
                                cadidateVertexId = j;
                            }
                        }
                    }

                    if (verticesConnecting == 2) {
                        Vector2 candidateVertex = triangle[cadidateVertexId];
                        currentComponent.AddVertex(candidateVertex);

                        if (currentComponent.IsConvex) {
                            trianglesUsed.Add(i);
                        } else {
                            currentComponent.RemoveVertex(candidateVertex);
                        }
                    }
                }

                for (int j = trianglesUsed.Count - 1; j >= 0; j--) {
                    triangles.RemoveAt(trianglesUsed[j]);
                }

                trianglesUsed.Clear();

                _convexComponents.Add(currentComponent);
            }
        }

        private bool IsEar(Vector2 previous, Vector2 center, Vector2 next, bool clockwise) {
            float x1 = next.X - center.X, y1 = next.Y - center.Y,
                  x2 = previous.X - center.X, y2 = previous.Y - center.Y;

            bool crossSign = (x1 * y2 > x2 * y1) == clockwise;
            float angle = System.Math.Abs(Util.Math.Angle(previous, center, next));

            if (!crossSign || angle == 180) {
                return false;
            }

            // test if some other points lies inside (if so triangle isn't an ear)
            foreach (Vector2 point in _vertices) {
                if (point == previous || point == center || point == next) {
                    continue;
                }

                if (Util.Math.IsPointInsideTriangle(previous, center, next, point)) {
                    return false;
                }
            }

            return true;
        }
    }
}
