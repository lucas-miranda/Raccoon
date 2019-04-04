using System.Text.RegularExpressions;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public struct Color {
        #region Private Static Readonly Members

        private static readonly Regex ColorFormatRegex = new Regex(@"^\#?((([a-fA-F0-9]){3,4})|(([a-fA-F0-9]{2}){3,4}))$");

        #endregion Private Static Readonly Members

        #region Public Static Readonly Members

        public static readonly Color Transparent = new Color(0x00000000);
        public static readonly Color White = new Color(0xFFFFFFFF);
        public static readonly Color Black = new Color(0x000000FF);
        public static readonly Color Red = new Color(0xFF0000FF);
        public static readonly Color Green = new Color(0x00FF00FF);
        public static readonly Color Blue = new Color(0x0000FFFF);
        public static readonly Color Cyan = new Color(0x00FFFFFF);
        public static readonly Color Magenta = new Color(0xFF00FFFF);
        public static readonly Color Yellow = new Color(0xFFFF00FF);
        public static readonly Color Orange = new Color(0xFF8000FF);
        public static readonly Color Violet = new Color(0x7F00FFFF);
        public static readonly Color Indigo = new Color(0x4B0082FF);
        public static readonly Color Purple = new Color(0x800080FF);
        public static readonly Color Lime = new Color(0xBFFF00FF);
        public static readonly Color Olive = new Color(0xD9E778FF);
        public static readonly Color Teal = new Color(0x6DFFFF);
        public static readonly Color Pink = new Color(0xFF8EDFFF);
        public static readonly Color Brown = new Color(0xD67C1CFF);
        
        #endregion

        #region Public Members

        public byte R, G, B, A;

        #endregion Public Members

        #region Constructors

        public Color(string rgba) {
            R = G = B = A = byte.MaxValue;

            if (rgba[0] == '#') {
                rgba = rgba.Substring(1);
            }

            if (rgba.Length >= 3 && rgba.Length <= 4) { // RGB or RGBA
                R = System.Convert.ToByte(string.Concat(rgba[0], rgba[0]), 16);
                G = System.Convert.ToByte(string.Concat(rgba[1], rgba[1]), 16);
                B = System.Convert.ToByte(string.Concat(rgba[2], rgba[2]), 16);

                if (rgba.Length == 4) {
                    A = System.Convert.ToByte(string.Concat(rgba[3], rgba[3]), 16);
                }
            } else if (rgba.Length >= 6 && rgba.Length <= 8) { // RRGGBB or RRGGBBAA
                R = System.Convert.ToByte(rgba.Substring(0, 2), 16);
                G = System.Convert.ToByte(rgba.Substring(2, 2), 16);
                B = System.Convert.ToByte(rgba.Substring(4, 2), 16);

                if (rgba.Length == 8) {
                    A = System.Convert.ToByte(rgba.Substring(6, 2), 16);
                }
            }
        }

        public Color(uint rgba) : this(rgba.ToString("X8")) { }

        public Color(byte r, byte g, byte b, byte a = byte.MaxValue) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(float r, float g, float b, float a = 1f) {
            R = (byte) Math.Clamp(r * byte.MaxValue, 0, 255);
            G = (byte) Math.Clamp(g * byte.MaxValue, 0, 255);
            B = (byte) Math.Clamp(b * byte.MaxValue, 0, 255);
            A = (byte) Math.Clamp(a * byte.MaxValue, 0, 255);
        }

        public Color(Color color, float alpha) : this(color.R, color.G, color.B, (byte) Math.Clamp(alpha * byte.MaxValue, 0, 255)) {
        }

        public Color(Color color, byte alpha) : this(color.R, color.G, color.B, alpha) {
        }

        #endregion Constructors

        #region Public Properties

        public uint RGBA { get { return ((uint) R << 24) | ((uint) G << 16) | ((uint) B << 8) | A; } }
        public uint ARGB { get { return ((uint) A << 24) | ((uint) R << 16) | ((uint) G << 8) | B; } }

        public float[] Normalized {
            get {
                return new float[4] {
                    R / 255f, 
                    G / 255f, 
                    B / 255f, 
                    A / 255f
                };
            }
        }

        #endregion Public Properties

        #region Public Static Methods

        public static bool TryParse(string value, out Color result) {
            result = new Color(0, 0, 0, 0);
            if (!ColorFormatRegex.IsMatch(value)) {
                return false;
            }

            result = new Color(value);
            return true;
        }

        public static Color Parse(string value) {
            return new Color(value);
        }

        public static Color FromARGB(uint rgba) {
            return new Color((byte) (rgba >> 16), (byte) (rgba >> 8), (byte) rgba, (byte) (rgba >> 24));
        }

        public static Color FromARGB(byte a, byte r, byte g, byte b) {
            return new Color(r, g, b, a);
        }

        public static Color FromARGB(float a, float r, float g, float b) {
            return new Color(r, g, b, a);
        }

        public static Color Lerp(Color start, Color end, float t) {
            return new Color(
                Math.Lerp(start.R / 255f, end.R / 255f, t), 
                Math.Lerp(start.G / 255f, end.G / 255f, t), 
                Math.Lerp(start.B / 255f, end.B / 255f, t), 
                Math.Lerp(start.A / 255f, end.A / 255f, t)
            );
        }

        public static Color Darken(Color color, float amount) {
            amount = 1f - Math.Clamp(amount, 0f, 1f);
            return new Color(
                (byte) Math.Clamp(color.R * amount, 0, 255),
                (byte) Math.Clamp(color.G * amount, 0, 255),
                (byte) Math.Clamp(color.B * amount, 0, 255),
                color.A
            );
        }

        public static Color Lighten(Color color, float amount) {
            Color diff = White - color;
            diff *= amount;
            return new Color(color + diff, color.A);
        }

        public static Color Complementary(Color color) {
            float[] hsl = ToHSL(color);
            return FromHSL((hsl[0] + .5f) % 1f, hsl[1], hsl[2]);
        }

        public static float[] ToHSL(Color color) {
            float r = color.R / 255f,
                  g = color.G / 255f,
                  b = color.B / 255f;

            float max = Math.Max(Math.Max(r, g), b);
            float min = Math.Min(Math.Min(r, g), b);

            float h, s, l;
            h = s = l = (max + min) / 2f;

            if (max == min) {
                h = s = 0f;
            } else {
                float d = max - min;
                s = l > .5f ? d / (2f - (max + min)) : d / (max + min);

                if (r > g && r > b) {
                    h = (g - b) / d + (g < b ? 6f : 0f);
                } else if (g > b) {
                    h = (b - r) / d + 2f;
                } else {
                    h = (r - g) / d + 4f;
                }

                h /= 6f;
            }

            return new float[] { h, s, l };
        }

        public static Color FromHSL(float h, float s, float l) {
            float r, g, b;
            r = g = b = 0f;

            if (s == 0f) {
                r = g = b = l;
            } else {
                float y = l < .5f ? l * (1f + s) : l + s - l * s;
                float x = 2f * l - y;

                r = HUEToRGB(x, y, h + 1f / 3f);
                g = HUEToRGB(x, y, h);
                b = HUEToRGB(x, y, h - 1f / 3f);
            }

            return new Color(r, g, b);

            float HUEToRGB(float p, float q, float t) {
                if (t < 0f) {
                    t += 1f;
                }

                if (t > 1f) {
                    t -= 1f;
                }

                if (t < 1f/6f) {
                    return p + (q - p) * 6f * t;
                }

                if (t < 1f/2f) {
                    return q;
                }

                if (t < 2f/3f) {
                    return p + (q - p) * (2f / 3f - t) * 6f;
                }

                return p;
            }
        }

        public static Color FromHSL(float[] hsl) {
            return FromHSL(hsl[0], hsl[1], hsl[2]);
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
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A);
        }

        #endregion Public Methods

        #region Implicit Conversions

        public static implicit operator Microsoft.Xna.Framework.Color(Color c) {
            return new Microsoft.Xna.Framework.Color(c.R, c.G, c.B, c.A);
        }

        public static implicit operator Color(uint rgba) {
            return new Color(rgba);
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

        public static Color operator *(Color l, Color r) {
            float[] lNormalized = l.Normalized,
                    rNormalized = r.Normalized;
            return new Color(
                lNormalized[0] * rNormalized[0], 
                lNormalized[1] * rNormalized[1], 
                lNormalized[2] * rNormalized[2], 
                lNormalized[3] * rNormalized[3]
            );
        }

        public static Color operator *(Color l, float n) {
            float[] normalized = l.Normalized;

            return new Color(
                normalized[0] * n,
                normalized[1] * n,
                normalized[2] * n,
                normalized[3] * n
            );
        }

        public static Color operator /(Color l, float n) {
            float[] normalized = l.Normalized;

            return new Color(
                normalized[0] / n,
                normalized[1] / n,
                normalized[2] / n,
                normalized[3] / n
            );
        }

        #endregion Operators
    }
}
