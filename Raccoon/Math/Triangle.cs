using System.Collections.Generic;

namespace Raccoon {
    public struct Triangle {
        #region Private Members

        private float _barycDenominator;

        #endregion Private Members

        #region Public Members

        public Vector2 A, B, C;

        #endregion Public Members

        #region Constructors

        public Triangle(Vector2 a, Vector2 b, Vector2 c) {
            A = a;
            B = b;
            C = c;
            IsValid = Validate(A, B, C);

            // barycentric coordinate system
            _barycDenominator = ((B.Y - C.Y) * (A.X - C.X) + (C.X - B.X) * (A.Y - C.Y));
        }

        #endregion Constructors

        #region Public Properties

        public bool IsValid { get; }

        #endregion Public Properties

        #region Public Static Methods

        public static bool Validate(Vector2 a, Vector2 b, Vector2 c) {
            return Vector2.Cross(a, b, c) != 0;
        }

        #endregion Public Static Methods

        #region Public Methods

        public void RotateAround(float degrees, Vector2 origin) {
            A = Util.Math.RotateAround(A, origin, degrees);
            B = Util.Math.RotateAround(B, origin, degrees);
            C = Util.Math.RotateAround(C, origin, degrees);
        }

        public void Rotate(float degrees) {
            RotateAround(degrees, (A + B + C) / 3f);
        }

        public bool Contains(Vector2 point) {
            float a = ((B.Y - C.Y) * (point.X - C.X) + (C.X - B.X) * (point.Y - C.Y)) / _barycDenominator,
                  b = ((C.Y - A.Y) * (point.X - C.X) + (A.X - C.X) * (point.Y - C.Y)) / _barycDenominator,
                  c = 1 - a - b;
            
            return !(0 > a || a > 1 || 0 > b || b > 1 || 0 > c || c <= 1);
        }

        public IEnumerator<Line> Edges() {
            yield return new Line(A, B);
            yield return new Line(B, C);
            yield return new Line(C, A);
        }

        public override bool Equals(object obj) {
            return obj is Triangle && Equals((Triangle) obj);
        }

        public bool Equals(Triangle t) {
            return this == t;
        }

        public override string ToString() {
            return $"[{A} {B} {C}]";
        }

        public override int GetHashCode() {
            var hashCode = 793064651;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            return hashCode;
        }

        #endregion Public Methods

        #region Operators

        public static bool operator ==(Triangle l, Triangle r) {
            return (l.A == r.A && ((l.B == r.B && l.C == r.C) || (l.B == r.C && l.C == r.B)))
                || (l.A == r.B && ((l.B == r.C && l.C == r.A) || (l.B == r.A && l.C == r.C)))
                || (l.A == r.C && ((l.B == r.A && l.C == r.B) || (l.B == r.B && l.C == r.A)));
        }

        public static bool operator !=(Triangle l, Triangle r) {
            return !(l == r);
        }

        public static Triangle operator +(Triangle l, Vector2 r) {
            return new Triangle(l.A + r, l.B + r, l.C + r);
        }

        public static Triangle operator +(Vector2 l, Triangle r) {
            return r + l;
        }

        public static Triangle operator -(Triangle l, Vector2 r) {
            return new Triangle(l.A - r, l.B - r, l.C - r);
        }

        public static Triangle operator *(Triangle l, float r) {
            return new Triangle(l.A * r, l.B * r, l.C * r);
        }

        public static Triangle operator *(float l, Triangle r) {
            return r * l;
        }

        public static Triangle operator /(Triangle l, float r) {
            return new Triangle(l.A / r, l.B / r, l.C / r);
        }

        public static Triangle operator /(float l, Triangle r) {
            return new Triangle(l / r.A, l / r.B, l / r.C);
        }

        #endregion Operators
    }
}
