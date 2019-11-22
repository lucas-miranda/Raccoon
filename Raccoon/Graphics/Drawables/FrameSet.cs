namespace Raccoon.Graphics {
    public class FrameSet : Image {
        #region Private Members

        private int _currentFrame;

        #endregion Private Members

        #region Contructors

        private FrameSet(Size frameSize, int frameCount) {
            FrameSize = frameSize;
            FrameCount = System.Math.Max(-1, frameCount);
        }

        private FrameSet(Size frameSize) : this(frameSize, -1) { 
        }

        public FrameSet(string filename, Size frameSize, int frameCount) : this(frameSize, frameCount) {
            Texture = new Texture(filename);
            Load();
        }

        public FrameSet(string filename, Size frameSize) : this(filename, frameSize, -1) {
        }

        public FrameSet(AtlasSubTexture subTexture, Size frameSize, int frameCount) : this(frameSize, frameCount) {
            Texture = subTexture.Texture;
            SourceRegion = subTexture.SourceRegion;
            Load();
        }

        public FrameSet(AtlasSubTexture subTexture, Size frameSize) : this(subTexture, frameSize, -1) {
        }

        public FrameSet(AtlasSubTexture subTexture) : this(subTexture, subTexture.ClippingRegion.Size, -1) {
        }

        #endregion Contructors

        #region Public Properties

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public Size FrameSize { get; private set; }
        public int FrameCount { get; private set; }

        public int CurrentFrame {
            get {
                return _currentFrame;
            }

            set {
                if (value < 0 || value >= FrameCount) throw new System.ArgumentOutOfRangeException("CurrentFrame", value, $"Frame Id must be inclusive between 0 and {FrameCount - 1} (FrameCount)");

                _currentFrame = value;
                UpdateClippingRegion();
            }
        }

        #endregion Public Properties

        #region Protected Methods

        protected override void Load() {
            base.Load();
            int texColumns = (int) (SourceRegion.Width / FrameSize.Width);
            int texRows = (int) (SourceRegion.Height / FrameSize.Height);

            if (FrameCount <= -1) {
                FrameCount = texColumns * texRows;
                Columns = texColumns;
                Rows = texRows;
            } else {
                Columns = System.Math.Min(texColumns, FrameCount);
                Rows = (int) System.Math.Ceiling((double) (FrameCount / texColumns));
            }

            UpdateClippingRegion();
        }

        #endregion Protected Methods

        #region Private Methods

        private void UpdateClippingRegion() {
            ClippingRegion = new Rectangle((CurrentFrame % Columns) * FrameSize.Width, CurrentFrame / Columns * FrameSize.Height, FrameSize.Width, FrameSize.Height);
        }

        #endregion Private Methods
    }
}
