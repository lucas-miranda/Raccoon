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

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(Polygon A, Polygon B, out Contact? contact) {
            Vector2[] axes = new Vector2[A.Normals.Length + B.Normals.Length + 1];

            axes[0] = (B.Center - A.Center).Normalized();
            int i = 1;

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

            Vector2[] a = new Vector2[axes.Count + 1];
            a[0] = (posB - posA).Normalized();
            axes.CopyTo(a, 1);

            foreach (Vector2 axis in a) {
                Range projectionA = shapeA.Projection(posA, axis), projectionB = shapeB.Projection(posB, axis);
                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) { 
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
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

            Vector2[] a = new Vector2[polygon.Normals.Length + axes.Count + 1];
            a[0] = (shapePos - polygon.Center).Normalized();
            polygon.Normals.CopyTo(a, 1);
            axes.CopyTo(a, polygon.Normals.Length + 1);

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
                Position =  intersections[0],
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

            contact = leastPenetrationContact;
            return true;
        }
    }
}
