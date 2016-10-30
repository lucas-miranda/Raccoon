namespace Raccoon.Graphics {
    public class FrameSet : Image {
        #region Private Members

        private int _currentFrame;

        #endregion Private Members

        #region Contructors

        private FrameSet(Size frameSize, int frameCount = -1) {
            FrameSize = frameSize;
            FrameCount = System.Math.Max(-1, frameCount);
        }

        public FrameSet(string filename, Size frameSize, int frameCount = -1) : this(frameSize, frameCount) {
            Texture = new Texture(filename);
            Load();
        }

        public FrameSet(AtlasSubTexture subTexture, Size frameSize, int frameCount = -1) : this(frameSize, frameCount) {
            Texture = subTexture.Texture;
            SourceRegion = subTexture.Region;
            Load();
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
                _currentFrame = (int) Math.Clamp(value, 0, FrameCount - 1);
                UpdateClippingRegion();
            }
        }

        #endregion Public Properties

        #region Private Methods
        
        private void UpdateClippingRegion() {
            if (Columns == 0 || Rows == 0) {
                return;
            }

            ClippingRegion = new Rectangle((CurrentFrame % Columns) * FrameSize.Width, CurrentFrame / Columns * FrameSize.Height, FrameSize.Width, FrameSize.Height);
        }

        #endregion Private Methods

        #region Internal Methods

        internal override void Load() {
            base.Load();
            if (!Game.Instance.Core.IsContentManagerReady) {
                return;
            }

            int texColumns = (int) (SourceRegion.Width / FrameSize.Width);
            int texRows = (int) (SourceRegion.Height / FrameSize.Height);

            if (FrameCount == -1) {
                FrameCount = texColumns * texRows;
                Columns = texColumns;
                Rows = texRows;
            } else {
                Columns = System.Math.Min(texColumns, FrameCount);
                Rows = (int) System.Math.Ceiling((double) (FrameCount / texColumns));
            }

            UpdateClippingRegion();
        }

        #endregion Internal Methods
    }
}
