using Raccoon.Util;

namespace Raccoon.Graphics.ScreenEffects {
    public abstract class ScreenEffect : Graphic {
        #region Public Members

        public event System.Action OnStart,
                                   OnEnd,
                                   OnCycleBegin,
                                   OnCycleEnd;

        private bool _reverse, _startReversed;

        #endregion Public Members

        #region Constructors

        public ScreenEffect(uint cycleDuration, int repeatTimes = 0, bool pingpong = false) {
            Renderer = Game.Instance.InterfaceRenderer;
            CycleDuration = cycleDuration;
            RepeatTimes = repeatTimes;
            PingPong = pingpong;
        }

        #endregion Constructors

        #region Public Properties

        public int PlayCount { get; private set; }
        public int RepeatTimes { get; private set; }
        public float Time { get; protected set; }
        public float ElapsedTime { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool HasEnded { get; private set; }
        public bool PingPong { get; private set; }
        public uint CycleDuration { get; private set; }
        public uint TotalDuration { get { return CycleDuration * (uint) (Math.Max(0, RepeatTimes) + 1); } }
        public uint Timer { get; private set; }

        public bool Reverse {
            get {
                return _reverse;
            }

            set {
                _reverse = value;
                _startReversed = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
            base.Update(delta);

            if (!IsPlaying || IsPaused) {
                return;
            }

            Timer += (uint) delta;

            if (Timer >= CycleDuration) {
                CycleUpdate(delta);
                Finish(all: false);
            } else {
                ElapsedTime = Math.Min(Timer / (float) CycleDuration, 1f);
                Time = Math.Clamp(Reverse ? (1f - ElapsedTime) : ElapsedTime, 0f, 1f);
                CycleUpdate(delta);
            }
        }

        public void Play(bool forceReset = true) {
            if (forceReset) {
                Reset();
            } else if (IsPaused) {
                Resume();
                return;
            }

            IsPlaying = true;
            IsPaused = false;
            OnStart?.Invoke();
        }

        public void Resume() {
            if (!IsPlaying || !IsPaused) {
                return;
            }

            IsPaused = false;
        }

        public void Pause() {
            if (!IsPlaying || IsPaused) {
                return;
            }

            IsPaused = true;
        }

        public void Reset() {
            IsPlaying = IsPaused = false;
            HasEnded = false;
            Reverse = _startReversed;
            Timer = 0;
            Time = Reverse ? 1f : 0f;
            ElapsedTime = 0f;
            PlayCount = 0;
            Reseted();
        }

        public void Finish(bool all = true) {
            if (HasEnded) {
                return;
            }

            OnCycleEnd?.Invoke();
            CycleEnd();

            if (PlayCount < RepeatTimes && !all) {
                if (PingPong) {
                    // alternate reverse state
                    _reverse = !_reverse;
                } else {
                    // keep reverse state
                }

                Timer = 0;
                ElapsedTime = 0f;
                Time = Reverse ? 0f : 1f;
                PlayCount += 1;
                CycleBegin();
                OnCycleBegin?.Invoke();
                return;
            }

            HasEnded = true;
            IsPlaying = IsPaused = false;
            Timer = CycleDuration;
            ElapsedTime = 1f;
            Reverse = RepeatTimes % 2 == 0 ? _startReversed : !_startReversed;
            Time = Reverse ? 0f : 1f;
            PlayCount = RepeatTimes;
            OnEnd?.Invoke();
        }

        public sealed override void Dispose() {
            if (IsDisposed) {
                return;
            }

            Disposed();
            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected abstract void Reseted();
        protected abstract void CycleBegin();
        protected abstract void CycleUpdate(int delta);
        protected abstract void CycleEnd();
        protected abstract void Disposed();

        #endregion Protected Methods
    }
}
