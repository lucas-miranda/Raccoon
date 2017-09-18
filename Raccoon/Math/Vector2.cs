﻿using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon {
    public struct Vector2 : System.IEquatable<Vector2> {
        #region Static Readonly

        public static readonly Vector2 Zero = new Vector2(0, 0);
        public static readonly Vector2 One = new Vector2(1, 1);
        public static readonly Vector2 Up = new Vector2(0, -1);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Down = new Vector2(0, 1);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 UpLeft = new Vector2(-1, -1);
        public static readonly Vector2 UpRight = new Vector2(-1, 1);
        public static readonly Vector2 DownRight = new Vector2(1, 1);
        public static readonly Vector2 DownLeft = new Vector2(1, -1);
        public static readonly Vector2 UnitX = new Vector2(1, 0);
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        #endregion Static Readonly          

        #region Public Members

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
            return a.Dot(b);
        }

        public static float Cross(Vector2 a, Vector2 b) {
            return a.Cross(b);
        }

        public static float Cross(Vector2 a, Vector2 b, Vector2 c) {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        public static Vector2 Normalize(Vector2 v) {
            if ((int) v.X == 0 && (int) v.Y == 0) throw new System.ArgumentException("Both X and Y values can't be zero.", "v");

            return v * (1f / v.Length());
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

        public Vector2 Perpendicular() {
            return new Vector2(-Y, X);
        }

        public float Dot(Vector2 other) {
            return X * other.X + Y * other.Y;
        }

        public float Cross(Vector2 other) {
            return X * other.Y - Y * other.X;
        }

        public Range Projection(ICollection<Vector2> points) {
            if (points.Count == 0) throw new System.ArgumentException("Projection needs at least one value.", "points");

            IEnumerator<Vector2> enumerator = points.GetEnumerator();
            enumerator.MoveNext();

            float min = Dot(enumerator.Current),
                  max = min;

            while (enumerator.MoveNext()) {
                float p = Dot(enumerator.Current);
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

        public override bool Equals(object obj) {
            return obj is Vector2 && Equals((Vector2) obj);
        }

        public bool Equals(Vector2 other) {
            return this == other;
        }

        public Direction ToDirection() {
            Direction dir = Direction.None;
            if (X > 0)
                dir |= Direction.Right;
            else if (X < 0)
                dir |= Direction.Left;

            if (Y > 0)
                dir |= Direction.Up;
            else if (Y < 0)
                dir |= Direction.Down;

            return dir;
        }

        public override string ToString() {
            return $"[{X} {Y}]";
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
            return l.X == r.X && l.Y == r.Y;
        }

        public static bool operator !=(Vector2 l, Vector2 r) {
            return !(l == r);
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
            return l + (-r);
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
            return new Vector2((float) (l.X - v), (float) (l.Y - v));
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
            return new Vector2((float) (l.X * v), (float)  (l.Y * v));
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
            return r / v;
        }

        public static Vector2 operator /(Vector2 l, double v) {
            return new Vector2((float) (l.X / v), (float) (l.Y / v));
        }

        public static Vector2 operator /(double v, Vector2 r) {
            return r / v;
        }

        #endregion Operators
    }
}
