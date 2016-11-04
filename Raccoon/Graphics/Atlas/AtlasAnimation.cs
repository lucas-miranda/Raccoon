using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Graphics {
    public class AtlasAnimation : AtlasSubTexture, IEnumerable {
        #region Private Members

        private Dictionary<string, List<AtlasAnimationFrame>> _tracks;

        #endregion Private Members

        #region Constructors

        public AtlasAnimation(Size frameSize, Texture texture, Rectangle region) : base(texture, region) {
            _tracks = new Dictionary<string, List<AtlasAnimationFrame>>();
            _tracks.Add("Default", new List<AtlasAnimationFrame>());
            FrameSize = frameSize;
        }

        #endregion Constructors

        #region Public Properties

        public Size FrameSize { get; }

        public List<AtlasAnimationFrame> this[string tag] {
            get {
                return _tracks[tag];
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Add(int spriteId, int duration, string tag) {
            if (!_tracks.ContainsKey(tag)) {
                _tracks.Add(tag, new List<AtlasAnimationFrame>());
            }

            _tracks[tag].Add(new AtlasAnimationFrame(spriteId, duration));
        }

        public IEnumerator GetEnumerator() {
            return _tracks.GetEnumerator();
        }

        #endregion Public Methods
    }

    public struct AtlasAnimationFrame {
        public AtlasAnimationFrame(int id, int duration) {
            Id = id;
            Duration = duration;
        }

        public int Id { get; }
        public int Duration { get; }
    }
}
