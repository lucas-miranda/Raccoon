using System;
using System.Collections;

using Raccoon.Util.Collections;

namespace Raccoon {
    public class Coroutines {
        #region Private Members

        private static readonly Lazy<Coroutines> _lazy = new Lazy<Coroutines>(() => new Coroutines());

        private Locker<Coroutine> _runningCoroutines = new Locker<Coroutine>(), 
                                  _pausedCoroutines = new Locker<Coroutine>();

        #endregion Private Members

        #region Constructors

        private Coroutines() {
        }

        #endregion Constructors

        #region Public Static Properties

        public static Coroutines Instance { get { return _lazy.Value; } }

        #endregion Public Static Properties

        #region Public Properties

        public int RunningCount { get { return _runningCoroutines.Count; } }
        public int PausedCount { get { return _pausedCoroutines.Count; } }
        public int TotalCount { get { return RunningCount + PausedCount; } }

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            foreach (Coroutine coroutine in _pausedCoroutines) {
                if (coroutine.IsRunning) {
                    _pausedCoroutines.Remove(coroutine);
                    _runningCoroutines.Add(coroutine);
                }
            }

            foreach (Coroutine coroutine in _runningCoroutines) {
                coroutine.Update(delta);

                if (coroutine.HasEnded) {
                    _runningCoroutines.Remove(coroutine);
                } else if (!coroutine.IsRunning) {
                    _runningCoroutines.Remove(coroutine);
                    _pausedCoroutines.Add(coroutine);
                }
            }

            _runningCoroutines.Upkeep();
            _pausedCoroutines.Upkeep();
        }

        public Coroutine Start(IEnumerator coroutine) {
            Coroutine c = new Coroutine(coroutine);
            _runningCoroutines.Add(c);
            return c;
        }

        public Coroutine Start(Func<IEnumerator> coroutine) {
            return Start(coroutine());
        }

        public void ClearAll() {
            _runningCoroutines.Clear();
            _pausedCoroutines.Clear();
        }

        #endregion Public Methods

        #region Class Coroutine

        public class Coroutine {
            public Coroutine(IEnumerator enumerator) {
                Enumerator = enumerator;
            }

            public IEnumerator Enumerator { get; private set; }
            public bool IsRunning { get; private set; } = true;
            public bool HasEnded { get; private set; }
            public uint Timer { get; private set; }
            public int DelayInterval { get; private set; }

            public void Update(int delta) {
                if (!IsRunning || HasEnded) {
                    return;
                }

                Timer += (uint) delta;

                // must wait 'till delay interval ends
                if (DelayInterval > 0) {
                    DelayInterval = Math.Max(0, DelayInterval - delta);
                    return;
                }

                if (!MoveNext(Enumerator)) {
                    Stop();
                }
            }

            public void Resume() {
                if (HasEnded || IsRunning) {
                    return;
                }

                IsRunning = true;
            }

            public void Pause() {
                if (HasEnded || !IsRunning) {
                    return;
                }

                IsRunning = false;
            }

            public void Stop() {
                HasEnded = true;
                IsRunning = false;
            }

            public void Wait(int mili) {
                if (HasEnded) {
                    return;
                }

                DelayInterval += mili;
            }

            public void Wait(uint mili) {
                Wait((int) mili);
            }

            public void Wait(float seconds) {
                Wait((uint) (seconds * Util.Time.SecToMili));
            }

            public void Reset() {
                Enumerator.Reset();
                HasEnded = false;
                IsRunning = true;
            }

            private bool MoveNext(IEnumerator enumerator) {
                // checks if need to run a nested coroutine
                if (enumerator.Current is IEnumerator && MoveNext(enumerator.Current as IEnumerator)) {
                    return true;
                }

                // special Current values
                switch (enumerator.Current) {
                    case float seconds:
                        Wait(seconds);
                        return true;

                    case int miliI:
                        Wait(miliI);
                        return true;

                    case uint miliU:
                        Wait(miliU);
                        return true;

                    default:
                        break;
                }

                return enumerator.MoveNext();
            }
        }

        #endregion Class Coroutine
    }
}
