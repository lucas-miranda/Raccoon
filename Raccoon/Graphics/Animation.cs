using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Raccoon.Graphics {
    public class Animation<KeyType> : Image {
        #region Private Readonly

        private readonly Regex FrameRegex = new Regex(@"(\d+)-(\d+)|(\d+)");
        private readonly Regex DurationRegex = new Regex(@"(\d+\.\d+)");

        #endregion Private Readonly

        #region Private Members

        private int columns, rows;

        #endregion Private Members

        #region Constructors

        public Animation() {
            Tracks = new Dictionary<KeyType, Track>();
            Playing = false;
        }

        public Animation(string path, int frameWidth, int frameHeight) : this() {
            Name = path;
            Size = new Size(frameWidth, frameHeight);
            if (Game.Instance.IsRunning) {
                Load();
            }
        }

        #endregion Constructors

        #region Public Properties

        public Dictionary<KeyType, Track> Tracks { get; private set; }
        public bool Playing { get; set; }
        public KeyType CurrentKey { get; set; }
        public Track CurrentTrack { get; private set; }
        public int ElapsedTime { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
            if (!Playing)
                return;

            ElapsedTime += delta;
            if (ElapsedTime >= CurrentTrack.Duration) {
                ElapsedTime = 0;
                if (CurrentTrack.CurrentFrameIndex + 1 < CurrentTrack.Frames.Length) {
                    CurrentTrack.CurrentFrameIndex++;
                    UpdateTextureRect();
                } else if (CurrentTrack.IsLooping) {
                    CurrentTrack.CurrentFrameIndex = 0;
                    UpdateTextureRect();
                } else {
                    Pause();
                }
            }
        }
        
        public void Add(KeyType key, string frames, string durations) {
            List<int> durationList = new List<int>();
            foreach (Match m in DurationRegex.Matches(durations)) {
                durationList.Add((int) (float.Parse(m.Groups[1].Value) * 1000));
            }

            if (durationList.Count == 0)
                return;

            List<int> frameList = new List<int>();
            int captureID = 0;
            foreach (Match m in FrameRegex.Matches(frames)) {
                if (m.Groups[3].Length > 0) {
                    frameList.Add(int.Parse(m.Groups[3].Value));
                    if (captureID == durationList.Count)
                        durationList.Add(durationList[durationList.Count - 1]);
                    captureID++;
                } else {
                    int d = durationList[captureID];
                    durationList.RemoveAt(captureID);
                    for (int i = int.Parse(m.Groups[1].Value); i <= int.Parse(m.Groups[2].Value); i++) {
                        frameList.Add(i);
                        durationList.Insert(captureID, d);
                        captureID++;
                    }
                }
            }

            if (frameList.Count == 0)
                return;

            Tracks.Add(key, new Track(frameList.ToArray(), durationList.ToArray()));
        }

        public void Add(KeyType key, string frames, float duration) {
            List<int> frameList = new List<int>();
            foreach (Match m in FrameRegex.Matches(frames)) {
                if (m.Groups[3].Length > 0) {
                    frameList.Add(int.Parse(m.Groups[3].Value));
                } else {
                    for (int i = int.Parse(m.Groups[1].Value); i <= int.Parse(m.Groups[2].Value); i++)
                        frameList.Add(i);
                }
            }

            int[] durations = new int[frameList.Count];
            int d = (int) (duration * 1000);
            for (int i = 0; i < frameList.Count; i++)
                durations.SetValue(d, i);

            Tracks.Add(key, new Track(frameList.ToArray(), durations));
        }

        public void Play(KeyType key, bool forceReset = false) {
            Playing = true;
            if (CurrentKey == null || !CurrentKey.Equals(key)) {
                CurrentKey = key;
                CurrentTrack = Tracks[CurrentKey];
                CurrentTrack.CurrentFrameIndex = 0;
                if (Game.Instance.IsRunning)
                    UpdateTextureRect();
            } else if (forceReset) {
                CurrentTrack.CurrentFrameIndex = 0;
                if (Game.Instance.IsRunning)
                    UpdateTextureRect();
            }
        }

        public void Pause() {
            Playing = false;
        }

        public void Stop() {
            Pause();
            CurrentTrack.CurrentFrameIndex = 0;
            UpdateTextureRect();
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateTextureRect() {
            int x = CurrentTrack.CurrentSpriteID % columns;
            int y = CurrentTrack.CurrentSpriteID / columns;
            TextureRect = new Rectangle(x * Size.Width, y * Size.Height, Size.Width, Size.Height);
        }

        #endregion Private Methods

        #region Internal Methods

        internal override void Load() {
            base.Load();
            columns = (int) (Texture.Width / Size.Width);
            rows = (int) (Texture.Height / Size.Height);
            UpdateTextureRect();
        }

        #endregion Internal Methods

        #region Public Class Track

        public class Track {
            public Track() {
                IsLooping = true;
            }

            public Track(int[] frames, int[] duration) : this() {
                Frames = frames;
                Durations = duration;
            }

            public int[] Frames { get; private set; }
            public int[] Durations { get; private set; }
            public float Duration { get { return Durations[CurrentFrameIndex]; } }
            //public float ElapsedTime { get; set; } // TODO: maybe I don't need to save time on each Track
            public int CurrentFrameIndex { get; set; }
            public int CurrentSpriteID {
                get { return Frames[CurrentFrameIndex]; }
                set {
                    for (int i = 0; i < Frames.Length; i++) {
                        if (Frames[i] == value)
                            CurrentFrameIndex = i;
                    }
                }
            }
            public bool IsLooping { get; set; }
        }

        #endregion Public Class Track
    }
}
