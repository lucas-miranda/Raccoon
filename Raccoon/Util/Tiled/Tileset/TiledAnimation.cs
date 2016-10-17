using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Raccoon.Tiled {
    public class TiledAnimation : IEnumerable<TiledAnimationFrame> {
        private List<TiledAnimationFrame> _frames;

        public TiledAnimation(XmlElement animationElement) {
            _frames = new List<TiledAnimationFrame>();
            foreach (XmlElement frameElement in animationElement) {
                _frames.Add(new TiledAnimationFrame(int.Parse(frameElement.GetAttribute("tileid")), int.Parse(frameElement.GetAttribute("duration"))));
            }
        }

        public int Count { get { return _frames.Count; } }

        public TiledAnimationFrame this[int i] {
            get {
                return _frames[i];
            }
        }

        public IEnumerator<TiledAnimationFrame> GetEnumerator() {
            return _frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    public struct TiledAnimationFrame {
        public int TileId, Duration;

        public TiledAnimationFrame(int tileId, int duration) {
            TileId = tileId;
            Duration = duration;
        }
    }
}
