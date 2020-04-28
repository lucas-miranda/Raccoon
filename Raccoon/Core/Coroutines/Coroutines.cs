using System.Collections;

using Raccoon.Util.Collections;

namespace Raccoon {
    public class Coroutines {
        #region Private Members

        private static readonly System.Lazy<Coroutines> _lazy = new System.Lazy<Coroutines>(() => new Coroutines());

        private Locker<Coroutine> _runningCoroutines = new Locker<Coroutine>(),
                                  _pausedCoroutines = new Locker<Coroutine>();

        #endregion Private Members

        #region Constructors

        private Coroutines() {
            IsRunning = true;
        }

        #endregion Constructors

        #region Public Static Properties

        public static Coroutines Instance { get { return _lazy.Value; } }
        public static bool IsRunning { get; set; }

        #endregion Public Static Properties

        #region Public Properties

        public int RunningCount { get { return _runningCoroutines.Count; } }
        public int PausedCount { get { return _pausedCoroutines.Count; } }
        public int TotalCount { get { return RunningCount + PausedCount; } }

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            if (!IsRunning) {
                return;
            }

            _pausedCoroutines.Lock();
            foreach (Coroutine coroutine in _pausedCoroutines) {
                if (coroutine.IsRunning) { // checks if a coroutine it's running again
                    _pausedCoroutines.Remove(coroutine);
                    _runningCoroutines.Add(coroutine);
                }
            }
            _pausedCoroutines.Unlock();

            _runningCoroutines.Lock();
            foreach (Coroutine coroutine in _runningCoroutines) {
                coroutine.Update(delta);

                if (coroutine.HasEnded) { // remove ended coroutine
                    _runningCoroutines.Remove(coroutine);
                } else if (!coroutine.IsRunning) { // move paused coroutine to the proper list
                    _runningCoroutines.Remove(coroutine);
                    _pausedCoroutines.Add(coroutine);
                }
            }
            _runningCoroutines.Unlock();
        }

        public Coroutine Start(IEnumerator coroutine) {
            Coroutine c = new Coroutine(coroutine);
            _runningCoroutines.Add(c);
            return c;
        }

        public Coroutine Start(System.Func<IEnumerator> coroutine) {
            return Start(coroutine());
        }

        public Coroutine Start(Coroutine coroutine) {
            if (_runningCoroutines.Contains(coroutine)) {
                return coroutine;
            } else if (_pausedCoroutines.Contains(coroutine)) {
                coroutine.Resume();
                return coroutine;
            }

            _runningCoroutines.Add(coroutine);
            return coroutine;
        }

        public bool Remove(Coroutine coroutine) {
            bool removedFromRunning = _runningCoroutines.Remove(coroutine),
                 removedFromPaused = _pausedCoroutines.Remove(coroutine);

            return removedFromRunning || removedFromPaused;
        }

        public void ClearAll() {
            foreach (Coroutine coroutine in _runningCoroutines) {
                coroutine.Stop();
            }

            foreach (Coroutine coroutine in _pausedCoroutines) {
                coroutine.Stop();
            }

            _runningCoroutines.Clear();
            _pausedCoroutines.Clear();
        }

        #endregion Public Methods
    }
}
