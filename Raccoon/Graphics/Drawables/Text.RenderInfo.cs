using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Graphics {
    public partial class Text {
        #region RenderInfo Class

        internal class RenderData : IEnumerable<RenderData.Glyph> {
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

            #endregion Public Properties

            #region Public Methods

            public void AppendGlyph(Glyph glyph) {
                if (GlyphCount == _glyphs.Length) {
                    throw new System.InvalidOperationException($"Trying to append a glyph where is no glyph slot left.");
                }

                _glyphs[GlyphCount] = glyph;
                GlyphCount++;
            }

            public void AppendGlyph(Vector2 position, Rectangle sourceArea) {
                AppendGlyph(new Glyph(position, sourceArea));
            }

            public void Clear() {
                GlyphCount = 0;
            }

            public IEnumerator<Glyph> GetEnumerator() {
                for (int i = 0; i < GlyphCount; i++) {
                    yield return _glyphs[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            #endregion Public Methods

            #region Glyph Struct

            internal struct Glyph {
                public Glyph(Vector2 position, Rectangle sourceArea) {
                    Position = position;
                    SourceArea = sourceArea;
                }

                public Vector2 Position { get; }
                public Rectangle SourceArea { get; }
            }

            #endregion Glyph Struct
        }

        #endregion RenderInfo Class
    }
}
