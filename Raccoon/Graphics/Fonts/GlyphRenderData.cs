namespace Raccoon.Fonts {
    internal class GlyphRenderData {
        #region Constructors

        public GlyphRenderData(char c, float advanceX, float horizontalBearingX) {
            Char = c;
            AdvanceX = advanceX;
            HorizontalBearingX = horizontalBearingX;
        }

        #endregion Constructors

        #region Public Methods

        public Vector2 Position { get; set; }
        public Size Size { get; set; }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }

        public System.Drawing.Bitmap Bitmap { get; set; }

#if DEBUG

        public char Char { get; set; }
        public float AdvanceX { get; set; }
        public float HorizontalBearingX { get; set; }
        public float Underrun { get; set; }
        public float Overrun { get; set; }
        public float RightEdge { get; set; }
        public float Kern { get; set; }

        #endif

        #endregion Public Methods

        #region Public Methods

        public static void PrintHeader() {
            System.Diagnostics.Debug.Print(
                "    {1,5} {2,5} {3,5} {4,5} {5,5} {6,5} {7,5}",
                "", "adv", "bearing", "wid", "undrn", "ovrrn", "kern", "redge"
            );
        }

        public override string ToString() {
            return string.Format(
                       "'{0}' {1,5:F0} {2,5:F0} {3,5:F0} {4,5:F0} {5,5:F0} {6,5:F0} {7,5:F0}",
                       Char, AdvanceX, HorizontalBearingX, Width, Underrun, Overrun, Kern, RightEdge
                   );
        }

        #endregion Public Methods
    }
}
