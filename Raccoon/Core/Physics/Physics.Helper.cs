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
                Range projectionA = A.Projection(axis), projectionB = B.Projection(axis);
                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) { //BiasGreaterThan(leastPenetrationContact.PenetrationDepth, penetrationDepth)) {
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

        private bool TestSAT(IShape shapeA, IShape shapeB, ICollection<Vector2> axes, out Contact? contact) {
            Contact leastPenetrationContact = new Contact {
                Position = shapeA.Body.Position,
                PenetrationDepth = float.PositiveInfinity
            };

            Vector2[] a = new Vector2[axes.Count + 1];
            a[0] = (shapeB.Body.Position - shapeA.Body.Position).Normalized();
            axes.CopyTo(a, 1);

            foreach (Vector2 axis in a) {
                Range projectionA = shapeA.Projection(axis), projectionB = shapeB.Projection(axis);
                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) { //BiasGreaterThan(leastPenetrationContact.PenetrationDepth, penetrationDepth)) {
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(IShape shape, Polygon polygon, ICollection<Vector2> axes, out Contact? contact) {
            Contact leastPenetrationContact = new Contact {
                Position = shape.Body.Position,
                PenetrationDepth = float.PositiveInfinity
            };

            Vector2[] a = new Vector2[polygon.Normals.Length + axes.Count + 1];
            a[0] = (shape.Body.Position - polygon.Center).Normalized();
            polygon.Normals.CopyTo(a, 1);
            axes.CopyTo(a, polygon.Normals.Length + 1);

            foreach (Vector2 axis in a) {
                Range projectionA = shape.Projection(axis), projectionB = polygon.Projection(axis);
                if (!projectionA.Overlaps(projectionB, out float penetrationDepth)) {
                    contact = null;
                    return false;
                }

                if (penetrationDepth < leastPenetrationContact.PenetrationDepth) { //BiasGreaterThan(leastPenetrationContact.PenetrationDepth, penetrationDepth)) {
                    leastPenetrationContact.PenetrationDepth = penetrationDepth;
                    leastPenetrationContact.Normal = projectionA.Min > projectionB.Min ? -axis : axis;
                }
            }

            contact = leastPenetrationContact;
            return true;
        }

        private bool TestSAT(IShape shape, Polygon polygon, out Contact? contact) {
            return TestSAT(shape, polygon, new Vector2[] { }, out contact);
        }

        private bool BiasGreaterThan(float a, float b) {
          return a >= (b * .95f + a * .01f);
        }
    }
}
