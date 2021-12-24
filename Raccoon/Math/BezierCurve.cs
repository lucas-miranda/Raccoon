using Raccoon.Util;

namespace Raccoon {
    public class BezierCurve {
        private Vector2[] _controlPoints;

        public BezierCurve(int controlPoints) {
            if (controlPoints != 3 && controlPoints != 4) {
                throw new System.NotSupportedException($"Can't create a bezier curve with {controlPoints} control points. (Supported values are: 3 or 4)");
            }

            _controlPoints = new Vector2[controlPoints];
        }

        public Vector2[] ControlPoints { get { return _controlPoints; } }
        public Vector2 A { get { return _controlPoints[0]; } }
        public Vector2 B { get { return _controlPoints[1]; } }
        public Vector2 C { get { return _controlPoints[2]; } }
        public Vector2 D { get { return _controlPoints[3]; } }

        public Vector2 this[int controlPointIndex] {
            get {
                return _controlPoints[controlPointIndex];
            }
        }

        public Vector2 this[float t] {
            get {
                return Get(t);
            }
        }

        public int ControlPointsCount { get { return _controlPoints == null ? 0 : _controlPoints.Length; } }

        public static BezierCurve From(Vector2 a, Vector2 b, Vector2 c) {
            BezierCurve bezierCurve = new BezierCurve(controlPoints: 3);
            bezierCurve.SetControlPoints(a, b, c);
            return bezierCurve;
        }

        public static BezierCurve From(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            BezierCurve bezierCurve = new BezierCurve(controlPoints: 4);
            bezierCurve.SetControlPoints(a, b, c, d);
            return bezierCurve;
        }

        public static Vector2 CalculateControlPointC(Vector2 a, Vector2 b, Vector2 d, float t, Vector2 expectedValue) {
            float iv_t = 1f - t,
                  div = 3f * iv_t * t * t;

            if (Math.EqualsEstimate(div, 0f)) {
                return Vector2.Zero;
            }

            return (expectedValue - (iv_t * iv_t * iv_t * a + 3f * iv_t * iv_t * t * b + t * t * t * d)) / div;
        }

        public static Vector2 CalculateControlPointC(BezierCurve bezierCurve, float t, Vector2 expectedValue) {
            if (bezierCurve.ControlPointsCount == 3) {
                throw new System.NotImplementedException();
            }

            return CalculateControlPointC(bezierCurve.A, bezierCurve.B, bezierCurve.D, t, expectedValue);
        }

        public Vector2 GetControlPoint(int index) {
            if (_controlPoints == null) {
                throw new System.InvalidOperationException("BezierCurve not initialized.");
            }

            if (index < 0 || index >= _controlPoints.Length) {
                throw new System.ArgumentOutOfRangeException("index");
            }

            return _controlPoints[index];
        }

        public void SetControlPoint(int index, Vector2 controlPoint) {
            if (_controlPoints == null) {
                throw new System.InvalidOperationException("BezierCurve not initialized.");
            }

            if (index < 0 || index >= _controlPoints.Length) {
                throw new System.ArgumentOutOfRangeException("index");
            }

            _controlPoints[index] = controlPoint;
        }

        public void SetControlPoints(Vector2 a, Vector2 b, Vector2 c) {
            int controlPoints = ControlPointsCount;
            if (controlPoints <= 0) {
                throw new System.InvalidOperationException("BezierCurve not initialized.");
            }

            if (controlPoints > 3) {
                throw new System.ArgumentException($"BezierCurve contains 4 control points, use SetControlPoints with 4 points.");
            }

            _controlPoints[0] = a;
            _controlPoints[1] = b;
            _controlPoints[2] = c;
        }

        public void SetControlPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
            int controlPoints = ControlPointsCount;
            if (controlPoints <= 0) {
                throw new System.InvalidOperationException("BezierCurve not initialized.");
            }

            if (controlPoints < 4) {
                throw new System.ArgumentException($"BezierCurve contains only 3 control points, use SetControlPoints with 3 points.");
            }

            _controlPoints[0] = a;
            _controlPoints[1] = b;
            _controlPoints[2] = c;
            _controlPoints[3] = d;
        }

        public Vector2 Get(float t) {
            if (_controlPoints == null || _controlPoints.Length == 0) {
                throw new System.InvalidOperationException("BezierCurve not initialized.");
            }

            float iv_t = 1f - t;
            if (_controlPoints.Length == 3) {
                return iv_t * iv_t * A + 2f * iv_t * t * B + t * t * C;
            } else if (_controlPoints.Length == 4) {
                return iv_t * iv_t * iv_t * A + 3f * iv_t * iv_t * t * B + 3f * iv_t * t * t * C + t * t * t * D;
            }

            throw new System.NotSupportedException("Only BezierCurves with 3 or 4 control points are supported.");
        }
    }
}
