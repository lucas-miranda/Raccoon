using System.Collections;
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
                _glyphs = new Glyph[glyphCount];
            }

            #endregion Constructors

            #region Public Properties

            public int GlyphCount { get; private set; }
            public int Capacity { get { return _glyphs.Length; } }

            public Glyph this[int index] {
                get {
                    return _glyphs[index];
                }

                set {
                    _glyphs[index] = value;
                }
            }

            #endregion Public Properties

            #region Public Methods

            public Glyph GetGlyph(int index) {
                return _glyphs[index];
            }

            public Glyph AppendGlyph(Glyph glyph) {
                if (GlyphCount == _glyphs.Length) {
                    throw new System.InvalidOperationException($"Trying to append a glyph where is no glyph slot left.");
                }

                _glyphs[GlyphCount] = glyph;
                GlyphCount++;
                return glyph;
            }

            public Glyph AppendGlyph(Vector2 position, Rectangle sourceArea, char representation, Fonts.FontFaceRenderMap.Glyph data) {
                Glyph glyph = new Glyph(position, sourceArea, representation, data);
                AppendGlyph(glyph);
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

            /*
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
            */

            #endregion Public Methods

            #region Glyph Struct

            public struct Glyph {
                public Glyph(Vector2 position, Rectangle sourceArea, char representation, Fonts.FontFaceRenderMap.Glyph data) {
                    Position = position;
                    SourceArea = sourceArea;
                    Representation = representation;
                    Data = data;
                }

                public Vector2 Position { get; set; }
                public Rectangle SourceArea { get; set; }
                public char Representation { get; set; }
                public Fonts.FontFaceRenderMap.Glyph Data { get; set; }

                public override string ToString() {
                    return $"Position: {Position}; SourceArea: {SourceArea}; Representation: {Representation}; Has Data? {(Data != null).ToPrettyString()}";
                }
            }

            #endregion Glyph Struct
        }

        #endregion RenderInfo Class
    }
}
