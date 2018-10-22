using System.Text.RegularExpressions;

namespace Raccoon {
    public struct Size {
        #region Private Members

        private static readonly Regex StringFormatRegex = new Regex(@"(\-?\d+(?:\.?\d+)?)(?: *, *| +)(\-?\d+(?:\.?\d+)?)");

        #endregion Private Members

        #region Public Members

        public static readonly Size Empty = new Size(0);
        public static readonly Size One = new Size(1);

        public float Width, Height;

        #endregion Public Members

        #region Constructors

        public Size(float w, float h) {
            Width = System.Math.Abs(w);
            Height = System.Math.Abs(h);
        }

        public Size(float wh) : this(wh, wh) {
        }

        public Size(Vector2 v) : this(v.X, v.Y) {
        }

        #endregion Constructors

        #region Public Properties

        public float Area { get { return Width * Height; } }
        public bool IsEmpty { get { return (int) Width == 0 && (int) Height == 0; } }

        #endregion Public Properties

        #region Public Methods

        public static Size Parse(string value) {
            MatchCollection matches = StringFormatRegex.Matches(value);

            if (matches.Count == 0 || !matches[0].Success) {
                throw new System.FormatException($"String '{value}' doesn't not typify a Size.");
            }

            return new Size(
                float.Parse(matches[0].Groups[1].Value),
                float.Parse(matches[0].Groups[2].Value)
            );
        }

        public static bool TryParse(string value, out Size result) {
            MatchCollection matches = StringFormatRegex.Matches(value);

            if (matches.Count == 0 || !matches[0].Success) {
                result = Empty;
                return false;
            }

            result = new Size(
                float.Parse(matches[0].Groups[1].Value),
                float.Parse(matches[0].Groups[2].Value)
            );

            return true;
        }

        public void Inflate(float w, float h) {
            Width = System.Math.Abs(Width + w);
            Height = System.Math.Abs(Height + h);
        }

        public void Deflate(float w, float h) {
            Width = System.Math.Abs(Width - w);
            Height = System.Math.Abs(Height - h);
        }

        public override bool Equals(object obj) {
            return obj is Size && Equals((Size) obj);
        }

        public bool Equals(Size s) {
            return this == s;
        }

        public Rectangle ToRectangle() {
            return new Rectangle(0, 0, Width, Height);
        }

        public Vector2 ToVector2() {
            return new Vector2(Width, Height);
        }

        public override string ToString() {
            return $"[{Width} {Height}]";
        }

        public override int GetHashCode() {
            var hashCode = 859600377;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        #endregion Public Methods

        #region Operators

        public static bool operator ==(Size l, Size r) {
            return l.Width == r.Width && l.Height == r.Height;
        }

        public static bool operator !=(Size l, Size r) {
            return !(l == r);
        }

        public static bool operator <(Size l, Size r) {
            return l.Area < r.Area;
        }

        public static bool operator >(Size l, Size r) {
            return l.Area > r.Area;
        }

        public static bool operator <=(Size l, Size r) {
            return !(l > r);
        }

        public static bool operator >=(Size l, Size r) {
            return !(l < r);
        }

        public static Size operator +(Size l, Size r) {
            return new Size(l.Width + r.Width, l.Height + r.Height);
        }

        public static Size operator +(Size l, Rectangle r) {
            return new Size(l.Width + r.Width, l.Height + r.Height);
        }

        public static Size operator -(Size l, Size r) {
            return new Size(l.Width - r.Width, l.Height - r.Height);
        }

        public static Size operator -(Size l, Rectangle r) {
            return new Size(l.Width - r.Width, l.Height - r.Height);
        }

        public static Size operator *(Size l, Size r) {
            return new Size(l.Width * r.Width, l.Height * r.Height);
        }

        public static Size operator *(Size l, float v) {
            return new Size(l.Width * v, l.Height * v);
        }

        public static Size operator *(float v, Size r) {
            return r * v;
        }

        public static Size operator *(Size l, Vector2 r) {
            return new Size(l.Width * r.X, l.Height * r.Y);
        }

        public static Size operator *(Size l, Rectangle r) {
            return new Size(l.Width * r.Width, l.Height * r.Height);
        }

        public static Size operator /(Size l, Size r) {
            return new Size(l.Width / r.Width, l.Height / r.Height);
        }

        public static Size operator /(Size l, float v) {
            return new Size(l.Width / v, l.Height / v);
        }

        public static Size operator /(float v, Size r) {
            return r * v;
        }

        public static Size operator /(Size l, Vector2 r) {
            return new Size(l.Width / r.X, l.Height / r.Y);
        }

        public static Size operator /(Size l, Rectangle r) {
            return new Size(l.Width / r.Width, l.Height / r.Height);
        }

        #endregion Operators
    }
}
