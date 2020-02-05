using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Raccoon.Util;

namespace Raccoon {
    public class Polygon : IEnumerable {
        #region Private Members

        private List<Vector2> _vertices;
        private List<Vector2[]> _convexComponents = new List<Vector2[]>();
        private bool _needRecalculateComponents;
        private Vector2 _anchorPosWhenConvexComponentsWereCalculated;

        #endregion Private Members

        #region Constructors

        public Polygon(IEnumerable<Vector2> points) {
            _vertices = new List<Vector2>(points);
            IsConvex = false;
            Normals = new Vector2[0];
            Center = Vector2.Zero;

            Verify();
        }

        public Polygon(params Vector2[] points) : this((IEnumerable<Vector2>) points) { }

        public Polygon(Polygon polygon) {
            _vertices = new List<Vector2>(polygon._vertices);

            foreach (Vector2[] component in polygon.ConvexComponents()) {
                _convexComponents.Add(component);
            }

            IsConvex = polygon.IsConvex;

            Normals = new Vector2[polygon.Normals.Length];
            polygon.Normals.CopyTo(Normals, 0);

            Center = polygon.Center;

            _needRecalculateComponents = polygon._needRecalculateComponents;
        }

        public Polygon(Triangle triangle) : this(triangle.A, triangle.B, triangle.C) {
        }

        public Polygon(Rectangle rectangle) : this(rectangle.TopLeft, rectangle.TopRight, rectangle.BottomRight, rectangle.BottomLeft) {
        }

        #endregion Constructors

        #region Public Properties

        public int VertexCount { get { return _vertices.Count; } }
        public bool IsConvex { get; private set; }
        public Vector2[] Normals { get; private set; }
        public Vector2 Center { get; private set; }
        public ReadOnlyCollection<Vector2> Vertices { get { return _vertices.AsReadOnly(); } }

        public Vector2 this [int index] {
            get {
                return _vertices[index];
            }

            set {
                _vertices[index] = value;
                Verify();
            }
        }

        #endregion Public Properties

        #region Public Static Methods

        public static Polygon RotateAround(Polygon polygon, float degrees, Vector2 origin) {
            List<Vector2> _rotatedVertices = new List<Vector2>(polygon.VertexCount);
            foreach (Vector2 vertex in polygon._vertices) {
                _rotatedVertices.Add(Math.RotateAround(vertex, origin, degrees));
            }

            return new Polygon(_rotatedVertices);
        }

        public static Polygon Rotate(Polygon polygon, float degrees) {
            return RotateAround(polygon, degrees, polygon.Center);
        }

        public static int Orientation(IList<Vector2> vertices) {
            int rMin = 0;
            Vector2 vertexMin = vertices[0];

            for (int i = 1; i < vertices.Count; i++) {
                Vector2 vertex = vertices[i];
                if (vertex.Y > vertexMin.Y) {
                    continue;
                }

                if (vertex.Y == vertexMin.Y && vertex.X > vertexMin.X) {
                    continue;
                }

                rMin = i;
                vertexMin = vertex;
            }

            return Triangle.Orientation(Helper.At(vertices, rMin - 1), Helper.At(vertices, rMin), Helper.At(vertices, rMin + 1));
        }

        public static bool IsClockwise(IList<Vector2> vertices) {
            return Orientation(vertices) > 0;
        }

        public static bool IsCounterClockwise(IList<Vector2> vertices) {
            return Orientation(vertices) < 0;
        }

        public static bool IsDegenerate(IList<Vector2> vertices) {
            return Orientation(vertices) == 0;
        }

        public static List<Vector2> ConvexHull(ICollection<Vector2> points) {
            // using jarvis (gift wrapping) algorithm
            List<Vector2> convexHull = new List<Vector2>();

            if (points.Count < 3) {
                convexHull.AddRange(points);
                return convexHull;
            }

            Vector2? firstPoint = null,
                     leftmostPoint = null;

            // find leftmostPoint
            foreach (Vector2 point in points) {
                if (firstPoint == null) {
                    firstPoint = leftmostPoint = point;
                }

                if (point.X < leftmostPoint.Value.X) {
                    leftmostPoint = point;
                }
            }

            Vector2 endPoint, pointOnHull = leftmostPoint.Value;
            do {
                convexHull.Add(pointOnHull);
                endPoint = firstPoint.Value;
                foreach (Vector2 point in points) {
                    if (endPoint == pointOnHull || Math.IsLeft(pointOnHull, endPoint, point)) {
                        endPoint = point;
                    }
                }

                pointOnHull = endPoint;
            } while (endPoint != convexHull[0]);

            return convexHull;
        }

        public static Polygon Merge(Polygon polygonA, Polygon polygonB) {
            List<Vector2> vertices = new List<Vector2>(polygonA.Vertices);
            vertices.AddRange(polygonB.Vertices);
            List<Vector2> convexHullVertices = ConvexHull(vertices);
            return new Polygon(convexHullVertices);
        }

        public static Line? FindSharedEdge(Vector2[] polygonA, Vector2[] polygonB) {
            Line? line = null;
            Vector2 edgeAPointA = polygonA[0],
                    edgeBPointA = polygonB[0];

            for (int i = 1; i < polygonA.Length; i++) {
                Vector2 edgeAPointB = polygonA[i];
                for (int j = 1; j < polygonB.Length; j++) {
                    Vector2 edgeBPointB = polygonB[i];
                    if (Helper.EqualsPermutation(edgeAPointA, edgeAPointB, edgeBPointA, edgeBPointB)) {
                        line = new Line(edgeAPointA, edgeAPointB);
                        break;
                    }
                }

                if (line != null) {
                    break;
                }
            }

            return line;
        }

        public static Line? FindSharedEdge(Polygon polygonA, Polygon polygonB) {
            return FindSharedEdge(polygonA._vertices.ToArray(), polygonB._vertices.ToArray());
        }

        #endregion Public Static Methods

        #region Public Methods

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

            CalculateCentroid();
        }

        public void Translate(float x, float y) {
            Translate(new Vector2(x, y));
        }

        public void Scale(Vector2 factor, Vector2 origin) {
            for (int i = 0; i < VertexCount; i++) {
                _vertices[i] = (_vertices[i] - origin) * factor + origin;
            }

            CalculateCentroid();
        }

        public void Scale(float factor, Vector2 origin) {
            Scale(new Vector2(factor), origin);
        }

        public void Scale(Vector2 factor) {
            Scale(factor, Center);
        }

        public void Scale(float factor) {
            Scale(new Vector2(factor), Center);
        }

        public void RotateAround(float degrees, Vector2 origin) {
            if (degrees == 0) {
                return;
            }

            for (int i = 0; i < VertexCount; i++) {
                _vertices[i] = Math.RotateAround(_vertices[i], origin, degrees);
            }

            CalculateCentroid();
            CalculateNormals();
            _needRecalculateComponents = true;
        }

        public void Rotate(float degrees) {
            RotateAround(degrees, Center);
        }

        public void Reflect(Vector2 axis) {
            for (int i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];
                _vertices[i] = new Vector2(2 * axis.X - vertex.X, 2 * axis.Y - vertex.Y);
            }

            CalculateCentroid();
            CalculateNormals();
            _needRecalculateComponents = true;
        }

        public void ReflectHorizontal(float axis) {
            for (int i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];
                _vertices[i] = new Vector2(2 * axis - vertex.X, vertex.Y);
            }

            CalculateCentroid();
            CalculateNormals();
            _needRecalculateComponents = true;
        }

        public void ReflectVertical(float axis) {
            for (int i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];
                _vertices[i] = new Vector2(vertex.X, 2 * axis - vertex.Y);
            }

            CalculateCentroid();
            CalculateNormals();
            _needRecalculateComponents = true;
        }

        public Range Projection(Vector2 axis) {
            return axis.Projection(_vertices);
        }

        public Rectangle BoundingBox() {
            if (VertexCount <= 1) {
                return Rectangle.Empty;
            }

            Vector2 firstVertex = _vertices[0];

            float top = firstVertex.Y,
                  left = firstVertex.X,
                  bottom = firstVertex.Y,
                  right = firstVertex.X;

            for (int i = 1; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i];

                if (vertex.X < left) {
                    left = vertex.X;
                } else if (vertex.X > right) {
                    right = vertex.X;
                }

                if (vertex.Y < top) {
                    top = vertex.Y;
                } else if (vertex.Y > bottom) {
                    bottom = vertex.Y;
                }
            }

            return new Rectangle(new Vector2(left, top), new Vector2(right, bottom));
        }

        public Vector2 ClosestPoint(Vector2 point) {
            if (IsInside(point)) {
                return point;
            }

            Vector2 closestPoint = Center;
            float closestSquaredDist = float.PositiveInfinity;
            foreach (Line edge in Edges()) {
                Vector2 p = edge.ClosestPoint(point);
                float squaredDist = Math.DistanceSquared(p, point);
                if (squaredDist < closestSquaredDist) {
                    closestPoint = p;
                    closestSquaredDist = squaredDist;
                }
            }

            return closestPoint;
        }

        public float Area() {
            if (VertexCount < 3) {
                return 0;
            }

            float area = 0;
            for (int i = 1, j = 2, k = 0; i < VertexCount; i++, j++, k++) {
                area += Helper.At(_vertices, i).X * (Helper.At(_vertices, j).Y - Helper.At(_vertices, k).Y);
            }

            area += _vertices[0].X * (_vertices[1].Y - _vertices[VertexCount - 1].Y);
            return area / 2.0f;
        }

        public float DistanceSquared(Vector2 point) {
            Vector2 closestPoint = ClosestPoint(point);
            return Vector2.Dot(closestPoint - point, closestPoint - point);
        }

        public bool IsInside(Vector2 point) {
            // ref: http://geomalgorithms.com/a03-_inclusion.html
            int windNumber = 0;
            foreach (Line edge in Edges()) {
                if (edge.PointA.Y <= point.Y) {
                    if (edge.PointB.Y > point.Y && isLeft(edge, point) > 0) {
                        ++windNumber;
                    }
                } else {
                    if (edge.PointB.Y <= point.Y && isLeft(edge, point) < 0) {
                        --windNumber;
                    }
                }
            }

            return windNumber > 0;

            int isLeft(Line line, Vector2 p) {
                return (int) System.Math.Round((line.PointB.X - line.PointA.X) * (p.Y - line.PointA.Y) - (p.X - line.PointA.X) * (line.PointB.Y - line.PointA.Y));
            }
        }

        public Vector2[] Intersects(Line segment) {
            List<Vector2> intersections = new List<Vector2>();

            foreach (Line edge in Edges()) {
                if (edge.IntersectionPoint(segment, out Vector2 intersectionPoint)) {
                    intersections.Add(intersectionPoint);
                }
            }

            return intersections.ToArray();
        }

        public List<Vector2[]> ConvexComponents() {
            if (_needRecalculateComponents) {
                _needRecalculateComponents = false;
                CalculateComponents();
            }

            return _convexComponents;
        }

        public List<Triangle> Triangulate() {
            // force counterclockwise (as it's a precondition to ear clipping algorithm)
            LinkedList<Vector2> vertices = new LinkedList<Vector2>();
            if (IsCounterClockwise(_vertices)) {
                foreach (Vector2 vertex in _vertices) {
                    vertices.AddLast(vertex);
                }
            } else {
                foreach (Vector2 vertex in _vertices) {
                    vertices.AddFirst(vertex);
                }
            }

            // triangulate using ear clipping
            List<Triangle> triangles = new List<Triangle>();
            Stack<LinkedListNode<Vector2>> ears = new Stack<LinkedListNode<Vector2>>();

            LinkedListNode<Vector2> current = vertices.First;

            while (current != null) {
                if (IsInternalDiagonal(current.PreviousOrLast(), current.NextOrFirst())) {
                    ears.Push(current);
                }

                current = current.Next;
            }

            LinkedListNode<Vector2> prev2, prev, next, next2;

            while (vertices.Count > 3) {
                Debug.Assert(ears.Count > 0, "Error in Triangulate: No ear found.");

                current = ears.Pop();
                prev = current.PreviousOrLast();
                prev2 = prev.PreviousOrLast();
                next = current.NextOrFirst();
                next2 = next.NextOrFirst();

                triangles.Add(new Triangle(current.Value, prev.Value, next.Value));

                if (IsInternalDiagonal(prev2, next)) {
                    ears.Push(prev);
                }

                if (IsInternalDiagonal(prev, next2)) {
                    ears.Push(next);
                }

                vertices.Remove(current);
            }

            current = vertices.First;
            triangles.Add(new Triangle(current.PreviousOrLast().Value, current.Value, current.NextOrFirst().Value));

            return triangles;
        }

        public void Merge(Triangle triangle) {
            Vector2[] triangleVertices = triangle.Vertices;
            Vector2? vertexToMerge = null;
            int i;
            for (i = 0; i < VertexCount; i++) {
                Vector2 vertex = _vertices[i],
                        nextVertex = Helper.At(_vertices, i + 1);

                for (int j = 0; j < triangleVertices.Length; j++) {
                    if (vertex == triangleVertices[j]) {
                        if (nextVertex == Helper.At(triangleVertices, j + 1)) {
                            vertexToMerge = Helper.At(triangleVertices, j + 2);
                            break;
                        } else if (nextVertex == Helper.At(triangleVertices, j + 2)) {
                            vertexToMerge = Helper.At(triangleVertices, j + 1);
                            break;
                        }
                    }
                }

                if (vertexToMerge != null) {
                    break;
                }
            }

            if (vertexToMerge != null) {
                _vertices.Insert(Helper.Index(i + 1, VertexCount), triangle.C);
                Verify();
            }
        }

        public IEnumerable<Line> Edges() {
            for (int i = 0; i < VertexCount; i++) {
                yield return new Line(_vertices[i], _vertices[(i + 1) % VertexCount]);
            }
        }

        public IEnumerator GetEnumerator() {
            return _vertices.GetEnumerator();
        }

        public override string ToString() {
            return $"[{string.Join(", ", _vertices)}]";
        }

        #endregion Public Methods

        #region Private Methods

        private void Verify() {
            if (VertexCount < 3) {
                throw new System.InvalidOperationException("Polygon must have at least 3 vertices.");
            }

            CalculateCentroid();
            CalculateConvexity();
            CalculateNormals();
            _needRecalculateComponents = true;
        }

        private void CalculateCentroid() {
            Vector2 pointsSum = Vector2.Zero;
            foreach (Vector2 vertex in _vertices) {
                pointsSum += vertex;
            }

            Center = pointsSum / VertexCount;
        }

        private void CalculateConvexity() {
            // convexity test
            IsConvex = true;

            // algorithm to determine if it's convex: http://stackoverflow.com/a/1881201
            Vector2 previous = _vertices[_vertices.Count - 1], center = _vertices[0], next = _vertices[1];
            int sign = System.Math.Sign(Triangle.SignedArea2(previous, center, next));

            previous = center;
            for (int i = 1; i < VertexCount; i++) {
                center = _vertices[i];
                next = _vertices[(i + 1) % _vertices.Count];
                if (sign != System.Math.Sign(Triangle.SignedArea2(previous, center, next))) {
                    IsConvex = false;
                    break;
                }

                previous = center;
            }
        }

        private void CalculateNormals() {
            if (Vertices.Count < 3) {
                if (Normals.Length > 0) {
                    Normals = new Vector2[0];
                }

                return;
            }

            if (Normals.Length != VertexCount) {
                Normals = new Vector2[VertexCount];
            }

            int i = 0;
            if (Math.IsLeft(Vertices[0], Vertices[1], Vertices[2])) {
                foreach (Line line in Edges()) {
                    Normals[i] = line.ToVector2().PerpendicularCCW().Normalized();
                    i++;
                }
            } else {
                foreach (Line line in Edges()) {
                    Normals[i] = line.ToVector2().PerpendicularCW().Normalized();
                    i++;
                }
            }
        }

        private void CalculateComponents() {
            _convexComponents.Clear();
            CalculateConvexity();

            Vector2 anchorVertex;
            if (IsConvex) {
                Vector2[] singleComponent = new Vector2[_vertices.Count];
                anchorVertex = _vertices[0];

                for (int i = 0; i < _vertices.Count; i++) {
                    singleComponent[i] = _vertices[i] - anchorVertex;
                }

                _convexComponents.Add(singleComponent);
                return;
            }

            List<(int, int)> exclusionList = new List<(int, int)>();
            List<Vector2[]> components = new List<Vector2[]>();

            anchorVertex = _vertices[0];
            Triangulate().ForEach(
                (Triangle t) => {
                    Vector2[] triangleVertices = t.Vertices;

                    for (int i = 0; i < triangleVertices.Length; i++) {
                        triangleVertices[i] -= anchorVertex;
                    }

                    components.Add(triangleVertices);
                }
            );

            Vector2 sharedVertexA = Vector2.Zero, sharedVertexB = Vector2.Zero;
            List<Vector2> verticesToConvexHull = new List<Vector2>();

            int mergeTries = 0;
            while (components.Count >= 2 && mergeTries < components.Count - 1) {
                (int poly1Index, int poly2Index) = FindGreatestSharedEdgePolygons(out Vector2[] component1, out Vector2[] component2);

                // just ignore if no valid index has been found
                if (poly1Index == -1 || poly2Index == -1) {
                    mergeTries++;
                    continue;
                }

                verticesToConvexHull.Clear();
                verticesToConvexHull.AddRange(component1);
                verticesToConvexHull.AddRange(component2);
                List<Vector2> componentConvexHull = ConvexHull(verticesToConvexHull);
                int indexSharedVertexA = componentConvexHull.IndexOf(sharedVertexA),
                    indexSharedVertexB = componentConvexHull.IndexOf(sharedVertexB);

                // test if it's a valid convex hull
                if (indexSharedVertexA != -1 && indexSharedVertexB != -1 && Math.IsLeft(Helper.At(componentConvexHull, indexSharedVertexA - 1), Helper.At(componentConvexHull, indexSharedVertexA + 1), Helper.At(componentConvexHull, indexSharedVertexA)) && Math.IsLeft(Helper.At(componentConvexHull, indexSharedVertexB - 1), Helper.At(componentConvexHull, indexSharedVertexB + 1), Helper.At(componentConvexHull, indexSharedVertexB))) {
                    if (poly1Index > poly2Index) {
                        components.RemoveAt(poly1Index);
                        components.RemoveAt(poly2Index);
                    } else {
                        components.RemoveAt(poly2Index);
                        components.RemoveAt(poly1Index);
                    }

                    components.Add(componentConvexHull.ToArray());
                    mergeTries = 0;
                    exclusionList.Clear();
                } else {
                    mergeTries++;
                    exclusionList.Add((poly1Index, poly2Index));
                }
            }

            _convexComponents.AddRange(components);

            return;

            (int p1Index, int p2Index) FindGreatestSharedEdgePolygons(out Vector2[] component1, out Vector2[] component2) {
                (int, int) indexes = (-1, -1);
                component1 = component2 = null;
                float greatestEdgeLength = 0;

                for (int a = 0; a < components.Count; a++) {
                    Vector2[] componentA = components[a];
                    for (int b = a + 1; b < components.Count; b++) {
                        Vector2[] componentB = components[b];

                        if (exclusionList.FindIndex(p => Helper.EqualsPermutation(p.Item1, p.Item2, a, b)) != -1) {
                            continue;
                        }

                        Line? sharedEdge = FindSharedEdge(componentA, componentB);
                        if (sharedEdge != null) {
                            float d = sharedEdge.Value.LengthSquared;
                            if (d <= greatestEdgeLength) {
                                continue;
                            }

                            indexes = (a, b);
                            greatestEdgeLength = d;
                            component1 = componentA;
                            component2 = componentB;
                            sharedVertexA = sharedEdge.Value.PointA;
                            sharedVertexB = sharedEdge.Value.PointB;
                        }
                    }
                }

                return indexes;
            }
        }

        #region Triangulate Helper

        private bool InCone(LinkedListNode<Vector2> A, LinkedListNode<Vector2> B) {

            Vector2 prevA = A.PreviousOrLast().Value,
                    a = A.Value,
                    nextA = A.NextOrFirst().Value,
                    b = B.Value;

            // if A is a convex vertex
            if (Math.IsLeftOn(a, nextA, prevA)) {
                return Math.IsLeft(a, b, prevA) && Math.IsLeft(b, a, nextA);
            }

            // else A is reflex
            return Math.IsLeftOn(a, b, nextA) || Math.IsLeftOn(b, a, prevA);
        }

        private bool IsDiagonalie(LinkedListNode<Vector2> A, LinkedListNode<Vector2> B) {
            Vector2 a = A.Value, b = B.Value;
            for (int i = 0; i < VertexCount; i++) {
                Vector2 c = Helper.At(_vertices, i),
                        c1 = Helper.At(_vertices, i + 1);

                if (c != a && c1 != a && c != b && c1 != b && Intersect(a, b, c, c1)) {
                    return false;
                }
            }

            return true;
        }

        private bool IsInternalDiagonal(LinkedListNode<Vector2> A, LinkedListNode<Vector2> B) {
            return InCone(A, B) && InCone(B, A) && IsDiagonalie(A, B);
        }

        private bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            if (IntersectProp(a, b, c, d)) {
                return true;
            }

            if (IsBetween(a, b, c) || IsBetween(a, b, d) || IsBetween(c, d, a) || IsBetween(c, d, b)) {
                return true;
            }

            return false;
        }

        private bool IsBetween(Vector2 a, Vector2 b, Vector2 c) {
            if (!Math.IsCollinear(a, b, c)) {
                return false;
            }

            if (a.X != b.X) {
                return (a.X <= c.X && c.X <= b.X) || (a.X >= c.X && c.X >= b.X);
            }

            return (a.Y <= c.Y && c.Y <= b.Y) || (a.Y >= c.Y && c.Y >= b.Y);
        }

        private bool IntersectProp(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            if (Math.IsCollinear(a, b, c) || Math.IsCollinear(a, b, d) || Math.IsCollinear(c, d, a) || Math.IsCollinear(c, d, b)) {
                return false;
            }

            return (!Math.IsLeft(a, b, c) ^ !Math.IsLeft(a, b, d)) && (!Math.IsLeft(c, d, a) ^ !Math.IsLeft(c, d, b));
        }

        #endregion Triangulate Helper

        #endregion Private Methods
    }
}
