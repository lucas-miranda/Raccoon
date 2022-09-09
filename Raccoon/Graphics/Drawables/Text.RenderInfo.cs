using System.Collections.Generic;

namespace Raccoon.Graphics {
    public partial class Text {
        #region RenderInfo Class

        public class RenderData {
            #region Private Members

            private readonly Glyph[] _glyphs;

            #endregion Private Members

            #region Constructors

            public RenderData(int glyphCount) {
                //NominalSize = nominalSize;
                _glyphs = new Glyph[glyphCount];
            }

            #endregion Constructors

            #region Public Properties

            //public float FontSize { get { return NominalSize * Scale; } }
            //public float NominalSize { get; }
            public int GlyphCount { get; private set; }
            public int Capacity { get { return _glyphs.Length; } }
            //public float Scale { get; set; } = 1.0f;

            public Glyph this[int index] {
                get {
                    if (index < 0 || index >= GlyphCount) {
                        if (GlyphCount == 0) {
                            throw new System.ArgumentOutOfRangeException(
                                nameof(index),
                                $"Glyphs is empty"
                            );
                        }

                        throw new System.ArgumentOutOfRangeException(
                            nameof(index),
                            $"Supplied index {index} is out of valid range [0, {GlyphCount - 1}]"
                        );
                    }

                    return _glyphs[index];
                }

                set {
                    _glyphs[index] = value;
                }
            }

            public Glyph LastGlyph {
                get {
                    return this[GlyphCount - 1];
                }
            }

            #endregion Public Properties

            #region Public Methods

            public Glyph GetGlyph(int index) {
                return _glyphs[index];
            }

            public Glyph AppendGlyph(
                double x,
                double y,
                Rectangle sourceArea,
                uint representation,
                Fonts.FontFaceRenderMap.Glyph data
            ) {
                if (GlyphCount == _glyphs.Length) {
                    throw new System.InvalidOperationException($"Trying to append a glyph where is no glyph slot left.");
                }

                if (data == null) {
                    throw new System.ArgumentNullException(
                        nameof(data), $"Missing '{nameof(Fonts.FontFaceRenderMap.Glyph)}'"
                    );
                }

                Glyph glyph = new Glyph(x, y, sourceArea, representation, data);
                _glyphs[GlyphCount] = glyph;
                GlyphCount++;

                return glyph;
            }

            public void Clear() {
                GlyphCount = 0;
            }

            public IEnumerator<Glyph> GetEnumerator() {
                for (int i = 0; i < GlyphCount; i++) {
                    yield return _glyphs[i];
                }
            }

            public Text.RenderData Clone() {
                Text.RenderData renderData = new Text.RenderData(GlyphCount);

                for (int i = 0; i < GlyphCount; i++) {
                    renderData._glyphs[i] = _glyphs[i].Clone();
                }

                renderData.GlyphCount = GlyphCount;
                return renderData;
            }

            #endregion Public Methods

            #region Glyph Struct

            public class Glyph {
                public Glyph(double x, double y, Rectangle sourceArea, uint representation, Fonts.FontFaceRenderMap.Glyph data) {
                    OriginalX = X = x;
                    OriginalY = Y = y;
                    SourceArea = sourceArea;
                    Representation = representation;
                    Data = data;
                }

                public double X { get; set; }
                public double Y { get; set; }
                public double OriginalX { get; }
                public double OriginalY { get; }
                public Rectangle SourceArea { get; set; }
                public uint Representation { get; set; }
                public Fonts.FontFaceRenderMap.Glyph Data { get; set; }

                public Glyph Clone() {
                    return new Glyph(OriginalX, OriginalY, SourceArea, Representation, Data) {
                        X = X,
                        Y = Y,
                    };
                }

                public override string ToString() {
                    return $"X: {X}, Y: {Y}; SourceArea: {SourceArea}; Representation: {Representation}; Has Data? {(Data != null).ToPrettyString()}";
                }
            }

            #endregion Glyph Struct
        }

        #endregion RenderInfo Class
    }
}
