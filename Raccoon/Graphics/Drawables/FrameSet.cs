namespace Raccoon.Graphics {
    public class FrameSet : Image {
        #region Private Members

        private int _currentFrameColumn, _currentFrameRow;

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

        public FrameSet(Texture texture, Size frameSize, int frameCount) : this(frameSize, frameCount) {
            Texture = texture;
            Load();
        }

        public FrameSet(Texture texture, Size frameSize) : this(texture, frameSize, -1) {
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
                return Columns * CurrentFrameRow + CurrentFrameColumn;
            }

            set {
                if (value < 0 || value >= FrameCount) {
                    throw new System.ArgumentOutOfRangeException("CurrentFrame", value, $"Frame Id must be in range  [0, {FrameCount - 1}]");
                }

                if (Columns == 0) {
                    _currentFrameColumn = value;
                    _currentFrameRow = 0;
                } else {
                    int column = value % Columns,
                        row = value / Columns;

                    _currentFrameColumn = column;
                    _currentFrameRow = row;
                }

                UpdateClippingRegion();
            }
        }

        public int CurrentFrameColumn {
            get {
                return _currentFrameColumn;
            }

            set {
                if (value < 0 || value >= Columns) {
                    throw new System.ArgumentOutOfRangeException("CurrentFrameColumn", value, $"Frame Column must be in range [0, {Columns - 1}]");
                }

                _currentFrameColumn = value;
                UpdateClippingRegion();
            }
        }

        public int CurrentFrameRow {
            get {
                return _currentFrameRow;
            }

            set {
                if (value < 0 || value >= Rows) {
                    throw new System.ArgumentOutOfRangeException("CurrentFrameRow", value, $"Frame Row must be in range [0, {Rows - 1}]");
                }

                _currentFrameColumn = value;
                UpdateClippingRegion();
            }
        }

        public (int Column, int Row) CurrentFrameCell {
            get {
                return (_currentFrameColumn, _currentFrameRow);
            }

            set {
                if (value.Column < 0 || value.Column >= Columns) {
                    throw new System.ArgumentOutOfRangeException("CurrentFrameCell.Column", value, $"Frame Column must be in range [0, {Columns - 1}]");
                }

                if (value.Row < 0 || value.Row >= Rows) {
                    throw new System.ArgumentOutOfRangeException("CurrentFrameCell.Row", value, $"Frame Row must be in range [0, {Rows - 1}]");
                }

                _currentFrameColumn = value.Column;
                _currentFrameRow = value.Row;
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
            ClippingRegion = new Rectangle(CurrentFrameColumn * FrameSize.Width, CurrentFrameRow * FrameSize.Height, FrameSize.Width, FrameSize.Height);
        }

        #endregion Private Methods
    }
}
