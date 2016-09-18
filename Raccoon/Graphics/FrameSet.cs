using System.Collections.Generic;

namespace Raccoon.Graphics {
    class FrameSet : Image {
        #region Private Members

        private List<Rectangle> frames;
        private int currentId, columns, rows;

        #endregion Private Members

        #region Contructors

        public FrameSet(string path, int frameWidth, int frameHeight, int frameCount = -1) {
            frames = new List<Rectangle>();
            Name = path;
            FrameSize = new Size(frameWidth, frameHeight);
            FrameCount = System.Math.Max(-1, frameCount);
            Load();
        }

        #endregion Contructors

        #region Public Properties

        public Size FrameSize { get; private set; } 
        public int FrameCount { get; private set; }

        public int CurrentId {
            get { return currentId; }
            set {
                currentId = value;
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
            TextureRect = new Rectangle((CurrentId % columns) * FrameSize.Width, CurrentId / columns * FrameSize.Height, FrameSize.Width, FrameSize.Height);
        }

        #endregion Private Methods

        #region Internal Methods

        internal override void Load() {
            base.Load();
            if (Texture == null)
                return;

            int texColumns = (int) (Texture.Width / FrameSize.Width);
            int texRows = (int) (Texture.Height / FrameSize.Height);

            if (FrameCount == -1) {
                FrameCount = texColumns * texRows;
                columns = texColumns;
                rows = texRows;
            } else {
                columns = System.Math.Min(texColumns, FrameCount);
                rows = (int) System.Math.Ceiling((double) (FrameCount / texColumns));
            }

            UpdateTextureRect();
        }

        #endregion Internal Methods
    }
}
