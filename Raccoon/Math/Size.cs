namespace Raccoon {
    public struct Size {
        #region Static Readonly

        public static readonly Size Empty = new Size(0);

        #endregion Static Readonly  

        #region Public Members

        public float Width, Height;

        #endregion Public Members

        #region Constructors

        public Size(float w, float h) {
            Width = System.Math.Abs(w);
            Height = System.Math.Abs(h);
        }

        public Size(float wh) : this(wh, wh) { }

        public Size(Vector2 v) : this(v.X, v.Y) { }

        #endregion Constructors

        #region Public Properties

        public float Area { get { return Width * Height; } }
        public bool IsEmpty { get { return Width == 0 && Height == 0; } }

        #endregion Public Properties

        #region Public Methods

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

        public override int GetHashCode() {
            return Width.GetHashCode() + Height.GetHashCode();
        }

        public Rectangle ToRectangle() {
            return new Rectangle(0, 0, Width, Height);
        }

        public Vector2 ToVector2() {
            return new Vector2(Width, Height);
        }

        public override string ToString() {
            return $"[Size | Width: {Width}, Height: {Height}]";
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

        public static Size operator -(Size l, Size r) {
            return new Size(l.Width - r.Width, l.Height - r.Height);
        }

        public static Size operator *(Size l, Size r) {
            return new Size(l.Width * r.Width, l.Height * r.Height);
        }

        public static Size operator *(Size l, float n) {
            return new Size(l.Width * n, l.Height * n);
        }

        public static Size operator *(Size l, Vector2 v) {
            return new Size(l.Width * v.X, l.Height * v.Y);
        }

        public static Size operator /(Size l, Size r) {
            return new Size(l.Width / r.Width, l.Height / r.Height);
        }

        public static Size operator /(Size l, float n) {
            return new Size(l.Width / n, l.Height / n);
        }

        public static Size operator /(Size l, Vector2 v) {
            return new Size(l.Width / v.X, l.Height / v.Y);
        }

        #endregion Operators
    }
}
