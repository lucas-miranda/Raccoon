using System.Collections.Generic;

namespace Raccoon.Graphics {
    public class FrameSet : Image {
        #region Private Members

        private List<Rectangle> _frames;
        private int _currentFrame, _columns, _rows;

        #endregion Private Members

        #region Contructors

        public FrameSet(string filename, Size frameSize, int frameCount = -1) {
            _frames = new List<Rectangle>();
            Name = filename;
            FrameSize = frameSize;
            FrameCount = System.Math.Max(-1, frameCount);
            Load();
        }

        #endregion Contructors

        #region Public Properties

        public Size FrameSize { get; private set; } 
        public int FrameCount { get; private set; }

        public int CurrentFrame {
            get {
                return _currentFrame;
            }

            set {
                _currentFrame = value;
                UpdateTextureRect();
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateTextureRect() {
            ClippingRegion = new Rectangle((CurrentFrame % _columns) * FrameSize.Width, CurrentFrame / _columns * FrameSize.Height, FrameSize.Width, FrameSize.Height);
        }

        #endregion Private Methods

        #region Internal Methods

        internal override void Load() {
            base.Load();
            if (Texture == null) {
                return;
            }

            int texColumns = (int) (Texture.Width / FrameSize.Width);
            int texRows = (int) (Texture.Height / FrameSize.Height);

            if (FrameCount == -1) {
                FrameCount = texColumns * texRows;
                _columns = texColumns;
                _rows = texRows;
            } else {
                _columns = System.Math.Min(texColumns, FrameCount);
                _rows = (int) System.Math.Ceiling((double) (FrameCount / texColumns));
            }

            UpdateTextureRect();
        }

        #endregion Internal Methods
    }
}
