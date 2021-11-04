using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Util;

namespace Raccoon.Fonts {
    public abstract class FontRenderMap : System.IDisposable {
        #region Constructors

        public FontRenderMap() {
        }

        #endregion Constructors

        #region Public Properties

        public Texture Texture { get; protected set; }
        public float LineHeight { get; protected set; }
        public float Ascender { get; protected set; }
        public float Descender { get; protected set; }
        public float UnderlinePosition { get; protected set; }
        public float UnderlineThickness { get; protected set; }
        public float NominalWidth { get; set; }
        public float NominalHeight { get; set; }
        public abstract bool HasKerning { get; }
        public uint DefaultErrorCharacter { get; set; } = '?';
        public Size GlyphSlotSize { get { return new Size((float) NominalWidth, (float) NominalHeight); } }
        public Dictionary<uint, Glyph> Glyphs { get; } = new Dictionary<uint, Glyph>();

        /// <summary>
        /// Default shader to be applied when using this map.
        /// </summary>
        public abstract Shader Shader { get; }

        /// <summary>
        /// Font size (in pixels/em).
        /// </summary>
        public float Size { get { return NominalWidth; } }

        public bool IsDisposed { get; private set; }

        #endregion Public Properties

        #region Internal Properties

        internal uint References { get; private set; } = 1;

        #endregion Internal Properties

        #region Public Methods

        public virtual void Setup(float size) {
        }

        public virtual Text.RenderData PrepareTextRenderData(string text, out double textEmWidth, out double textEmHeight) {
            return BuildText(text, out textEmWidth, out textEmHeight);
        }

        public abstract void Reload();

        public abstract IShaderParameters CreateShaderParameters();

        public override string ToString() {
            return $"Line Height: {LineHeight}, Ascender: {Ascender}, Descender: {Descender}, UnderlinePosition: {UnderlinePosition}, UnderlineThickness: {UnderlineThickness}, NominalWidth: {NominalWidth}, NominalHeight: {NominalHeight}, HasKerning? {HasKerning.ToPrettyString()}, DefaultErrorCharacter: {DefaultErrorCharacter}, GlyphSlotSize: {GlyphSlotSize}, Glyphs: {Glyphs.Count}";
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            References -= 1;
            if (References > 0) {
                return;
            }

            IsDisposed = true;

            Disposed();
        }

        #endregion Public Methods

        #region Protected Methods

        protected abstract double Kerning(uint leftGlyph, uint rightGlyph);

        protected Glyph RegisterGlyph(
            uint charCode,
            Rectangle sourceArea,
            double bearingX,
            double bearingY,
            double width,
            double height,
            double advanceX,
            double advanceY
        ) {
            Glyph glyph = new Glyph(
                sourceArea,
                bearingX,
                bearingY,
                width,
                height,
                advanceX,
                advanceY
            );

            Glyphs.Add(charCode, glyph);
            return glyph;
        }

        protected double ConvertEmToPx(double em, double fontTargetSize) {
            return em * fontTargetSize;
        }

        protected float ConvertEmToPx(float em, float fontTargetSize) {
            return em * fontTargetSize;
        }

        protected double ConvertPxToEm(double px, double fontTargetSize) {
            return px / fontTargetSize;
        }

        protected float ConvertPxToEm(float px, float fontTargetSize) {
            return px / fontTargetSize;
        }

        protected abstract void Disposed();

        #endregion Protected Methods

        #region Private Methods

        private Text.RenderData BuildText(string text, out double textEmWidth, out double textEmHeight) {
            int extraSpace = text.Count("\t") * Math.Max(0, Font.TabulationWhitespacesSize - 1);
            Text.RenderData textRenderData = new Text.RenderData(text.Length + extraSpace);
            textEmWidth = textEmHeight = 0.0;

            double kern,
                   penX = 0f,
                   penY = Math.Abs(Ascender);

            bool isEndOfLine = false;

            int renderTimes;

            // TODO: add support to unicode characters?
            for (int i = 0; i < text.Length; i++) {
                char charCode = text[i];

                if (charCode == '\n') { // new line
                    penY += LineHeight;
                    continue;
                } else if (charCode == '\r') { // carriage return
                    // do nothing, just ignore
                    // TODO: maybe add an option to detect when carriage return handling is needed (MAC OS 9 or older, maybe?)
                    continue;
                } else if (i + 1 == text.Length
                    || text[i + 1] == '\n'
                    || (i + 2 < text.Length && text[i + 1] == '\r' && text[i + 2] == '\n')
                ) {
                    isEndOfLine = true;
                }

                if (charCode == '\t') { // tabulation
                    // will render a tabulation representation using whitespaces
                    charCode = ' ';
                    renderTimes = Font.TabulationWhitespacesSize;
                } else {
                    renderTimes = 1;
                }

                if (!Glyphs.TryGetValue(charCode, out Glyph glyph)) {
                    // glyph not found, just render default symbol
                    glyph = Glyphs[DefaultErrorCharacter];
                }

                for (int j = 0; j < renderTimes; j++) {
                    #region Underrun

                    if (penX == 0.0) {
                        penX -= glyph.BearingX;
                    }

                    #endregion Underrun

                    textRenderData.AppendGlyph(
                        penX + glyph.BearingX,
                        penY + glyph.BearingY,
                        glyph.SourceArea,
                        charCode,
                        glyph
                    );

                    penX += glyph.AdvanceX;
                    penY += glyph.AdvanceY;

                    #region Kerning with Next Repeated Character

                    // Adjust for kerning between this character and the next (if repeatTimes > 1)
                    if (HasKerning && !isEndOfLine && renderTimes > 1) {
                        kern = Kerning(charCode, charCode);

                        if (Math.Abs(kern) > glyph.AdvanceX * 5.0) {
                            kern = 0;
                        }

                        penX += kern;
                    }

                    #endregion Kerning with Next Repeated Character
                }

                #region Kerning with Next Character

                // Adjust for kerning between this character and the next.
                if (HasKerning && !isEndOfLine) {
                    char nextCharCode = text[i + 1];
                    kern = Kerning(charCode, nextCharCode);

                    if (Math.Abs(kern) > glyph.AdvanceX * 5.0) {
                        kern = 0;
                    }

                    penX += kern;
                }

                #endregion Kerning with Next Character

                if (isEndOfLine) {
                    isEndOfLine = false;

                    if (penX > textEmWidth) {
                        textEmWidth = penX;
                    }

                    penX = 0.0;
                }
            }

            textEmHeight = penY + Descender;
            return textRenderData;
        }

        #endregion Private Methods

        #region Internal Methods

        internal void AddReferenceCount() {
            if (IsDisposed) {
                return;
            }

            References += 1;
        }

        #endregion Internal Methods

        #region Class Glyph

        public class Glyph {
            public Glyph(
                Rectangle sourceArea,
                double bearingX,
                double bearingY,
                double width,
                double height,
                double advanceX,
                double advanceY
            ) {
                SourceArea = sourceArea;
                BearingX = bearingX;
                BearingY = bearingY;
                Width = width;
                Height = height;
                AdvanceX = advanceX;
                AdvanceY = advanceY;
            }

            public Rectangle SourceArea { get; }
            public double BearingX { get; }
            public double BearingY { get; }
            public double Width { get; }
            public double Height { get; }
            public double AdvanceX { get; }
            public double AdvanceY { get; }

            public override string ToString() {
                return $"Source Area: {SourceArea}, BearingX: {BearingX}, BearingY: {BearingY}, Width: {Width}, Height: {Height}, AdvanceX: {AdvanceX}, AdvanceY: {AdvanceY}";
            }
        }

        #endregion Class Glyph
    }
}
