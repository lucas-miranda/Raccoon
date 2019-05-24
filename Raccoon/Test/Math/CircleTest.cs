using NUnit.Framework;

namespace Raccoon.Tests.Math {
    class CircleTest {
        [TestCase(0, -20,  0, 20,  0, 0, 16,  0, -16,  0, 16)]
        public void IntersectionPoints_Expected2Points(float linePointAX, float linePointAY, float linePointBX, float linePointBY, float circleCenterX, float circleCenterY, float circleRadius, float expectedPointAX, float expectedPointAY, float expectedPointBX, float expectedPointBY) {
            Vector2[] expectedResult = new Vector2[] {
                new Vector2(expectedPointAX, expectedPointAY),
                new Vector2(expectedPointBX, expectedPointBY)
            };

            Line line = new Line(new Vector2(linePointAX, linePointAY), new Vector2(linePointBX, linePointBY));
            Circle circle = new Circle(new Vector2(circleCenterX, circleCenterY), circleRadius);

            Vector2[] intersectionPoints = circle.IntersectionPoints(line);
            Assert.AreEqual(expectedResult.Length, intersectionPoints.Length);
            Assert.AreEqual(expectedResult[0], intersectionPoints[0]);
            Assert.AreEqual(expectedResult[1], intersectionPoints[1]);
        }

        [TestCase(0, -20,  0, 0,  0, 0, 16,  0, -16)]
        [TestCase(0, 20,  0, 0,  0, 0, 16,  0, 16)]
        public void IntersectionPoints_Expected1Point(float linePointAX, float linePointAY, float linePointBX, float linePointBY, float circleCenterX, float circleCenterY, float circleRadius, float expectedPointAX, float expectedPointAY) {
            Vector2[] expectedResult = new Vector2[] {
                new Vector2(expectedPointAX, expectedPointAY)
            };

            Line line = new Line(new Vector2(linePointAX, linePointAY), new Vector2(linePointBX, linePointBY));
            Circle circle = new Circle(new Vector2(circleCenterX, circleCenterY), circleRadius);

            Vector2[] intersectionPoints = circle.IntersectionPoints(line);
            Assert.AreEqual(expectedResult.Length, intersectionPoints.Length);
            Assert.AreEqual(expectedResult[0], intersectionPoints[0]);
        }

        [TestCase(0, -20,  0, -17,  0, 0, 16)]
        public void IntersectionPoints_Expected0Points(float linePointAX, float linePointAY, float linePointBX, float linePointBY, float circleCenterX, float circleCenterY, float circleRadius) {
            Vector2[] expectedResult = new Vector2[] { };

            Line line = new Line(new Vector2(linePointAX, linePointAY), new Vector2(linePointBX, linePointBY));
            Circle circle = new Circle(new Vector2(circleCenterX, circleCenterY), circleRadius);

            Vector2[] intersectionPoints = circle.IntersectionPoints(line);
            Assert.AreEqual(expectedResult.Length, intersectionPoints.Length);
        }
    }
}
