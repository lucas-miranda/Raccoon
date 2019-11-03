using System.Collections.Generic;
using Raccoon.Util;

namespace Raccoon {
    public sealed partial class Physics {
        private bool TestSAT(Polygon A, Polygon B, IEnumerable<Vector2> axes, out Contact? contact) {
            Contact leastPenetrationContact = new Contact {
                Position = A[0],
                PenetrationDepth = float.PositiveInfinity
            };

            foreach (Vector2 axis in axes) {
                Range projectionA = A.Projection(axis), 
                      projectionB = B.Projection(axis);

                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) {
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            // contact points
            (Vector2 MaxProjVertex, Line Edge) edgeA = FindBestEdge(A, leastPenetrationContact.Normal);
            (Vector2 MaxProjVertex, Line Edge) edgeB = FindBestEdge(B, -leastPenetrationContact.Normal);

            Vector2[] contactPoints = CalculateContactPoints(edgeA, edgeB, leastPenetrationContact.Normal);

            if (contactPoints.Length > 0) {
                leastPenetrationContact.Position = contactPoints[0];
            }

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(Polygon A, Polygon B, out Contact? contact) {
            Vector2[] axes = new Vector2[A.Normals.Length + B.Normals.Length/* + 1*/];

            /*
            axes[0] = (B.Center - A.Center).Normalized();
            int i = 1;
            */

            int i = 0;

            // polygon A axes
            foreach (Vector2 normal in A.Normals) {
                axes[i] = normal;
                i++;
            }

            // polygon B axes
            foreach (Vector2 normal in B.Normals) {
                axes[i] = normal;
                i++;
            }

            return TestSAT(A, B, axes, out contact);
        }

        private bool TestSAT(IShape shapeA, Vector2 posA, IShape shapeB, Vector2 posB, ICollection<Vector2> axes, out Contact? contact) {
            Contact leastPenetrationContact = new Contact {
                Position = posA,
                PenetrationDepth = float.PositiveInfinity
            };

            /*
            Vector2[] a = new Vector2[axes.Count + 1];
            a[0] = (posB - posA).Normalized();
            axes.CopyTo(a, 1);
            */

            foreach (Vector2 axis in axes) {
                Range projectionA = shapeA.Projection(posA, axis), 
                      projectionB = shapeB.Projection(posB, axis);

                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) { 
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            // contact points
            (Vector2 MaxProjVertex, Line Edge) edgeA = shapeA.FindBestClippingEdge(posA, leastPenetrationContact.Normal);
            (Vector2 MaxProjVertex, Line Edge) edgeB = shapeB.FindBestClippingEdge(posB, -leastPenetrationContact.Normal);

            if (!(edgeA.Edge.PointA == edgeA.Edge.PointB || edgeB.Edge.PointA == edgeA.Edge.PointB)) {
                Vector2[] contactPoints = CalculateContactPoints(edgeA, edgeB, leastPenetrationContact.Normal);

                if (contactPoints.Length > 0) {
                    leastPenetrationContact.Position = contactPoints[0];
                }
            }

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(IShape shape, Vector2 shapePos, Polygon polygon, ICollection<Vector2> axes, out Contact? contact) {
            Contact leastPenetrationContact = new Contact {
                Position = shapePos,
                PenetrationDepth = float.PositiveInfinity
            };

            Vector2[] a = new Vector2[polygon.Normals.Length + axes.Count/* + 1*/];
            /*
            a[0] = (shapePos - polygon.Center).Normalized();
            */
            polygon.Normals.CopyTo(a, 0); // 1
            axes.CopyTo(a, polygon.Normals.Length); // + 1

            foreach (Vector2 axis in a) {
                Range projectionA = shape.Projection(shapePos, axis), projectionB = polygon.Projection(axis);
                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) {
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            // contact points
            (Vector2 MaxProjVertex, Line Edge) edgeA = shape.FindBestClippingEdge(shapePos, leastPenetrationContact.Normal);
            (Vector2 MaxProjVertex, Line Edge) edgeB = FindBestEdge(polygon, -leastPenetrationContact.Normal);

            if (edgeA.Edge.PointA != edgeA.Edge.PointB) {
                Vector2[] contactPoints = CalculateContactPoints(edgeA, edgeB, leastPenetrationContact.Normal);

                if (contactPoints.Length > 0) {
                    leastPenetrationContact.Position = contactPoints[0];
                }
            }

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(IShape shape, Vector2 shapePos, Polygon polygon, out Contact? contact) {
            return TestSAT(shape, shapePos, polygon, new Vector2[] { }, out contact);
        }

        private bool TestSAT(Vector2 startPoint, Vector2 endPoint, IShape shape, Vector2 shapePos, IEnumerable<Vector2> axes, out Contact? contact) {
            Contact leastPenetrationContact = new Contact {
                Position = shapePos,
                PenetrationDepth = float.PositiveInfinity
            };

            foreach (Vector2 axis in axes) {
                Range projectionA = axis.Projection(startPoint, endPoint), projectionB = shape.Projection(shapePos, axis);
                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) {
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            // contact points
            float startPointProjection = startPoint.Projection(leastPenetrationContact.Normal),
                  endPointProjection = endPoint.Projection(leastPenetrationContact.Normal);

            Vector2 maxProjVertex;
            if (startPointProjection > endPointProjection) {
                maxProjVertex = startPoint;
            } else {
                maxProjVertex = endPoint;
            }

            (Vector2 MaxProjVertex, Line Edge) edgeA = (maxProjVertex, new Line(startPoint, endPoint));
            (Vector2 MaxProjVertex, Line Edge) edgeB = shape.FindBestClippingEdge(shapePos, -leastPenetrationContact.Normal);

            if (edgeB.Edge.PointA != edgeB.Edge.PointB) {
                Vector2[] contactPoints = CalculateContactPoints(edgeA, edgeB, leastPenetrationContact.Normal);

                if (contactPoints.Length > 0) {
                    leastPenetrationContact.Position = contactPoints[0];
                }
            }

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(Vector2 startPoint, Vector2 endPoint, Polygon polygon, IEnumerable<Vector2> axes, out Contact? contact) {
            Vector2[] intersections = polygon.Intersects(new Line(startPoint, endPoint));

            // no intersections, no contact
            if (intersections.Length == 0) {
                contact = null;
                return false;
            }

            Contact leastPenetrationContact = new Contact {
                Position = intersections[0],
                PenetrationDepth = float.PositiveInfinity
            };

            if (intersections.Length > 1 && Math.DistanceSquared(startPoint, intersections[1]) < Math.DistanceSquared(startPoint, intersections[0])) {
                // test if second intersection point it's closer than first one
                leastPenetrationContact.Position = intersections[1];
            }

            foreach (Vector2 axis in axes) {
                Range projectionA = axis.Projection(startPoint, endPoint),
                      projectionB = polygon.Projection(axis);

                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) {
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            // contact points
            float startPointProjection = startPoint.Projection(leastPenetrationContact.Normal),
                  endPointProjection = endPoint.Projection(leastPenetrationContact.Normal);

            Vector2 maxProjVertex;
            if (startPointProjection > endPointProjection) {
                maxProjVertex = startPoint;
            } else {
                maxProjVertex = endPoint;
            }

            (Vector2 MaxProjVertex, Line Edge) edgeA = (maxProjVertex, new Line(startPoint, endPoint));
            (Vector2 MaxProjVertex, Line Edge) edgeB = FindBestEdge(polygon, -leastPenetrationContact.Normal);

            Vector2[] contactPoints = CalculateContactPoints(edgeA, edgeB, leastPenetrationContact.Normal);

            if (contactPoints.Length > 0) {
                leastPenetrationContact.Position = contactPoints[0];
            }

            contact = leastPenetrationContact;
            return true;
        }

        #region Contact Points by Clipping

        internal static (Vector2 MaxProjVertex, Line Edge) FindBestEdge(Polygon polygon, Vector2 normal) {
            float maxProjection = float.NegativeInfinity;
            int index = -1;
            for (int i = 0; i < polygon.VertexCount; i++) {
                Vector2 v = polygon[i];
                float projection = v.Projection(normal);
                if (projection > maxProjection) {
                    maxProjection = projection;
                    index = i;
                }
            }

            Vector2 vertex = polygon[index],
                    nextVertex = polygon[(index + 1) % polygon.VertexCount],
                    previousVertex = polygon[index - 1 < 0 ? polygon.VertexCount - 1 : index - 1];

            Vector2 left = (vertex - nextVertex).Normalized(),
                    right = (vertex - previousVertex).Normalized();

            if (Vector2.Dot(right, normal) <= Vector2.Dot(left, normal)) {
                return (vertex, new Line(previousVertex, vertex));
            }

            return (vertex, new Line(vertex, nextVertex));
        }

        private Vector2[] CalculateContactPoints((Vector2 MaxProjVertex, Line Edge) edgeA, (Vector2 MaxProjVertex, Line Edge) edgeB, Vector2 normal) {
            (Vector2 MaxProjVertex, Line Edge) referenceEdge,
                                               incidentEdge;

            bool flip = false;

            if (Math.Abs(Vector2.Dot(edgeA.Edge.ToVector2(), normal)) <= Math.Abs(Vector2.Dot(edgeB.Edge.ToVector2(), normal))) {
                referenceEdge = edgeA;
                incidentEdge = edgeB;
            } else {
                flip = true;
                referenceEdge = edgeA;
                incidentEdge = edgeB;
            }

            //

            Vector2 refEdgeNormalized = referenceEdge.Edge.ToVector2().Normalized();

            float offsetA = Vector2.Dot(refEdgeNormalized, referenceEdge.Edge.PointA);
            List<Vector2> clippedPoints = Clip(incidentEdge.Edge.PointA, incidentEdge.Edge.PointB, refEdgeNormalized, offsetA);

            if (clippedPoints.Count < 2) {
                return new Vector2[0];
            }

            float offsetB = Vector2.Dot(refEdgeNormalized, referenceEdge.Edge.PointB);
            clippedPoints = Clip(clippedPoints[0], clippedPoints[1], -refEdgeNormalized, -offsetB);

            if (clippedPoints.Count < 2) {
                return new Vector2[0];
            }

            Vector2 refNormal = Vector2.Cross(referenceEdge.Edge.ToVector2(), -1f);
            if (flip) {
                refNormal = -refNormal;
            }

            float max = Vector2.Dot(refNormal, referenceEdge.MaxProjVertex);

            Vector2 clippedPointA = clippedPoints[0],
                    clippedPointB = clippedPoints[1];

            if (Vector2.Dot(refNormal, clippedPointA) - max < 0f) {
                clippedPoints.Remove(clippedPointA);
            }

            if (Vector2.Dot(refNormal, clippedPointB) - max < 0f) {
                clippedPoints.Remove(clippedPointB);
            }

            return clippedPoints.ToArray();
        }

        private List<Vector2> Clip(Vector2 pointA, Vector2 pointB, Vector2 normal, float offset) {
            List<Vector2> clippedPoints = new List<Vector2>();

            float dA = Vector2.Dot(normal, pointA) - offset,
                  dB = Vector2.Dot(normal, pointB) - offset;

            if (dA >= 0f) {
                clippedPoints.Add(pointA);
            }

            if (dB >= 0f) {
                clippedPoints.Add(pointB);
            }

            if (dA * dB < 0f) {
                float u = dA / (dA - dB);
                Vector2 edge = pointA + u * (pointB - pointA);
                clippedPoints.Add(edge);
            }

            return clippedPoints;
        }

        #endregion Contact Points by Clipping
    }
}
