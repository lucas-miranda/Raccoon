using System.Collections;
using System.Collections.Generic;

namespace Raccoon.Graphics {
    public class AtlasAnimation : AtlasSubTexture, IEnumerable {
        public const string DefaultAllFramesTrackName = "all";

        #region Private Members

        private Dictionary<string, List<AtlasAnimationFrame>> _tracks;

        #endregion Private Members

        #region Constructors

        public AtlasAnimation(Texture texture, Rectangle sourceRegion) : base(texture, sourceRegion, new Rectangle(Vector2.Zero, sourceRegion.Size)) {
            _tracks = new Dictionary<string, List<AtlasAnimationFrame>> {
                { DefaultAllFramesTrackName, new List<AtlasAnimationFrame>() }
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

        public bool TryGetTrack(string tag, out List<AtlasAnimationFrame> track) {
            return _tracks.TryGetValue(tag, out track);
        }

        public bool TryGetDefaultTrack(out List<AtlasAnimationFrame> frames) {
            return TryGetTrack(DefaultAllFramesTrackName, out frames);
        }

        public void AddFrame(
            int globalIndex,
            Rectangle clippingRegion,
            int duration,
            Rectangle originalFrame,
            string targetTag
        ) {
            if (!_tracks.ContainsKey(targetTag)) {
                _tracks.Add(targetTag, new List<AtlasAnimationFrame>());
            }

            _tracks[targetTag].Add(new AtlasAnimationFrame(
                globalIndex,
                duration,
                clippingRegion,
                originalFrame
            ));
        }

        public void AddFrame(
            int globalIndex,
            Rectangle clippingRegion,
            int duration,
            string targetTag
        ) {
            if (!_tracks.ContainsKey(targetTag)) {
                _tracks.Add(targetTag, new List<AtlasAnimationFrame>());
            }

            _tracks[targetTag].Add(new AtlasAnimationFrame(
                globalIndex,
                duration,
                clippingRegion
            ));
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
