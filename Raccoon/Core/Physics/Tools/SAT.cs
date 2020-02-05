using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon {
    public static class SAT {
        #region Public Methods

        /// <summary>
        /// Tests a Polygon with another Polygon with explicit axes.
        /// </summary>
        /// <param name="A">First element in the test.</param>
        /// <param name="B">Second element in the test.</param>
        /// <param name="axes">Separating axes to use when testing.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(Polygon A, Polygon B, IEnumerable<Vector2> axes, out Contact? contact) {
            if (A.IsConvex) {
                if (B.IsConvex) {
                    return TestConvexPolygons(A, B, axes, out contact);
                }

                return TestConvexPolygonWithConcavePolygon(A, B, axes, out contact);
            }

            if (B.IsConvex) {
                return TestConvexPolygonWithConcavePolygon(B, A, axes, out contact);
            }

            return TestConcavePolygons(A, B, axes, out contact);
        }

        /// <summary>
        /// Tests a Polygon with another Polygon without explicit axes.
        /// The axes will be infered by the Polygons faces normals.
        /// </summary>
        /// <param name="A">First element in the test.</param>
        /// <param name="B">Second element in the test.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(Polygon A, Polygon B, out Contact? contact) {
            if (A.IsConvex) {
                if (B.IsConvex) {
                    return TestConvexPolygons(A, B, out contact);
                }

                return TestConvexPolygonWithConcavePolygon(A, B, out contact);
            }

            if (B.IsConvex) {
                return TestConvexPolygonWithConcavePolygon(B, A, out contact);
            }

            return TestConcavePolygons(A, B, out contact);
        }

        /// <summary>
        /// Tests a IShape with another IShape with explicit axes.
        /// </summary>
        /// <param name="shapeA">First IShape in the test.</param>
        /// <param name="posA">First IShape position.</param>
        /// <param name="shapeB">Second IShape in the test.</param>
        /// <param name="posB">Second IShape position.</param>
        /// <param name="axes">Separating axes to use when testing.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(IShape shapeA, Vector2 posA, IShape shapeB, Vector2 posB, ICollection<Vector2> axes, out Contact? contact) {
            Polygon polygonA = null,
                    polygonB = null;

            if (shapeA is PolygonShape polygonShapeA) {
                polygonA = new Polygon(polygonShapeA.Shape);
                polygonA.Translate(posA);
            }

            if (shapeB is PolygonShape polygonShapeB) {
                polygonB = new Polygon(polygonShapeB.Shape);
                polygonB.Translate(posB);
            }

            if (polygonA != null) {
                if (polygonB != null) {
                    if (polygonA.IsConvex) {
                        if (polygonB.IsConvex) {
                            return TestConvexPolygons(polygonA, polygonB, axes, out contact);
                        }

                        return TestConvexPolygonWithConcavePolygon(polygonA, polygonB, axes, out contact);
                    }

                    if (polygonB.IsConvex) {
                        return TestConvexPolygonWithConcavePolygon(polygonB, polygonA, axes, out contact);
                    }

                    return TestConcavePolygons(polygonA, polygonB, axes, out contact);
                }

                if (polygonA.IsConvex) {
                    return TestIShapeWithConvexPolygon(shapeB, posB, polygonA, axes, out contact);
                }

                return TestIShapeWithConcavePolygon(shapeB, posB, polygonA, axes, out contact);
            } else if (polygonB != null) {

                if (polygonB.IsConvex) {
                    return TestIShapeWithConvexPolygon(shapeA, posA, polygonB, axes, out contact);
                }

                return TestIShapeWithConcavePolygon(shapeA, posA, polygonB, axes, out contact);
            }

            return TestIShapeWithIShape(shapeA, posA, shapeB, posB, axes, out contact);
        }

        /// <summary>
        /// Tests a IShape with another IShape without explicit axes.
        /// The axes will be infered by IShapes normals only.
        /// </summary>
        /// <param name="shapeA">First IShape in the test.</param>
        /// <param name="posA">First IShape position.</param>
        /// <param name="shapeB">Second IShape in the test.</param>
        /// <param name="posB">Second IShape position.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(IShape shapeA, Vector2 posA, IShape shapeB, Vector2 posB, out Contact? contact) {
            Polygon polygonA = null,
                    polygonB = null;

            if (shapeA is PolygonShape polygonShapeA) {
                polygonA = new Polygon(polygonShapeA.Shape);
                polygonA.Translate(posA);
            }

            if (shapeB is PolygonShape polygonShapeB) {
                polygonB = new Polygon(polygonShapeB.Shape);
                polygonB.Translate(posB);
            }

            if (polygonA != null) {
                if (polygonB != null) {
                    if (polygonA.IsConvex) {
                        if (polygonB.IsConvex) {
                            return TestConvexPolygons(polygonA, polygonB, out contact);
                        }

                        return TestConvexPolygonWithConcavePolygon(polygonA, polygonB, out contact);
                    }

                    if (polygonB.IsConvex) {
                        return TestConvexPolygonWithConcavePolygon(polygonB, polygonA, out contact);
                    }

                    return TestConcavePolygons(polygonA, polygonB, out contact);
                }

                if (polygonA.IsConvex) {
                    return TestIShapeWithConvexPolygon(shapeB, posB, polygonA, out contact);
                }

                return TestIShapeWithConcavePolygon(shapeB, posB, polygonA, out contact);
            } else if (polygonB != null) {

                if (polygonB.IsConvex) {
                    return TestIShapeWithConvexPolygon(shapeA, posA, polygonB, out contact);
                }

                return TestIShapeWithConcavePolygon(shapeA, posA, polygonB, out contact);
            }

            return TestIShapeWithIShape(shapeA, posA, shapeB, posB, out contact);
        }

        /// <summary>
        /// Tests IShape with a Polygon with explicit axes.
        /// </summary>
        /// <param name="shape">First element in the test.</param>
        /// <param name="shapePos">IShape position.</param>
        /// <param name="polygon">Second element in the test.</param>
        /// <param name="axes">Separating axes to use when testing.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(IShape shape, Vector2 shapePos, Polygon polygon, ICollection<Vector2> axes, out Contact? contact) {
            if (polygon.IsConvex) {
                return TestIShapeWithConvexPolygon(shape, shapePos, polygon, axes, out contact);
            }

            return TestIShapeWithConcavePolygon(shape, shapePos, polygon, axes, out contact);
        }

        /// <summary>
        /// Tests IShape with a Polygon without explicit axes.
        /// The axes will be infered by Polygon face normals only.
        /// </summary>
        /// <param name="shape">First element in the test.</param>
        /// <param name="shapePos">IShape position.</param>
        /// <param name="polygon">Second element in the test.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(IShape shape, Vector2 shapePos, Polygon polygon, out Contact? contact) {
            if (polygon.IsConvex) {
                return TestIShapeWithConvexPolygon(shape, shapePos, polygon, out contact);
            }

            return TestIShapeWithConcavePolygon(shape, shapePos, polygon, out contact);
        }

        /// <summary>
        /// Tests a line segment with a IShape with explicit axes.
        /// </summary>
        /// <param name="startPoint">Start point at line segment.</param>
        /// <param name="endPoint">End point at line segment.</param>
        /// <param name="shape">IShape element to test.</param>
        /// <param name="shapePos">IShape position.</param>
        /// <param name="axes">Separating axes to use when testing.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(Vector2 startPoint, Vector2 endPoint, IShape shape, Vector2 shapePos, IEnumerable<Vector2> axes, out Contact? contact) {
            if (shape is PolygonShape polygonShape) {
                Polygon polygon = new Polygon(polygonShape.Shape);
                polygon.Translate(shapePos);

                if (polygon.IsConvex) {
                    return TestLineSegmentWithConvexPolygon(startPoint, endPoint, polygon, axes, out contact);
                }

                return TestLineSegmentWithConcavePolygon(startPoint, endPoint, polygon, axes, out contact);
            }

            return TestLineSegmentWithIShape(startPoint, endPoint, shape, shapePos, axes, out contact);
        }
        ///
        /// <summary>
        /// Tests a line segment with a IShape without explicit axes.
        /// The axes will be infered by the IShape normals and line segment direction and perpendicular.
        /// </summary>
        /// <param name="startPoint">Start point at line segment.</param>
        /// <param name="endPoint">End point at line segment.</param>
        /// <param name="shape">IShape element to test.</param>
        /// <param name="shapePos">IShape position.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(Vector2 startPoint, Vector2 endPoint, IShape shape, Vector2 shapePos, out Contact? contact) {
            if (shape is PolygonShape polygonShape) {
                Polygon polygon = new Polygon(polygonShape.Shape);
                polygon.Translate(shapePos);

                if (polygon.IsConvex) {
                    return TestLineSegmentWithConvexPolygon(startPoint, endPoint, polygon, out contact);
                }

                return TestLineSegmentWithConcavePolygon(startPoint, endPoint, polygon, out contact);
            }

            return TestLineSegmentWithIShape(startPoint, endPoint, shape, shapePos, out contact);
        }

        /// <summary>
        /// Tests a line segment with a Polygon with explicit axes.
        /// </summary>
        /// <param name="startPoint">Start point at line segment.</param>
        /// <param name="endPoint">End point at line segment.</param>
        /// <param name="polygon">Polygon to test.</param>
        /// <param name="axes">Separating axes to use when testing.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(Vector2 startPoint, Vector2 endPoint, Polygon polygon, IEnumerable<Vector2> axes, out Contact? contact) {
            if (polygon.IsConvex) {
                return TestLineSegmentWithConvexPolygon(startPoint, endPoint, polygon, axes, out contact);
            }

            return TestLineSegmentWithConcavePolygon(startPoint, endPoint, polygon, axes, out contact);
        }

        /// <summary>
        /// Tests a line segment with a Polygon without explicit axes.
        /// The axes will be infered by the Polygon face normals and line segment direction and perpendicular.
        /// </summary>
        /// <param name="startPoint">Start point at line segment.</param>
        /// <param name="endPoint">End point at line segment.</param>
        /// <param name="polygon">Polygon to test.</param>
        /// <param name="contact">Contact info about the test, Null otherwise.</param>
        /// <returns>True if they're intersecting, False otherwise.</returns>
        public static bool Test(Vector2 startPoint, Vector2 endPoint, Polygon polygon, out Contact? contact) {
            if (polygon.IsConvex) {
                return TestLineSegmentWithConvexPolygon(startPoint, endPoint, polygon, out contact);
            }

            return TestLineSegmentWithConcavePolygon(startPoint, endPoint, polygon, out contact);
        }

        #endregion Public Methods

        #region Private Methods

        #region Polygon with Polygon

        private static bool TestConvexPolygons(Polygon A, Polygon B, IEnumerable<Vector2> axes, out Contact? contact) {
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

        private static bool TestConvexPolygons(Polygon A, Polygon B, out Contact? contact) {
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

            return TestConvexPolygons(A, B, axes, out contact);
        }

        private static bool TestConcavePolygons(Polygon A, Polygon B, IEnumerable<Vector2> axes, out Contact? contact) {
            List<Vector2[]> componentsA = A.ConvexComponents(),
                            componentsB = B.ConvexComponents();

            Vector2 polyAAnchor = A[0],
                    polyBAnchor = B[0];

            foreach (Vector2[] componentA in componentsA) {
                Polygon componentPolyA = new Polygon(componentA);
                componentPolyA.Translate(polyAAnchor);

                foreach (Vector2[] componentB in componentsB) {
                    Polygon componentPolyB = new Polygon(componentB);
                    componentPolyB.Translate(polyBAnchor);

                    if (TestConvexPolygons(componentPolyA, componentPolyB, axes, out contact)) {
                        return true;
                    }
                }
            }

            contact = null;
            return false;
        }

        private static bool TestConcavePolygons(Polygon A, Polygon B, out Contact? contact) {
            List<Vector2[]> componentsA = A.ConvexComponents(),
                            componentsB = B.ConvexComponents();

            Vector2 polyAAnchor = A[0],
                    polyBAnchor = B[0];

            Vector2[] axes;
            int i;

            foreach (Vector2[] componentA in componentsA) {
                Polygon componentPolyA = new Polygon(componentA);
                componentPolyA.Translate(polyAAnchor);

                foreach (Vector2[] componentB in componentsB) {
                    Polygon componentPolyB = new Polygon(componentB);
                    componentPolyB.Translate(polyBAnchor);

                    axes = new Vector2[componentPolyA.Normals.Length + componentPolyB.Normals.Length];

                    i = 0;

                    // component polygon A axes
                    foreach (Vector2 normal in componentPolyA.Normals) {
                        axes[i] = normal;
                        i++;
                    }

                    // component polygon B axes
                    foreach (Vector2 normal in componentPolyB.Normals) {
                        axes[i] = normal;
                        i++;
                    }

                    if (TestConvexPolygons(componentPolyA, componentPolyB, axes, out contact)) {
                        return true;
                    }
                }
            }

            contact = null;
            return false;
        }

        private static bool TestConvexPolygonWithConcavePolygon(Polygon convexPolygon, Polygon concavePolygon, IEnumerable<Vector2> axes, out Contact? contact) {
            List<Vector2[]> concavePolygonComponents = concavePolygon.ConvexComponents();
            Vector2 concavePolygonAnchor = concavePolygon[0];

            foreach (Vector2[] component in concavePolygonComponents) {
                Polygon componentPolygon = new Polygon(component);
                componentPolygon.Translate(concavePolygonAnchor);

                if (TestConvexPolygons(convexPolygon, componentPolygon, axes, out contact)) {
                    return true;
                }
            }

            contact = null;
            return false;
        }

        private static bool TestConvexPolygonWithConcavePolygon(Polygon convexPolygon, Polygon concavePolygon, out Contact? contact) {
            List<Vector2[]> concavePolygonComponents = concavePolygon.ConvexComponents();

            Vector2 concavePolygonAnchor = concavePolygon[0];

            Vector2[] axes;
            int i;

            foreach (Vector2[] component in concavePolygonComponents) {
                Polygon componentPolygon = new Polygon(component);
                componentPolygon.Translate(concavePolygonAnchor);

                axes = new Vector2[componentPolygon.Normals.Length + convexPolygon.Normals.Length];

                i = 0;

                // component polygon A axes
                foreach (Vector2 normal in convexPolygon.Normals) {
                    axes[i] = normal;
                    i++;
                }

                // component polygon B axes
                foreach (Vector2 normal in componentPolygon.Normals) {
                    axes[i] = normal;
                    i++;
                }

                if (TestConvexPolygons(convexPolygon, componentPolygon, axes, out contact)) {
                    return true;
                }
            }

            contact = null;
            return false;
        }

        #endregion Polygon with Polygon

        #region IShape with Polygon

        public static bool TestIShapeWithConvexPolygon(IShape shape, Vector2 shapePos, Polygon polygon, ICollection<Vector2> axes, out Contact? contact) {
            Debug.Assert(!(shape is PolygonShape), "TestIShapeWithConvexPolygon can't handle PolygonShape correctly. Please use another polygon specific variant.");

            Contact leastPenetrationContact = new Contact {
                Position = shapePos,
                PenetrationDepth = float.PositiveInfinity
            };

            foreach (Vector2 axis in axes) {
                Range projectionA = shape.Projection(shapePos, axis), 
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

        public static bool TestIShapeWithConvexPolygon(IShape shape, Vector2 shapePos, Polygon polygon, out Contact? contact) {
            Vector2[] shapeAxes = shape.CalculateAxes();
            Vector2[] axes = new Vector2[shapeAxes.Length + polygon.Normals.Length];

            shapeAxes.CopyTo(axes, 0);
            polygon.Normals.CopyTo(axes, shapeAxes.Length);

            return TestIShapeWithConvexPolygon(shape, shapePos, polygon, polygon.Normals, out contact);
        }

        public static bool TestIShapeWithConcavePolygon(IShape shape, Vector2 shapePos, Polygon concavePolygon, ICollection<Vector2> axes, out Contact? contact) {
            List<Vector2[]> concavePolygonComponents = concavePolygon.ConvexComponents();

            Vector2 concavePolygonAnchor = concavePolygon[0];

            foreach (Vector2[] component in concavePolygonComponents) {
                Polygon componentPolygon = new Polygon(component);
                componentPolygon.Translate(concavePolygonAnchor);

                if (TestIShapeWithConvexPolygon(shape, shapePos, componentPolygon, axes, out contact)) {
                    return true;
                }
            }

            contact = null;
            return false;
        }

        public static bool TestIShapeWithConcavePolygon(IShape shape, Vector2 shapePos, Polygon concavePolygon, out Contact? contact) {
            List<Vector2[]> concavePolygonComponents = concavePolygon.ConvexComponents();

            Vector2 concavePolygonAnchor = concavePolygon[0];

            Vector2[] shapeAxes = shape.CalculateAxes();
            Vector2[] axes;

            foreach (Vector2[] component in concavePolygonComponents) {
                Polygon componentPolygon = new Polygon(component);
                componentPolygon.Translate(concavePolygonAnchor);

                axes = new Vector2[shapeAxes.Length + componentPolygon.Normals.Length];

                shapeAxes.CopyTo(axes, 0);
                componentPolygon.Normals.CopyTo(axes, shapeAxes.Length);

                if (TestIShapeWithConvexPolygon(shape, shapePos, componentPolygon, axes, out contact)) {
                    return true;
                }
            }

            contact = null;
            return false;
        }

        #endregion IShape with Polygon

        #region IShape with IShape

        public static bool TestIShapeWithIShape(IShape shapeA, Vector2 posA, IShape shapeB, Vector2 posB, ICollection<Vector2> axes, out Contact? contact) {
            Debug.Assert(!(shapeA is PolygonShape), "TestIShapeWithIShape can't handle PolygonShape correctly. Please use another polygon specific variant.");
            Debug.Assert(!(shapeB is PolygonShape), "TestIShapeWithIShape can't handle PolygonShape correctly. Please use another polygon specific variant.");

            Contact leastPenetrationContact = new Contact {
                Position = posA,
                PenetrationDepth = float.PositiveInfinity
            };

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

        public static bool TestIShapeWithIShape(IShape shapeA, Vector2 posA, IShape shapeB, Vector2 posB, out Contact? contact) {
            Vector2[] shapeAAxes = shapeA.CalculateAxes(),
                      shapeBAxes = shapeB.CalculateAxes();

            Vector2[] axes = new Vector2[shapeAAxes.Length + shapeBAxes.Length];

            shapeAAxes.CopyTo(axes, 0);
            shapeBAxes.CopyTo(axes, shapeAAxes.Length);

            return TestIShapeWithIShape(shapeA, posA, shapeB, posB, axes, out contact);
        }

        #endregion IShape with IShape

        #region Line Segment with Polygon

        public static bool TestLineSegmentWithConvexPolygon(Vector2 startPoint, Vector2 endPoint, Polygon polygon, IEnumerable<Vector2> axes, out Contact? contact) {
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

        public static bool TestLineSegmentWithConvexPolygon(Vector2 startPoint, Vector2 endPoint, Polygon polygon, out Contact? contact) {
            Vector2[] axes = new Vector2[2 + polygon.Normals.Length];

            Vector2 direction = (endPoint - startPoint).Normalized();
            axes[0] = direction;
            axes[1] = direction.PerpendicularCW();

            polygon.Normals.CopyTo(axes, 2);

            return TestLineSegmentWithConvexPolygon(startPoint, endPoint, polygon, axes, out contact);
        }

        public static bool TestLineSegmentWithConcavePolygon(Vector2 startPoint, Vector2 endPoint, Polygon concavePolygon, IEnumerable<Vector2> axes, out Contact? contact) {
            List<Vector2[]> concavePolygonComponents = concavePolygon.ConvexComponents();
            Vector2 concavePolygonAnchor = concavePolygon[0];

            foreach (Vector2[] component in concavePolygonComponents) {
                Polygon componentPolygon = new Polygon(component);
                componentPolygon.Translate(concavePolygonAnchor);

                if (TestLineSegmentWithConvexPolygon(startPoint, endPoint, componentPolygon, axes, out contact)) {
                    return true;
                }
            }

            contact = null;
            return false;
        }

        public static bool TestLineSegmentWithConcavePolygon(Vector2 startPoint, Vector2 endPoint, Polygon concavePolygon, out Contact? contact) {
            List<Vector2[]> concavePolygonComponents = concavePolygon.ConvexComponents();
            Vector2 concavePolygonAnchor = concavePolygon[0];

            Vector2 direction = (endPoint - startPoint).Normalized();
            Vector2[] axes;
            int i;

            foreach (Vector2[] component in concavePolygonComponents) {
                Polygon componentPolygon = new Polygon(component);
                componentPolygon.Translate(concavePolygonAnchor);

                axes = new Vector2[2 + componentPolygon.Normals.Length];

                axes[0] = direction;
                axes[1] = direction.PerpendicularCW();

                i = 2;

                // component polygon A axes
                foreach (Vector2 normal in componentPolygon.Normals) {
                    axes[i] = normal;
                    i++;
                }

                if (TestLineSegmentWithConvexPolygon(startPoint, endPoint, componentPolygon, axes, out contact)) {
                    return true;
                }
            }

            contact = null;
            return false;
        }

        #endregion Line Segment with Polygon

        #region Line Segment with IShape

        public static bool TestLineSegmentWithIShape(Vector2 startPoint, Vector2 endPoint, IShape shape, Vector2 shapePos, IEnumerable<Vector2> axes, out Contact? contact) {
            Debug.Assert(!(shape is PolygonShape), "TestILineSegmentWithIShape can't handle PolygonShape correctly. Please use another polygon specific variant.");

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

        public static bool TestLineSegmentWithIShape(Vector2 startPoint, Vector2 endPoint, IShape shape, Vector2 shapePos, out Contact? contact) {
            Vector2[] shapeAxes = shape.CalculateAxes();
            Vector2[] axes = new Vector2[2 + shapeAxes.Length];

            Vector2 direction = (endPoint - startPoint).Normalized();
            axes[0] = direction;
            axes[1] = direction.PerpendicularCW();

            shapeAxes.CopyTo(axes, 2);

            return TestLineSegmentWithIShape(startPoint, endPoint, shape, shapePos, axes, out contact);
        }

        #endregion Line Segment with IShape

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

        private static Vector2[] CalculateContactPoints((Vector2 MaxProjVertex, Line Edge) edgeA, (Vector2 MaxProjVertex, Line Edge) edgeB, Vector2 normal) {
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

        private static List<Vector2> Clip(Vector2 pointA, Vector2 pointB, Vector2 normal, float offset) {
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

        #endregion Private Methods
    }
}
