﻿using System.Collections.Generic;

namespace Raccoon.Graphics {
    public class FrameSet : Image {
        #region Private Members

        private int _currentFrameColumn, _currentFrameRow;
        private Frame[] _frames;

        #endregion Private Members

        #region Contructors

        public FrameSet(Texture texture, Size frameSize, int frameCount) {
            if (texture == null) {
                throw new System.ArgumentNullException(nameof(texture));
            } else if (frameCount < 0) {
                throw new System.ArgumentException("Frame count can't be negative.", nameof(frameCount));
            }

            Setup(texture, new Rectangle(texture.Size), frameSize, frameCount);
        }

        public FrameSet(Texture texture, Size frameSize) {
            if (texture == null) {
                throw new System.ArgumentNullException(nameof(texture));
            }

            Setup(texture, new Rectangle(texture.Size), frameSize);
        }

        public FrameSet(string filename, Size frameSize, int frameCount) : this(new Texture(filename), frameSize, frameCount) {
        }

        public FrameSet(string filename, Size frameSize) : this(new Texture(filename), frameSize) {
        }

        public FrameSet(AtlasSubTexture subTexture, Size frameSize, int frameCount) {
            if (subTexture == null) {
                throw new System.ArgumentNullException(nameof(subTexture));
            } else if (frameCount < 0) {
                throw new System.ArgumentException("Frame count can't be negative.", nameof(frameCount));
            }

            Setup(subTexture.Texture, subTexture.SourceRegion, frameSize, frameCount);
        }

        public FrameSet(AtlasSubTexture subTexture, Size frameSize) {
            if (subTexture == null) {
                throw new System.ArgumentNullException(nameof(subTexture));
            }

            Setup(subTexture.Texture, subTexture.SourceRegion, frameSize);
        }

        public FrameSet(AtlasSubTexture subTexture) {
            if (subTexture == null) {
                throw new System.ArgumentNullException(nameof(subTexture));
            }

            Setup(subTexture.Texture, subTexture.SourceRegion, subTexture.ClippingRegion.Size);
        }

        public FrameSet(AtlasAnimation animTexture, string trackName) {
            Texture = animTexture.Texture;
            SourceRegion = animTexture.SourceRegion;

            if (!animTexture.TryGetTrack(trackName, out List<AtlasAnimationFrame> frames)) {
                ClippingRegion = Raccoon.Rectangle.Empty;
                return;
            }

            _frames = new Frame[frames.Count];

            for (int i = 0; i < frames.Count; i++) {
                AtlasAnimationFrame atlasAnimationFrame = frames[i];

                _frames[i] = new Frame {
                    ClippingRegion = atlasAnimationFrame.ClippingRegion,
                    Destination = atlasAnimationFrame.OriginalFrame,
                };
            }

            if (_frames.Length > 0) {
                Columns = _frames.Length;
                Rows = 1;
                FrameSize = _frames[0].Destination.Size;
                CurrentFrameIndex = 0;
            }
        }

        public FrameSet(AtlasAnimation animTexture) : this(animTexture, AtlasAnimation.DefaultAllFramesTrackName) {
        }

        #endregion Contructors

        #region Public Properties

        public Size FrameSize { get; private set; }
        public int FrameCount { get { return _frames == null ? 0 : _frames.Length; } }
        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public int CurrentFrameIndex {
            get {
                if (FrameCount == 0) {
                    return -1;
                }

                return _currentFrameRow * Columns + _currentFrameColumn;
            }

            set {
                if (FrameCount == 0) {
                    throw new System.InvalidOperationException("There is no registered frame.");
                } else if (value < 0 || value >= FrameCount) {
                    throw new System.ArgumentOutOfRangeException("CurrentFrame", value, $"Frame Id must be in range  [0, {FrameCount - 1}]");
                }

                _currentFrameRow = value / Columns;
                _currentFrameColumn = value % Columns;
                UpdateClippingRegion();
            }
        }

        public Frame this[int index] {
            get {
                return _frames[index];
            }
        }

        public int CurrentFrameColumn {
            get {
                return _currentFrameColumn;
            }

            set {
                if (Columns == 0) {
                    throw new System.InvalidOperationException("There is no columns.");
                } else if (value < 0 || value >= Columns) {
                    throw new System.ArgumentOutOfRangeException(
                        nameof(CurrentFrameColumn),
                        value,
                        $"Frame Column must be in range [0, {Columns - 1}]"
                    );
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
                if (Rows == 0) {
                    throw new System.InvalidOperationException("There is no rows.");
                } else if (value < 0 || value >= Rows) {
                    throw new System.ArgumentOutOfRangeException(
                        nameof(CurrentFrameRow),
                        value,
                        $"Frame Row must be in range [0, {Rows - 1}]"
                    );
                }

                _currentFrameRow = value;
                UpdateClippingRegion();
            }
        }

        /*
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
        */

        #endregion Public Properties

        #region Public Methods

        public void ChangeFrameCell(int column, int row) {
            if (Columns == 0) {
                throw new System.InvalidOperationException("There is no columns.");
            } else if (column < 0 || column >= Columns) {
                throw new System.ArgumentOutOfRangeException(
                    nameof(column),
                    column,
                    $"Frame Column must be in range [0, {Columns - 1}]"
                );
            }

            if (Rows == 0) {
                throw new System.InvalidOperationException("There is no rows.");
            } else if (row < 0 || row >= Rows) {
                throw new System.ArgumentOutOfRangeException(
                    nameof(row),
                    row,
                    $"Frame Row must be in range [0, {Rows - 1}]"
                );
            }

            _currentFrameColumn = column;
            _currentFrameRow = row;
            UpdateClippingRegion();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();
        }

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            CalculateFrameOrigin(flip, ref origin);

            base.Draw(position, rotation, scale, flip, color, scroll, shader, shaderParameters, origin, layerDepth);
        }

        #endregion Protected Methods

        #region Private Methods

        private void Setup(Texture texture, Rectangle sourceRegion, Size frameSize, int frameCount) {
            Texture = texture;
            SourceRegion = sourceRegion;

            if (frameSize.Width > sourceRegion.Width) {
                throw new System.ArgumentException(nameof(frameSize), "Frame width can't be greater than source region width.");
            } else if (frameSize.Height > sourceRegion.Height) {
                throw new System.ArgumentException(nameof(frameSize), "Frame height can't be greater than source region height.");
            }

            _frames = new Frame[frameCount];
            Vector2 pos = Vector2.Zero;

            int columns = 0,
                rows = 0;

            for (int i = 0; i < _frames.Length; i++) {
                _frames[i] = new Frame(new Rectangle(pos, frameSize));

                if (pos.X + frameSize.Width >= sourceRegion.Width) {
                    if (pos.Y + frameSize.Height >= sourceRegion.Height) {
                        throw new System.ArgumentException(nameof(frameSize), $"Can only generate {i + 1} frames out of {frameCount} requested frames.");
                    }

                    pos = new Vector2(0f, pos.Y + frameSize.Height);

                    if (columns > Columns) {
                        Columns = columns;
                    }

                    columns = 0;
                    rows += 1;
                } else {
                    pos += new Vector2(frameSize.Width, 0f);
                    columns += 1;
                }
            }

            Rows = rows;

            if (_frames.Length > 0) {
                FrameSize = _frames[0].Destination.Size;
                CurrentFrameIndex = 0;
            }

            Load();
        }

        private void Setup(Texture texture, Rectangle sourceRegion, Size frameSize) {
            Texture = texture;
            SourceRegion = sourceRegion;

            if (frameSize.Width > sourceRegion.Width) {
                throw new System.ArgumentException(nameof(frameSize), "Frame width can't be greater than source region width.");
            } else if (frameSize.Height > sourceRegion.Height) {
                throw new System.ArgumentException(nameof(frameSize), "Frame height can't be greater than source region height.");
            }

            List<Frame> frames = new List<Frame>();
            Vector2 pos = Vector2.Zero;

            int columns = 0,
                rows = 0;

            while (pos.Y + frameSize.Height <= sourceRegion.Height) {
                frames.Add(new Frame(new Rectangle(pos, frameSize)));

                if (pos.X + frameSize.Width >= sourceRegion.Width) {
                    pos = new Vector2(0f, pos.Y + frameSize.Height);

                    if (columns + 1 > Columns) {
                        Columns = columns + 1;
                    }

                    columns = 0;
                    rows += 1;
                } else {
                    pos += new Vector2(frameSize.Width, 0f);
                    columns += 1;
                }
            }

            Rows = rows;
            _frames = frames.ToArray();

            if (_frames.Length > 0) {
                FrameSize = _frames[0].Destination.Size;
                CurrentFrameIndex = 0;
            }

            Load();
        }

        private void CalculateFrameOrigin(ImageFlip flip, ref Vector2 origin) {
            if (FrameCount == 0) {
                return;
            }

            // frame destination works like a guide to where render at local space
            // 'frameDestination.position' should be interpreted as 'origin'
            Frame frame = _frames[CurrentFrameIndex];

            if (frame.Destination.Position == Vector2.Zero) {
                return;
            }

            // frame region defines the crop rectangle at it's texture
            //ref Rectangle frameRegion = ref CurrentTrack.CurrentFrameRegion;

            if (flip.IsHorizontal()) {
                if (flip.IsVertical()) {
                    float rightSideSpacing = frame.Destination.Width - frame.ClippingRegion.Width + frame.Destination.X,
                          bottomSideSpacing = frame.Destination.Height - frame.ClippingRegion.Height + frame.Destination.Y;

                    origin = new Vector2(
                        (frame.ClippingRegion.Width - origin.X) - rightSideSpacing,
                        (frame.ClippingRegion.Height - origin.Y) - bottomSideSpacing
                    );
                } else {
                    float rightSideSpacing = frame.Destination.Width - frame.ClippingRegion.Width + frame.Destination.X;

                    origin = new Vector2(
                        (frame.ClippingRegion.Width - origin.X) - rightSideSpacing,
                        origin.Y + frame.Destination.Y
                    );
                }
            } else if (flip.IsVertical()) {
                float bottomSideSpacing = frame.Destination.Height - frame.ClippingRegion.Height + frame.Destination.Y;

                origin = new Vector2(
                    origin.X + frame.Destination.X,
                    (frame.ClippingRegion.Height - origin.Y) - bottomSideSpacing
                );
            } else {
                origin += frame.Destination.Position;
            }

            // maybe we should make a careful check before modifying DestinationRegion here
            // user could modify value globally to Animation and we'll be interfering
            // making a change frame based here

            /*
            // there we give the power to frame regions deform animations
            // I don't know if it will be usefull anywhere, but we can do this
            ref Rectangle frameRegion = ref CurrentTrack.CurrentFrameRegion;
            if (frameDestination.Size != frameRegion.Size) {
                DestinationRegion = new Rectangle(frameDestination.Size);
            }
            */
        }

        private void UpdateClippingRegion() {
            ClippingRegion = _frames[CurrentFrameIndex].ClippingRegion;
        }

        #endregion Private Methods

        #region Frame Class

        public class Frame {
            public Frame() {
            }

            public Frame(Rectangle clippingRegion) {
                ClippingRegion = clippingRegion;
                Destination = new Rectangle(ClippingRegion.Size);
            }

            public Rectangle ClippingRegion { get; set; }
            public Rectangle Destination { get; set; }
        }

        #endregion Frame Class
    }
}
