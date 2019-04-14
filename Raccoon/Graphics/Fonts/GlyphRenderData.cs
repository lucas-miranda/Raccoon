namespace Raccoon.Fonts {
    public class GlyphRenderData {
        public Vector2 Position { get; set; }
        public Size Size { get; set; }
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public float Width { get { return Size.Width; } }
        public float Height { get { return Size.Height; } }

        public System.Drawing.Bitmap Bitmap { get; set; }

#if DEBUG

        public float Underrun { get; set; }
        public float Overrun { get; set; }

        #endif
    }
}
