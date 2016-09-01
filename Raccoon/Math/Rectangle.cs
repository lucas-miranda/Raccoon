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

        public Rectangle(Point topLeft, Size size) : this(topLeft.X, topLeft.Y, size.Width, size.Height) {
        }

        public Rectangle(Point topLeft, Point bottomRight) : this(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y) {
        }

        public Rectangle(float w, float h) {
            X = Y = 0;
            Width = w;
            Height = h;
        }

        #endregion Constructors

        #region Public Properties

        public float Area { get { return System.Math.Abs(Width * Height); } }
        public Point Position { get { return new Point(X, Y); } set { X = value.X; Y = value.Y; } }
        public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }
        public float Left { get { return X; } set { X = value; } }
        public float Top { get { return Y; } set { Y = value; } }
        public float Right { get { return X + Width; } set { Width = value - X; } }
        public float Bottom { get { return Y + Height; } set { Height = value - Y; } }
        public Point Center { get { return new Point(X + Width / 2, Y + Height / 2); } }
        public bool IsEmpty { get { return Width == 0 && Height == 0; } }

        #endregion Public Properties

        #region Public Methods

        public bool Contains(Point p) {
            return !(p.X < Left || p.X > Right || p.Y < Top || p.Y > Bottom);
        }

        public bool Intersects(Rectangle r) {
            return !(r.Right < Left || r.Left > Right || r.Top < Top || r.Bottom > Bottom);
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

        public override int GetHashCode() {
            return (X.GetHashCode() + Y.GetHashCode()) ^ (Width.GetHashCode() + Height.GetHashCode());
        }

        public override string ToString() {
            return $"[Rectangle | X: {X}, Y: {Y}, Width: {Width}, Height: {Height}]";
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

        public static bool operator &(Rectangle l, Point r) {
            return l.Contains(r);
        }

        public static bool operator &(Rectangle l, Rectangle r) {
            return l.Intersects(r);
        }

        #endregion Operators
    }
}