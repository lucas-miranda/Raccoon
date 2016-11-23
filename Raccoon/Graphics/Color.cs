using System;

namespace Raccoon.Graphics {
    public struct Color {
        #region Public Static Readonly Members

        public static readonly Color White = new Color(0xFFFFFFFF);
        public static readonly Color Black = new Color(0x000000FF);
        public static readonly Color Red = new Color(0xFF0000FF);
        public static readonly Color Green = new Color(0x00FF00FF);
        public static readonly Color Blue = new Color(0x0000FFFF);
        public static readonly Color Cyan = new Color(0x00FFFFFF);
        public static readonly Color Magenta = new Color(0xFF00FFFF);
        public static readonly Color Yellow = new Color(0xFFFF00FF);

        #endregion

        #region Public Members

        public byte R, G, B, A;

        #endregion Public Members

        #region Constructors

        public Color(string hex) {
            R = G = B = A = 255;

            if (hex[0] == '#') {
                hex = hex.Substring(1);
            }

            if (hex.Length >= 3 && hex.Length <= 4) { // RGB or RGBA
                R = Convert.ToByte(string.Concat(hex[0], hex[0]), 16);
                G = Convert.ToByte(string.Concat(hex[1], hex[1]), 16);
                B = Convert.ToByte(string.Concat(hex[2], hex[2]), 16);

                if (hex.Length == 4) {
                    A = Convert.ToByte(string.Concat(hex[3], hex[3]), 16);
                }
            } else if (hex.Length >= 6 && hex.Length <= 8) { // RRGGBB or RRGGBBAA
                R = Convert.ToByte(hex.Substring(0, 2), 16);
                G = Convert.ToByte(hex.Substring(2, 2), 16);
                B = Convert.ToByte(hex.Substring(4, 2), 16);

                if (hex.Length == 8) {
                    A = Convert.ToByte(hex.Substring(6, 2), 16);
                }
            }
        }

        public Color(uint hex) : this(hex.ToString("X8")) {
        }

        public Color(byte r, byte g, byte b, byte a = 255) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(float r, float g, float b, float a = 1f) {
            R = (byte) Util.Math.Clamp(r * 255, byte.MinValue, byte.MaxValue);
            G = (byte) Util.Math.Clamp(g * 255, byte.MinValue, byte.MaxValue);
            B = (byte) Util.Math.Clamp(b * 255, byte.MinValue, byte.MaxValue);
            A = (byte) Util.Math.Clamp(a * 255, byte.MinValue, byte.MaxValue);
        }

        #endregion Constructors

        #region Public Properties

        public uint RGBA { get { return ((uint) R << 24) | ((uint) G << 16) | ((uint) B << 8) | A; } }
        public uint ARGB { get { return ((uint) A << 24) | ((uint) R << 16) | ((uint) G << 8) | B; } }
        public string Hex { get { return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A); } }

        #endregion Public Properties

        #region Public Static Methods

        public static Color FromARGB(uint hex) {
            return new Color((byte) (hex >> 16), (byte) (hex >> 8), (byte) hex, (byte) (hex >> 24));
        }

        public static Color FromARGB(byte a, byte r, byte g, byte b) {
            return new Color(r, g, b, a);
        }

        public static Color FromARGB(float a, float r, float g, float b) {
            return new Color(r, g, b, a);
        }

        public static Color Lerp(Color start, Color end, float t) {
            return new Color(Util.Math.Lerp(start.R, end.R, t), Util.Math.Lerp(start.G, end.G, t), Util.Math.Lerp(start.B, end.B, t), Util.Math.Lerp(start.A, end.A, t));
        }

        #endregion Public Static Methods

        #region Public Methods

        public override bool Equals(object obj) {
            return obj is Color && Equals((Color) obj);
        }

        public bool Equals(Color c) {
            return this == c;
        }

        public override int GetHashCode() {
            return (R ^ G) + (B ^ A);
        }

        public override string ToString() {
            return $"[Color | RGBA: {R} {G} {B} {A}]";
        }

        #endregion Public Methods

        #region Implicit Conversions

        public static implicit operator Microsoft.Xna.Framework.Color(Color c) {
            return new Microsoft.Xna.Framework.Color(c.R, c.G, c.B, c.A);
        }

        #endregion Implicit Conversions

        #region Operators

        public static bool operator ==(Color l, Color r) {
            return l.R == r.R && l.G == r.G && l.B == r.B && l.A == r.A;
        }

        public static bool operator !=(Color l, Color r) {
            return !(l == r);
        }

        public static Color operator +(Color l, Color r) {
            return new Color((byte) (l.R + r.R), (byte) (l.G + r.G), (byte) (l.B + r.B), (byte) (l.A + r.A));
        }

        public static Color operator -(Color l, Color r) {
            return new Color((byte) (l.R - r.R), (byte) (l.G - r.G), (byte) (l.B - r.B), (byte) (l.A - r.A));
        }

        public static Color operator *(Color l, float n) {
            return new Color((byte) (l.R * n), (byte) (l.G * n), (byte) (l.B * n), (byte) (l.A * n));
        }

        public static Color operator /(Color l, float n) {
            return new Color((byte) (l.R / n), (byte) (l.G / n), (byte) (l.B / n), (byte) (l.A / n));
        }

        #endregion Operators
    }
}
