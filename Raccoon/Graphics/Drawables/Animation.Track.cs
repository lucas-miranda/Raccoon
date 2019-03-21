namespace Raccoon.Graphics {
    public partial class Animation<KeyType> : Image {
        public class Track {
            private event System.Action _onEnd = delegate { }, 
                                        _onChangeFrame = delegate { };

            private int _currentFrameIndex;

            public Track(Rectangle[] framesRegions, int[] durations) {
                FramesRegions = framesRegions;
                Durations = durations;

                if (durations.Length < framesRegions.Length) {
                    // clone last element until reach same size
                    int toAdd = framesRegions.Length - durations.Length;
                    Durations = new int[durations.Length + toAdd];
                    durations.CopyTo(Durations, 0);

                    for (int i = durations.Length; i < Durations.Length; i++) {
                        Durations[i] = durations[durations.Length - 1];
                    }
                } else if (durations.Length > framesRegions.Length) {
                    // trim excess
                    int toRemove = durations.Length - framesRegions.Length;
                    Durations = new int[durations.Length - toRemove];

                    for (int i = 0; i < Durations.Length; i++) {
                        Durations[i] = durations[i];
                    }
                }
            }

            public Track(Track track, Rectangle[] replaceFrameRegions = null, int[] replaceDurations = null) {
                if (replaceFrameRegions != null) {
                    FramesRegions = replaceFrameRegions;
                } else {
                    FramesRegions = new Rectangle[track.FramesRegions.Length];
                    track.FramesRegions.CopyTo(FramesRegions, 0);
                }

                if (replaceDurations != null) {
                    Durations = replaceDurations;
                } else {
                    Durations = new int[track.Durations.Length];
                    track.Durations.CopyTo(Durations, 0);
                }

                RepeatTimes = track.RepeatTimes;
                IsPingPong = track.IsPingPong;
                IsReverse = track.IsReverse;

                _onEnd += track._onEnd;
                _onChangeFrame += track._onChangeFrame;
            }

            public Rectangle[] FramesRegions { get; private set; }
            public int[] Durations { get; private set; }
            public int CurrentFrameIndex { get { return _currentFrameIndex; } set { _currentFrameIndex = Util.Math.Clamp(value, 0, FramesRegions.Length - 1); } }
            public int CurrentFrameDuration { get { return Durations[CurrentFrameIndex]; } }
            public ref Rectangle CurrentFrameRegion { get { return ref FramesRegions[CurrentFrameIndex]; } }
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
                HasEnded = Durations.Length == 0 || (Durations.Length == 1 && Durations[0] == 0);
                CurrentFrameIndex = IsReverse ? FramesRegions.Length - 1 : 0;
                TimesPlayed = 0;
            }

            public void NextFrame() {
                if (HasEnded) {
                    return;
                }

                if ((IsReverse && CurrentFrameIndex - 1 < 0) || (IsForward && CurrentFrameIndex + 1 == FramesRegions.Length)) {
                    if (IsPingPong) {
                        IsReverse = !IsReverse;
                    }

                    if (IsLooping || TimesPlayed < RepeatTimes) {
                        CurrentFrameIndex = IsReverse ? FramesRegions.Length - 1 : 0;
                        TimesPlayed++;
                        _onEnd.Invoke();
                    } else {
                        HasEnded = true;
                        TimesPlayed++;
                        _onEnd.Invoke();
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

                if (RepeatTimes < 1) {
                    RepeatTimes = 1;
                }

                return this;
            }

            public Track Reverse() {
                IsReverse = true;
                CurrentFrameIndex = FramesRegions.Length - 1;
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

            public Track OnChangeFrame(System.Action onChangeFrame) {
                _onChangeFrame += onChangeFrame;
                return this;
            }
        }
    }
}
