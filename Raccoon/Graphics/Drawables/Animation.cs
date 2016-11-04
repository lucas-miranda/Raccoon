using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Raccoon.Graphics {
    public class Animation<KeyType> : Image {
        #region Private Readonly

        private readonly Regex FrameRegex = new Regex(@"(\d+)-(\d+)|(\d+)");
        private readonly Regex DurationRegex = new Regex(@"(\d+)");

        #endregion Private Readonly

        #region Private Members

        private int _columns, _rows;

        #endregion Private Members

        #region Constructors

        public Animation() {
            Tracks = new Dictionary<KeyType, Track>();
        }

        public Animation(string filename, Size frameSize) : this() {
            Texture = new Texture(filename);
            ClippingRegion = new Rectangle(frameSize);
            Load();
        }

        public Animation(AtlasSubTexture subTexture, Size frameSize) : this() {
            Texture = subTexture.Texture;
            SourceRegion = subTexture.Region;
            ClippingRegion = new Rectangle(frameSize);
            Load();
        }

        public Animation(AtlasAnimation animTexture) : this() {
            if (typeof(KeyType) != typeof(string)) {
                throw new NotSupportedException("KeyType " + typeof(KeyType) + " doesn't support AtlasAnimationTexture, switch to KeyType string");
            }

            Texture = animTexture.Texture;
            SourceRegion = animTexture.Region;
            ClippingRegion = new Rectangle(animTexture.FrameSize);
            foreach (KeyValuePair<string, List<AtlasAnimationFrame>> anim in animTexture) {
                string frames = "", durations = "";
                foreach (AtlasAnimationFrame frame in anim.Value) {
                    frames += frame.Id + " ";
                    durations += frame.Duration + " ";
                }

                Add((KeyType)(object) anim.Key, frames, durations);
            }

            Load();
        }

        #endregion Constructors

        #region Public Properties

        public Dictionary<KeyType, Track> Tracks { get; private set; }
        public bool Playing { get; private set; }
        public KeyType CurrentKey { get; private set; }
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
                    UpdateClippingRegion();
                } else if (CurrentTrack.IsLooping) {
                    CurrentTrack.CurrentFrameIndex = 0;
                    UpdateClippingRegion();
                } else {
                    Pause();
                }
            }
        }

        public void Play(KeyType key, bool forceReset = true) {
            Playing = true;
            if (CurrentTrack == null || !CurrentKey.Equals(key)) {
                CurrentKey = key;
                CurrentTrack = Tracks[CurrentKey];
                CurrentTrack.CurrentFrameIndex = 0;
                UpdateClippingRegion();
            } else if (forceReset) {
                CurrentTrack.CurrentFrameIndex = 0;
                UpdateClippingRegion();
            }
        }

        public void Pause() {
            Playing = false;
        }

        public void Stop() {
            Pause();
            CurrentTrack.CurrentFrameIndex = 0;
            UpdateClippingRegion();
        }

        public void Add(KeyType key, string frames, string durations) {
            List<int> durationList = new List<int>();
            foreach (Match m in DurationRegex.Matches(durations)) {
                durationList.Add(int.Parse(m.Groups[1].Value));
            }

            if (durationList.Count == 0) {
                return;
            }

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

            if (frameList.Count == 0) {
                return;
            }

            Tracks.Add(key, new Track(frameList.ToArray(), durationList.ToArray()));
        }

        public void Add(KeyType key, string frames, int duration) {
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
            for (int i = 0; i < frameList.Count; i++) {
                durations.SetValue(duration, i);
            }

            Tracks.Add(key, new Track(frameList.ToArray(), durations));
        }

        public void Add(KeyType key, ICollection<int> frames, ICollection<int> durations) {
            int[] frameList = new int[frames.Count];
            frames.CopyTo(frameList, 0);
            int[] durationList = new int[durations.Count];
            durations.CopyTo(durationList, 0);
            Tracks.Add(key, new Track(frameList, durationList));
        }

        public void Add(KeyType key, ICollection<int> frames, int duration) {
            int[] frameList = new int[frames.Count];
            frames.CopyTo(frameList, 0);

            int[] durations = new int[frameList.Length];
            for (int i = 0; i < frameList.Length; i++) {
                durations.SetValue(duration, i);
            }

            Tracks.Add(key, new Track(frameList, durations));
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();
            _columns = (int) (SourceRegion.Width / Size.Width);
            _rows = (int) (SourceRegion.Height / Size.Height);
        }

        #endregion Protected Methods

        #region Private Methods

        private void UpdateClippingRegion() {
            ClippingRegion = new Rectangle((CurrentTrack.CurrentSpriteID % _columns) * Size.Width, (CurrentTrack.CurrentSpriteID / _columns) * Size.Height, Size.Width, Size.Height);
        }

        #endregion Private Methods

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
            public int Duration { get { return Durations[CurrentFrameIndex]; } }
            //public int ElapsedTime { get; set; } // maybe I don't need to save time on each Track
            public int CurrentFrameIndex { get; set; }
            public bool IsLooping { get; set; }

            public int CurrentSpriteID {
                get { return Frames[CurrentFrameIndex]; }
                set {
                    for (int i = 0; i < Frames.Length; i++) {
                        if (Frames[i] == value)
                            CurrentFrameIndex = i;
                    }
                }
            }
        }

        #endregion Public Class Track
    }

    public class Animation : Animation<string> {
        #region Constructors

        public Animation() : base() { }
        public Animation(string filename, Size frameSize) : base(filename, frameSize) { }
        public Animation(AtlasSubTexture subTexture, Size frameSize) : base(subTexture, frameSize) { }
        public Animation(AtlasAnimation animTexture) : base(animTexture) { }

        #endregion Constructors
    }
}
