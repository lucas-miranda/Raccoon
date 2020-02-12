using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Graphics {
    public class AtlasAnimation : AtlasSubTexture, IEnumerable {
        #region Private Members

        private Dictionary<string, List<AtlasAnimationFrame>> _tracks;

        #endregion Private Members

        #region Constructors

        public AtlasAnimation(Texture texture, Rectangle sourceRegion) : base(texture, sourceRegion, new Rectangle(Vector2.Zero, sourceRegion.Size)) {
            _tracks = new Dictionary<string, List<AtlasAnimationFrame>> {
                { "all", new List<AtlasAnimationFrame>() }
            };
        }

        public AtlasAnimation(Texture texture) : this(texture, texture.Bounds) {
        }

        #endregion Constructors

        #region Public Properties

        public List<AtlasAnimationFrame> this[string tag] {
            get {
                return _tracks[tag];
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddFrame(Rectangle clippingRegion, int duration, Rectangle originalFrame, string targetTag) {
            if (!_tracks.ContainsKey(targetTag)) {
                _tracks.Add(targetTag, new List<AtlasAnimationFrame>());
            }

            _tracks[targetTag].Add(new AtlasAnimationFrame(duration, clippingRegion, originalFrame));
        }

        public void AddFrame(Rectangle clippingRegion, int duration, string targetTag) {
            if (!_tracks.ContainsKey(targetTag)) {
                _tracks.Add(targetTag, new List<AtlasAnimationFrame>());
            }

            _tracks[targetTag].Add(new AtlasAnimationFrame(duration, clippingRegion));
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            _tracks.Clear();

            base.Dispose();
        }

        public IEnumerator GetEnumerator() {
            return _tracks.GetEnumerator();
        }

        #endregion Public Methods
    }
}
