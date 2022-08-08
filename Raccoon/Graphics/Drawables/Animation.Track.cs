namespace Raccoon.Graphics {
    public partial class Animation<KeyType> : Image {
        public class Track {
            private event System.Action _onEnd;
            private event System.Action<int> _onChangeFrame;

            private int _currentFrameIndex;

            public Track(Frame[] frames) {
                if (frames == null) {
                    throw new System.ArgumentNullException(nameof(frames));
                }

                Frames = frames;

                //

                foreach (Frame frame in Frames) {
                    TotalDuration += frame.Duration;
                }
            }

            public Track(Track track, ReplaceFrame?[] replaceFrames = null) {
                Frames = new Frame[track.Frames.Length];
                track.Frames.CopyTo(Frames, 0);

                if (replaceFrames != null) {
                    for (int i = 0; i < replaceFrames.Length; i++) {
                        ReplaceFrame? replace = replaceFrames[i];

                        if (!replace.HasValue) {
                            continue;
                        }

                        Frames[i] = replace.Value.Apply(Frames[i]);
                    }
                }

                RepeatTimes = track.RepeatTimes;
                IsPingPong = track.IsPingPong;
                IsReverse = track.IsReverse;

                if (track._onEnd != null) {
                    _onEnd += track._onEnd;
                }

                if (track._onChangeFrame != null) {
                    _onChangeFrame += track._onChangeFrame;
                }

                //

                foreach (Frame frame in Frames) {
                    TotalDuration += frame.Duration;
                }
            }

            public Frame[] Frames { get; }
            public ref Frame CurrentFrame { get { return ref Frames[CurrentFrameIndex]; } }
            public int RepeatTimes { get; set; }
            public int TimesPlayed { get; private set; }
            public int FrameCount { get { return Frames?.Length ?? 0; } }
            public int LastFrameIndex { get { return Frames.Length - 1; } }
            public int TotalDuration { get; }
            public bool HasEnded { get; private set; }
            public bool IsLooping { get { return RepeatTimes < 0; } set { RepeatTimes = value ? -1 : 0; } }
            public bool IsPingPong { get; set; }
            public bool IsReverse { get; set; }
            public bool IsForward { get { return !IsReverse; } set { IsReverse = !value; } }
            public bool IsDisposed { get; private set; }
            public bool IsAtLastFrame { get { return CurrentFrameIndex == LastFrameIndex; } }

            public int CurrentFrameIndex {
                get {
                    return _currentFrameIndex;
                }

                set {
                    if (value < 0 || value >= Frames.Length) {
                        if (Frames.Length == 0) {
                            throw new System.InvalidOperationException("There is no frame registered.");
                        }

                        throw new System.InvalidOperationException($"Provided value ({value}) is out of bounds [0, {Frames.Length - 1}].");
                    }

                    _currentFrameIndex = value;
                }
            }

            public void Reset() {
                HasEnded = Frames == null || Frames.Length == 0 || (Frames.Length == 1 && Frames[0].Duration == 0);
                CurrentFrameIndex = IsReverse ? LastFrameIndex : 0;
                TimesPlayed = 0;
            }

            public void NextFrame() {
                if (HasEnded) {
                    return;
                }

                if ((IsReverse && CurrentFrameIndex - 1 < 0)
                 || (IsForward && CurrentFrameIndex + 1 == Frames.Length)
                ) {
                    if (IsPingPong) {
                        IsReverse = !IsReverse;
                    }

                    if (IsLooping || TimesPlayed < RepeatTimes) {
                        CurrentFrameIndex = IsReverse ? LastFrameIndex : 0;
                        TimesPlayed++;
                        _onEnd?.Invoke();
                    } else {
                        HasEnded = true;
                        TimesPlayed++;
                        _onEnd?.Invoke();
                    }
                } else {
                    CurrentFrameIndex = IsReverse ? CurrentFrameIndex - 1 : CurrentFrameIndex + 1;
                    _onChangeFrame?.Invoke(CurrentFrameIndex);
                }
            }

            public void FirstFrame() {
                CurrentFrameIndex = 0;
            }

            public void LastFrame() {
                CurrentFrameIndex = LastFrameIndex;
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

                if (RepeatTimes < 1) {
                    RepeatTimes = 1;
                }

                return this;
            }

            public Track Reverse() {
                IsReverse = true;
                CurrentFrameIndex = LastFrameIndex;
                return this;
            }

            public Track Forward() {
                IsForward = true;
                CurrentFrameIndex = 0;
                return this;
            }

            public Track OnEnd(System.Action onEnd) {
                _onEnd += onEnd;
                return this;
            }

            public Track OnChangeFrame(System.Action<int> onChangeFrame) {
                _onChangeFrame += onChangeFrame;
                return this;
            }

            public void Dispose() {
                if (IsDisposed) {
                    return;
                }

                _onEnd = null;
                _onChangeFrame = null;

                IsDisposed = true;
            }

            #region Frame Struct

            public struct Frame {
                public Rectangle FrameRegion;
                public Rectangle? FrameDestination;
                public int GlobalIndex, Duration;

                public Frame(
                    int globalIndex,
                    int duration,
                    Rectangle frameRegion,
                    Rectangle? frameDestination
                ) {
                    GlobalIndex = globalIndex;
                    Duration = duration;
                    FrameRegion = frameRegion;
                    FrameDestination = frameDestination;
                }

                public Frame(int globalIndex, int duration, Rectangle frameRegion) {
                    GlobalIndex = globalIndex;
                    Duration = duration;
                    FrameRegion = frameRegion;
                    FrameDestination = null;
                }
            }

            #endregion Frame Struct

            #region ReplaceFrame Struct

            public struct ReplaceFrame {
                public Rectangle? FrameRegion, FrameDestination;
                public int? GlobalIndex, Duration;

                public ReplaceFrame(int? globalIndex, int? duration, Rectangle? frameRegion, Rectangle? frameDestination) {
                    GlobalIndex = globalIndex;
                    Duration = duration;
                    FrameRegion = frameRegion;
                    FrameDestination = frameDestination;
                }

                public Frame Apply(Frame frame) {
                    if (FrameRegion.HasValue) {
                        frame.FrameRegion = FrameRegion.Value;
                    }

                    if (FrameDestination.HasValue) {
                        frame.FrameDestination = FrameDestination.Value;
                    }

                    if (GlobalIndex.HasValue) {
                        frame.GlobalIndex = GlobalIndex.Value;
                    }

                    if (Duration.HasValue) {
                        frame.Duration = Duration.Value;
                    }

                    return frame;
                }
            }

            #endregion ReplaceFrame Struct
        }
    }
}
