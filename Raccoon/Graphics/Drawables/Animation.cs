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
        private Dictionary<KeyType, Track> _tracks = new Dictionary<KeyType, Track>();

        #endregion Private Members

        #region Constructors

        public Animation() { }

        public Animation(Texture texture, Size frameSize) {
            Texture = texture;
            ClippingRegion = new Rectangle(frameSize);
            Load();
        }

        public Animation(string filename, Size frameSize) : this(new Texture(filename), frameSize) { }

        public Animation(AtlasSubTexture subTexture, Size frameSize) {
            Texture = subTexture.Texture;
            SourceRegion = subTexture.Region;
            ClippingRegion = new Rectangle(frameSize);
            Load();
        }

        public Animation(AtlasAnimation animTexture) {
            // TODO: support KeyType with AtlasAnimation, converting string animations label to KeyType enum
            if (typeof(KeyType) != typeof(string)) {
                throw new NotSupportedException("KeyType " + typeof(KeyType) + " doesn't support AtlasAnimationTexture, switch to string");
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

        public Animation(Animation<KeyType> animation) {
            Texture = animation.Texture;
            SourceRegion = animation.SourceRegion;
            ClippingRegion = animation.ClippingRegion;
            Load();

            if (!animation.DestinationRegion.IsEmpty) {
                DestinationRegion = animation.DestinationRegion;
            }

            foreach (KeyValuePair<KeyType, Track> track in animation._tracks) {
                Add(track.Key, new Track(track.Value));
            }
        }

        #endregion Constructors

        #region Public Properties

        public bool IsPlaying { get; private set; }
        public KeyType CurrentKey { get; private set; }
        public Track CurrentTrack { get; private set; }
        public int ElapsedTime { get; private set; }
        public float PlaybackSpeed { get; set; } = 1f;

        public Track this[KeyType key] { get { return _tracks[key]; } }

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
            if (!IsPlaying) {
                return;
            }

            ElapsedTime += (int) Math.Round(delta * PlaybackSpeed);
            if (ElapsedTime >= CurrentTrack.CurrentFrameDuration) {
                ElapsedTime -= CurrentTrack.CurrentFrameDuration;
                CurrentTrack.NextFrame();
                if (CurrentTrack.HasEnded) {
                    Pause();
                } else {
                    UpdateClippingRegion();
                }
            }
        }

        public void Play(bool forceReset = true) {
            if (CurrentTrack == null) throw new InvalidOperationException("Animation can't play or resume an invalid track");

            IsPlaying = true;
            if (forceReset) {
                CurrentTrack.Reset();
                UpdateClippingRegion();
            }
        }

        public void Play(KeyType key, bool forceReset = true) {
            IsPlaying = true;
            if (CurrentTrack == null || !CurrentKey.Equals(key)) {
                CurrentKey = key;
                CurrentTrack = _tracks[CurrentKey];
                CurrentTrack.Reset();
                UpdateClippingRegion();
            } else if (forceReset) {
                CurrentTrack.Reset();
                UpdateClippingRegion();
            }
        }

        public void Resume() {
            Play(false);
        }

        public void Pause() {
            IsPlaying = false;
        }

        public void Stop() {
            Pause();
            CurrentTrack.Reset();
            UpdateClippingRegion();
        }

        public void Add(KeyType key, string frames, string durations) {
            if (string.IsNullOrWhiteSpace(frames)) throw new ArgumentException("Value is empty", "frames");
            if (string.IsNullOrWhiteSpace(durations)) throw new ArgumentException("Value is empty", "durations");

            List<int> durationList = new List<int>();
            foreach (Match m in DurationRegex.Matches(durations)) {
                durationList.Add(int.Parse(m.Groups[1].Value));
            }

            if (durationList.Count == 0) throw new ArgumentException("Wrong value format", "durations");

            List<int> frameList = new List<int>();
            int frameIndex = 0;
            foreach (Match m in FrameRegex.Matches(frames)) {
                string frameId = m.Groups[3].Value;
                if (frameId.Length > 0) {
                    frameList.Add(int.Parse(frameId));
                    if (frameIndex >= durationList.Count) {
                        durationList.Add(durationList[durationList.Count - 1]);
                    }

                    frameIndex++;
                } else {
                    int d = durationList[frameIndex], startRange = int.Parse(m.Groups[1].Value), endRange = int.Parse(m.Groups[2].Value);
                    durationList.RemoveAt(frameIndex);
                    for (int i = startRange; i <= endRange; i++) {
                        frameList.Add(i);
                        durationList.Insert(frameIndex, d);
                        frameIndex++;
                    }
                }
            }

            if (frameList.Count == 0) throw new ArgumentException("Wrong value format", "frames");

            if (durationList.Count > frameList.Count) {
                durationList.RemoveRange(frameList.Count - 1, durationList.Count - frameList.Count);
            }

            Add(key, new Track(frameList.ToArray(), durationList.ToArray()));
        }

        public void Add(KeyType key, string frames, int duration) {
            if (string.IsNullOrWhiteSpace(frames)) throw new ArgumentException("Value is empty", "frames");
            if (duration < 0) throw new ArgumentException("Value is invalid, must be greater or equal zero", "durations");

            List<int> frameList = new List<int>();
            foreach (Match m in FrameRegex.Matches(frames)) {
                string frameId = m.Groups[3].Value;
                if (frameId.Length > 0) {
                    frameList.Add(int.Parse(frameId));
                } else {
                    int startRange = int.Parse(m.Groups[1].Value), endRange = int.Parse(m.Groups[2].Value);
                    for (int i = startRange; i <= endRange; i++) {
                        frameList.Add(i);
                    }
                }
            }

            if (frameList.Count == 0) throw new ArgumentException("Wrong value format", "frames");

            int[] durations = new int[frameList.Count];
            for (int i = 0; i < frameList.Count; i++) {
                durations.SetValue(duration, i);
            }

            Add(key, new Track(frameList.ToArray(), durations));
        }

        public void Add(KeyType key, ICollection<int> frames, ICollection<int> durations) {
            if (frames.Count == 0) throw new ArgumentException("Value is empty", "frames");
            if (durations.Count == 0) throw new ArgumentException("Value is empty", "durations");

            int[] frameList = new int[frames.Count];
            frames.CopyTo(frameList, 0);
            int[] durationList = new int[durations.Count];
            durations.CopyTo(durationList, 0);
            Add(key, new Track(frameList, durationList));
        }

        public void Add(KeyType key, ICollection<int> frames, int duration) {
            if (frames.Count == 0) throw new ArgumentException("Value is empty", "frames");
            if (duration < 0) throw new ArgumentException("Value is invalid, must be greater or equal zero", "durations");

            int[] frameList = new int[frames.Count];
            frames.CopyTo(frameList, 0);

            int[] durations = new int[frameList.Length];
            for (int i = 0; i < frameList.Length; i++) {
                durations.SetValue(duration, i);
            }

            Add(key, new Track(frameList, durations));
        }

        public void Add(KeyType key, Track track) {
            _tracks.Add(key, track);
        }

        public bool ContainsTrack(KeyType key) {
            return _tracks.ContainsKey(key);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Load() {
            base.Load();
            _columns = (int) (SourceRegion.Width / ClippingRegion.Width);
            _rows = (int) (SourceRegion.Height / ClippingRegion.Height);
        }

        #endregion Protected Methods

        #region Private Methods

        private void UpdateClippingRegion() {
            ClippingRegion = new Rectangle((CurrentTrack.CurrentSpriteID % _columns) * ClippingRegion.Width, (CurrentTrack.CurrentSpriteID / _columns) * ClippingRegion.Height, ClippingRegion.Width, ClippingRegion.Height);
        }

        #endregion Private Methods

        #region Public Class Track

        public class Track {
            private event Action _onEnd = delegate { }, _onChangeFrame = delegate { };
            private int _currentFrameIndex;

            public Track(int[] frames, int[] duration) {
                Frames = frames;
                Durations = duration;
            }

            public Track(Track track) {
                Frames = new int[track.Frames.Length];
                track.Frames.CopyTo(Frames, 0);

                Durations = new int[track.Durations.Length];
                track.Durations.CopyTo(Durations, 0);

                RepeatTimes = track.RepeatTimes;
                IsPingPong = track.IsPingPong;
                IsReverse = track.IsReverse;

                _onEnd += track._onEnd;
                _onChangeFrame += track._onChangeFrame;
            }

            public int[] Frames { get; private set; }
            public int[] Durations { get; private set; }
            public int CurrentFrameIndex { get { return _currentFrameIndex; } set { _currentFrameIndex = Util.Math.Clamp(value, 0, Frames.Length - 1); } }
            public int CurrentFrameDuration { get { return Durations[CurrentFrameIndex]; } }
            public int CurrentSpriteID { get { return Frames[CurrentFrameIndex]; } }
            public int RepeatTimes { get; set; }
            public int TimesPlayed { get; private set; }
            public bool HasEnded { get; private set; }
            public bool IsLooping { get { return RepeatTimes < 0; } set { RepeatTimes = value ? -1 : 0; } }
            public bool IsPingPong { get; set; }
            public bool IsReverse { get; set; }
            public bool IsForward { get { return !IsReverse; } set { IsReverse = !value; } }

            public int TotalDuration {
                get {
                    int total = 0;
                    foreach (int t in Durations) {
                        total += t;
                    }

                    return total;
                }
            }

            public void Reset() {
                HasEnded = false;
                CurrentFrameIndex = IsReverse ? Frames.Length - 1 : 0;
                TimesPlayed = 0;
            }

            public void NextFrame() {
                if (HasEnded) {
                    return;
                }

                if ((IsReverse && CurrentFrameIndex - 1 < 0) || (IsForward && CurrentFrameIndex + 1 == Frames.Length)) {
                    if (IsPingPong) {
                        IsReverse = !IsReverse;
                    } 
                    
                    if (IsLooping || TimesPlayed < RepeatTimes) {
                        CurrentFrameIndex = IsReverse ? Frames.Length - 1 : 0;
                        TimesPlayed++;
                        _onEnd.Invoke();
                    } else {
                        HasEnded = true;
                        _onEnd.Invoke();
                        return;
                    }
                } else {
                    CurrentFrameIndex = IsReverse ? CurrentFrameIndex - 1 : CurrentFrameIndex + 1;
                    _onChangeFrame.Invoke();
                }
            }

            public Track Repeat(int times = -1) {
                RepeatTimes = times;
                return this;
            }

            public Track Loop() {
                IsLooping = true;
                return this;
            }

            public Track PingPong() {
                IsPingPong = true;
                RepeatTimes = 1;
                return this;
            }

            public Track Reverse() {
                IsReverse = true;
                CurrentFrameIndex = Frames.Length - 1;
                return this;
            }

            public Track Forward() {
                IsForward = true;
                CurrentFrameIndex = 0;
                return this;
            }

            public Track OnEnd(Action onEnd) {
                _onEnd += onEnd;
                return this;
            }

            public Track OnChangeFrame(Action onChangeFrame) {
                _onChangeFrame += onChangeFrame;
                return this;
            }
        }

        #endregion Public Class Track
    }

    public class Animation : Animation<string> {
        #region Constructors

        public Animation() : base() { }
        public Animation(Texture texture, Size frameSize) : base(texture, frameSize) { }
        public Animation(string filename, Size frameSize) : base(filename, frameSize) { }
        public Animation(AtlasSubTexture subTexture, Size frameSize) : base(subTexture, frameSize) { }
        public Animation(AtlasAnimation animTexture) : base(animTexture) { }
        public Animation(Animation animation) : base(animation) { }

        #endregion Constructors
    }
}
