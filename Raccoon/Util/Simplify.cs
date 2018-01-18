using System.Collections.Generic;

namespace Raccoon.Util {
    public static class Simplify {
        public static void PointsByDistance(ref List<Vector2> points, float distance) {
            if (points.Count <= 3) {
                return;
            }

            float squaredDist = distance * distance;
            Vector2 currentPoint, nextPoint;
            for (int i = 0; i < points.Count; i++) {
                currentPoint = points[i];
                nextPoint = points[(i + 1) % points.Count];

                if (Math.DistanceSquared(currentPoint, nextPoint) <= squaredDist) {
                    points.RemoveAt(i);
                    i--;
                }
            }
        }

        public static List<Vector2> PointsByDistance(IList<Vector2> points, float distance) {
            List<Vector2> simplifiedPoints = new List<Vector2>(points);
            PointsByDistance(ref simplifiedPoints, distance);
            return simplifiedPoints;
        }

        public static void PointsByTriDistance(ref List<Vector2> points, float distance) {
            if (points.Count <= 3) {
                return;
            }

            float squaredDist = distance * distance;
            Vector2 previousPoint, currentPoint, nextPoint;
            for (int i = 0; i < points.Count; i++) {
                previousPoint = points[i - 1 >= 0 ? i - 1 : points.Count - 1];
                currentPoint = points[i];
                nextPoint = points[(i + 1) % points.Count];

                if (Math.DistanceSquared(previousPoint, currentPoint) <= squaredDist && Math.DistanceSquared(currentPoint, nextPoint) <= squaredDist) {
                    points.RemoveAt(i);
                    i--;
                }
            }
        }

        public static List<Vector2> PointsByTriDistance(IList<Vector2> points, float distance) {
            List<Vector2> simplifiedPoints = new List<Vector2>(points);
            PointsByTriDistance(ref simplifiedPoints, distance);
            return simplifiedPoints;
        }

        public static void PointsByCollinearity(ref List<Vector2> points, float tolerance = Math.Epsilon) {
            if (points.Count <= 3) {
                return;
            }

            Vector2 previousPoint, currentPoint, nextPoint;
            for (int i = 0; i < points.Count; i++) {
                previousPoint = points[i - 1 >= 0 ? i - 1 : points.Count - 1];
                currentPoint = points[i];
                nextPoint = points[(i + 1) % points.Count];

                if (Math.IsCollinear(previousPoint, currentPoint, nextPoint, tolerance)) {
                    points.RemoveAt(i);
                    i--;
                }
            }
        }

        public static List<Vector2> PointsByCollinearity(IList<Vector2> points, float tolerance = Math.Epsilon) {
            List<Vector2> simplifiedPoints = new List<Vector2>(points);
            PointsByCollinearity(ref simplifiedPoints);
            return simplifiedPoints;
        }
    }
}
