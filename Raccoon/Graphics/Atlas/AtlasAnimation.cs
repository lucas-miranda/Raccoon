using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Graphics {
    public class AtlasAnimation : AtlasSubTexture, IEnumerable {
        #region Private Members

        private Dictionary<string, List<AtlasAnimationFrame>> _tracks;

        #endregion Private Members

        #region Constructors

        public AtlasAnimation(Texture texture, Rectangle region) : base(texture, region) {
            _tracks = new Dictionary<string, List<AtlasAnimationFrame>> {
                { "all", new List<AtlasAnimationFrame>() }
            };
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

        public void Add(Rectangle clippingRegion, int duration, string tag) {
            if (!_tracks.ContainsKey(tag)) {
                _tracks.Add(tag, new List<AtlasAnimationFrame>());
            }

            _tracks[tag].Add(new AtlasAnimationFrame(duration, clippingRegion));
        }

        public IEnumerator GetEnumerator() {
            return _tracks.GetEnumerator();
        }

        #endregion Public Methods
    }
}
