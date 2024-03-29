﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

using Raccoon.Util;

namespace Raccoon {
    public struct Vector2 : System.IEquatable<Vector2> {
        #region Private Members

        private static readonly Regex StringFormatRegex = new Regex(@"(\-?\d+(?:\.?\d+)?)(?: *, *| +)(\-?\d+(?:\.?\d+)?)");

        #endregion Private Members

        #region Public Members

        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);
        public static readonly Vector2 Up = new Vector2(0, -1);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Down = new Vector2(0, 1);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 UpLeft = new Vector2(-1, -1);
        public static readonly Vector2 UpRight = new Vector2(1, -1);
        public static readonly Vector2 DownRight = new Vector2(1, 1);
        public static readonly Vector2 DownLeft = new Vector2(-1, 1);
        public static readonly Vector2 UnitX = new Vector2(1, 0);
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        public float X, Y;

        #endregion Public Members

        #region Constructors

        public Vector2(float x, float y) {
            X = x;
            Y = y;
        }

        public Vector2(float xy) : this(xy, xy) { }
        public Vector2(float[] xy) : this(xy[0], xy[1]) { }
        public Vector2(double x, double y) : this((float) x, (float) y) { }
        public Vector2(double xy) : this((float) xy, (float) xy) { }
        public Vector2(double[] xy) : this((float) xy[0], (float) xy[1]) { }
        public Vector2(Size size) : this(size.Width, size.Height) { }

        internal Vector2(Microsoft.Xna.Framework.Vector2 vec2) : this(vec2.X, vec2.Y) { }

        #endregion Constructors

        #region Public Static Methods

        public static float Dot(Vector2 a, Vector2 b) {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(Vector2 a, Vector2 b) {
            return a.X * b.Y - a.Y * b.X;
        }

        public static float Cross(Vector2 a, Vector2 b, Vector2 c) {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        public static Vector2 Cross(Vector2 a, float n) {
            return new Vector2(n * a.Y, -n * a.X);
        }

        public static Vector2 Cross(float n, Vector2 a) {
            return new Vector2(-n * a.Y, n * a.X);
        }

        public static Vector2 Normalize(Vector2 v) {
            if (Math.EqualsEstimate(v.X, 0f) && Math.EqualsEstimate(v.Y, 0f)) {
                return Zero;
            }

            float invLength = 1.0f / (float) System.Math.Sqrt(v.X * v.X + v.Y * v.Y);
            return new Vector2(v.X * invLength, v.Y * invLength);
        }

        public static Vector2 Lerp(Vector2 start, Vector2 end, float t) {
            return new Vector2(Math.Lerp(start.X, end.X, t), Math.Lerp(start.Y, end.Y, t));
        }

        public static Vector2 CatmullRom(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float amount) {
            return new Vector2(Math.CatmullRom(v1.X, v2.X, v3.X, v4.X, amount), Math.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, amount));
        }

        public static bool EqualsEstimate(Vector2 v1, Vector2 v2, float tolerance = Math.Epsilon) {
            return Math.EqualsEstimate(v1.X, v2.X, tolerance) && Math.EqualsEstimate(v1.Y, v2.Y, tolerance);
        }

        public static Vector2 Parse(string value) {
            MatchCollection matches = StringFormatRegex.Matches(value);

            if (matches.Count == 0 || !matches[0].Success) {
                throw new System.FormatException($"String '{value}' doesn't not typify a Vector2.");
            }

            return new Vector2(
                float.Parse(matches[0].Groups[1].Value),
                float.Parse(matches[0].Groups[2].Value)
            );
        }

        public static bool TryParse(string value, out Vector2 result) {
            MatchCollection matches = StringFormatRegex.Matches(value);

            if (matches.Count == 0 || !matches[0].Success) {
                result = Zero;
                return false;
            }

            result = new Vector2(
                float.Parse(matches[0].Groups[1].Value),
                float.Parse(matches[0].Groups[2].Value)
            );

            return true;
        }

        #endregion Public Static Methods

        #region Public Methods

        public float Length() {
            return (float) System.Math.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared() {
            return X * X + Y * Y;
        }

        public Vector2 Normalized() {
            return Normalize(this);
        }

        public void Decompose(out Vector2 normal, out float length) {
            if (Math.EqualsEstimate(X, 0f) && Math.EqualsEstimate(Y, 0f)) {
                length = 0f;
                normal = Zero;
                return;
            }

            length = Length();
            float invLength = 1.0f / length;
            normal = new Vector2(X * invLength, Y * invLength);
        }

        public Vector2 PerpendicularCCW() {
            return new Vector2(Y, -X);
        }

        public Vector2 PerpendicularCW() {
            return new Vector2(-Y, X);
        }

        public Range Projection(ICollection<Vector2> points) {
            if (points.Count == 0) {
                throw new System.ArgumentException("Projection needs at least one value.", "points");
            }

            IEnumerator<Vector2> enumerator = points.GetEnumerator();
            enumerator.MoveNext();

            float min = Dot(this, enumerator.Current),
                  max = min;

            while (enumerator.MoveNext()) {
                float p = Dot(this, enumerator.Current);
                if (p < min) {
                    min = p;
                } else if (p > max) {
                    max = p;
                }
            }

            return new Range(min, max);
        }

        public Range Projection(params Vector2[] points) {
            return Projection(points as ICollection<Vector2>);
        }

        public float Projection(Vector2 point) {
            return Dot(this, point);
        }

        public Vector2 WithX(float x) {
            return new Vector2(x, Y);
        }

        public Vector2 WithY(float y) {
            return new Vector2(X, y);
        }

        public Vector2 NearestRightAngle() {
            if (this == Vector2.Zero) {
                return Vector2.Zero;
            }

            float verticalProjection = Vector2.Up.Projection(this);

            if (verticalProjection >= .5f) {
                return Vector2.Up;
            } else if (verticalProjection <= -.5f) {
                return Vector2.Down;
            }

            if (Vector2.Right.Projection(this) >= .5f) {
                return Vector2.Right;
            }

            return Vector2.Left;
        }

        public override bool Equals(object obj) {
            return obj is Vector2 && Equals((Vector2) obj);
        }

        public bool Equals(Vector2 other) {
            return this == other;
        }

        public Direction ToDirection() {
            Direction dir = Direction.None;
            if (X > 0) {
                dir |= Direction.Right;
            } else if (X < 0) {
                dir |= Direction.Left;
            }

            if (Y > 0) {
                dir |= Direction.Up;
            } else if (Y < 0) {
                dir |= Direction.Down;
            }

            return dir;
        }

        public override string ToString() {
            return $"[{X.ToString("0.###")} {Y.ToString("0.###")}]";
        }

        public override int GetHashCode() {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        #endregion Public Methods

        #region Implicit Conversions

        public static implicit operator Microsoft.Xna.Framework.Vector2(Vector2 v) {
            return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
        }

        #endregion Implicit Conversions

        #region Operators

        public static bool operator ==(Vector2 l, Vector2 r) {
            return EqualsEstimate(l, r);
        }

        public static bool operator !=(Vector2 l, Vector2 r) {
            return !EqualsEstimate(l, r);
        }

        public static Vector2 operator -(Vector2 v) {
            return new Vector2(-v.X, -v.Y);
        }

        public static Vector2 operator +(Vector2 l, Vector2 r) {
            return new Vector2(l.X + r.X, l.Y + r.Y);
        }

        public static Vector2 operator +(Vector2 l, Size r) {
            return new Vector2(l.X + r.Width, l.Y + r.Height);
        }

        public static Vector2 operator +(Vector2 l, float v) {
            return new Vector2(l.X + v, l.Y + v);
        }

        public static Vector2 operator +(float v, Vector2 r) {
            return r + v;
        }

        public static Vector2 operator +(Vector2 l, double v) {
            return new Vector2((float) (l.X + v), (float) (l.Y + v));
        }

        public static Vector2 operator +(double v, Vector2 r) {
            return r + v;
        }

        public static Vector2 operator -(Vector2 l, Vector2 r) {
            return new Vector2(l.X - r.X, l.Y - r.Y);
        }

        public static Vector2 operator -(Vector2 l, Size r) {
            return new Vector2(l.X - r.Width, l.Y - r.Height);
        }

        public static Vector2 operator -(Vector2 l, float v) {
            return new Vector2(l.X - v, l.Y - v);
        }

        public static Vector2 operator -(float v, Vector2 r) {
            return r - v;
        }

        public static Vector2 operator -(Vector2 l, double v) {
            return new Vector2(l.X - v, l.Y - v);
        }

        public static Vector2 operator -(double v, Vector2 r) {
            return r - v;
        }

        public static Vector2 operator *(Vector2 l, Vector2 r) {
            return new Vector2(l.X * r.X, l.Y * r.Y);
        }

        public static Vector2 operator *(Vector2 l, float v) {
            return new Vector2(l.X * v, l.Y * v);
        }

        public static Vector2 operator *(float v, Vector2 r) {
            return r * v;
        }

        public static Vector2 operator *(Vector2 l, double v) {
            return new Vector2(l.X * v, l.Y * v);
        }

        public static Vector2 operator *(double v, Vector2 r) {
            return r * v;
        }

        public static Vector2 operator *(Vector2 l, Size s) {
            return new Vector2(l.X * s.Width, l.Y * s.Height);
        }

        public static Vector2 operator /(Vector2 l, Vector2 r) {
            return new Vector2(l.X / r.X, l.Y / r.Y);
        }

        public static Vector2 operator /(Vector2 l, Size s) {
            return new Vector2(l.X / s.Width, l.Y / s.Height);
        }

        public static Vector2 operator /(Vector2 l, float v) {
            return new Vector2(l.X / v, l.Y / v);
        }

        public static Vector2 operator /(float v, Vector2 r) {
            return new Vector2(v / r.X, v / r.Y);
        }

        public static Vector2 operator /(Vector2 l, double v) {
            return new Vector2(l.X / v, l.Y / v);
        }

        public static Vector2 operator /(double v, Vector2 r) {
            return new Vector2(v / r.X, v / r.Y);
        }

        #endregion Operators
    }
}
