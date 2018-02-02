namespace Raccoon {
    public struct Rectangle {
        #region Static Readonly

        public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);

        #endregion Static Readonly          

        #region Public Members

        public float X, Y, Width, Height;

        #endregion Public Members

        #region Constructors

        public Rectangle(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2 topLeft, Size size) : this(topLeft.X, topLeft.Y, size.Width, size.Height) { }
        public Rectangle(Vector2 topLeft, Vector2 bottomRight) : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y) { }

        public Rectangle(float w, float h) {
            X = Y = 0;
            Width = w;
            Height = h;
        }

        public Rectangle(Size size) : this(size.Width, size.Height) { }

        #endregion Constructors

        #region Public Properties

        public float Area { get { return System.Math.Abs(Width * Height); } }
        public Vector2 Position { get { return new Vector2(X, Y); } set { X = value.X; Y = value.Y; } }
        public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }
        public float Left { get { return X; } set { Width = Right - value; X = value; } }
        public float Top { get { return Y; } set { Height = Bottom - value; Y = value; } }
        public float Right { get { return X + Width; } set { Width = value - X; } }
        public float Bottom { get { return Y + Height; } set { Height = value - Y; } }
        public Vector2 Center { get { return new Vector2(X + Width / 2f, Y + Height / 2f); } }
        public bool IsEmpty { get { return (int) Width == 0 && (int) Height == 0; } }
        public Vector2 TopLeft { get { return new Vector2(Left, Top); } set { Left = value.X; Top = value.Y; } }
        public Vector2 TopRight { get { return new Vector2(Right, Top); } set { Right = value.X; Top = value.Y; } }
        public Vector2 LeftRight { get { return new Vector2(Left, Bottom); } set { Left = value.X; Bottom = value.Y; } }
        public Vector2 BottomLeft { get { return new Vector2(Left, Bottom); } set { Left = value.X; Bottom = value.Y; } }
        public Vector2 BottomRight { get { return new Vector2(Right, Bottom); } set { Right = value.X; Bottom = value.Y; } }

        #endregion Public Properties

        #region Static Public Methods

        public static Rectangle Union(Rectangle a, Rectangle b) {
            return new Rectangle(
                new Vector2(System.Math.Min(a.Left, b.Left), System.Math.Min(a.Top, b.Top)),
                new Vector2(System.Math.Max(a.Right, b.Right), System.Math.Max(a.Bottom, b.Bottom))
            );
        }

        public static bool Intersect(Rectangle a, Rectangle b) {
            return a.Intersects(b);
        }

        #endregion Static Public Methods

        #region Public Methods

        public bool Contains(Vector2 v) {
            return !(v.X <= Left || v.X >= Right || v.Y <= Top || v.Y >= Bottom);
        }

        public bool ContainsInclusive(Vector2 v) {
            return !(v.X < Left || v.X >= Right || v.Y < Top || v.Y >= Bottom);
        }

        public bool Intersects(Rectangle r) {
            return !(r.Right <= Left || r.Left >= Right || r.Bottom <= Top || r.Top >= Bottom);
        }

        public Vector2 ClosestPoint(Vector2 point) {
            return Util.Math.Clamp(point, this);
        }

        public void Inflate(float w, float h) {
            Width += w;
            Height += h;
        }

        public void Deflate(float w, float h) {
            Width -= w;
            Height -= h;
        }
        
        public override bool Equals(object obj) {
            return obj is Rectangle && Equals((Rectangle) obj);
        }

        public bool Equals(Rectangle r) {
            return this == r;
        }
        
        public override string ToString() {
            return $"[{X} {Y} {Width} {Height}]";
        }

        public override int GetHashCode() {
            var hashCode = 466501756;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        #endregion Public Methods

        #region Implicit Conversions

        public static implicit operator Microsoft.Xna.Framework.Rectangle(Rectangle r) {
            return new Microsoft.Xna.Framework.Rectangle((int) r.X, (int) r.Y, (int) r.Width, (int) r.Height);
        }

        #endregion Implicit Conversions

        #region Operators

        public static bool operator ==(Rectangle l, Rectangle r) {
            return l.X == r.X && l.Y == r.Y && l.Width == r.Width && l.Height == r.Height;
        }

        public static bool operator !=(Rectangle l, Rectangle r) {
            return !(l == r);
        }

        public static bool operator <(Rectangle l, Rectangle r) {
            return l.Area < r.Area;
        }

        public static bool operator >(Rectangle l, Rectangle r) {
            return l.Area > r.Area;
        }

        public static bool operator <=(Rectangle l, Rectangle r) {
            return !(l > r);
        }

        public static bool operator >=(Rectangle l, Rectangle r) {
            return !(l < r);
        }

        public static bool operator &(Rectangle l, Vector2 r) {
            return l.Contains(r);
        }

        public static bool operator &(Rectangle l, Rectangle r) {
            return l.Intersects(r);
        }

        public static Rectangle operator |(Rectangle l, Rectangle r) {
            return Union(l, r);
        }

        public static Rectangle operator +(Rectangle l, Vector2 r) {
            return new Rectangle(l.X + r.X, l.Y + r.Y, l.Width, l.Height);
        }

        public static Rectangle operator +(Vector2 l, Rectangle r) {
            return r + l;
        }

        public static Rectangle operator +(Rectangle l, Size r) {
            return new Rectangle(l.X, l.Y, l.Width + r.Width, l.Height + r.Height);
        }

        public static Rectangle operator -(Rectangle l, Vector2 r) {
            return l + (-r);
        }

        public static Rectangle operator -(Vector2 l, Rectangle r) {
            return r - l;
        }

        public static Rectangle operator -(Rectangle l, Size r) {
            return new Rectangle(l.X, l.Y, l.Width - r.Width, l.Height - r.Height);
        }

        public static Rectangle operator *(Rectangle l, Size r) {
            return new Rectangle(l.X, l.Y, l.Width * r.Width, l.Height * r.Height);
        }

        public static Rectangle operator *(Rectangle l, float v) {
            return new Rectangle(l.X, l.Y, l.Width * v, l.Height * v);
        }

        public static Rectangle operator *(float v, Rectangle r) {
            return r * v;
        }

        public static Rectangle operator /(Rectangle l, Size r) {
            return new Rectangle(l.X, l.Y, l.Width / r.Width, l.Height / r.Height);
        }

        public static Rectangle operator /(Rectangle l, float v) {
            return new Rectangle(l.X, l.Y, l.Width / v, l.Height / v);
        }

        #endregion Operators
    }
}