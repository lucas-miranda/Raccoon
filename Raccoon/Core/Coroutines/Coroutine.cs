using System.Collections;

using Raccoon.Util;

namespace Raccoon {
    public class Coroutine {
        private bool _waitingDelay;

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
            _waitingDelay = true;
        }

        public void Wait(uint mili) {
            Wait((int) mili);
        }

        public void Wait(float seconds) {
            Wait((int) (seconds * Util.Time.SecToMili));
        }

        private bool MoveNext(IEnumerator enumerator) {
            if (enumerator == null) {
                return false;
            }

            // checks if need to run a nested coroutine
            if (enumerator.Current is IEnumerator && MoveNext(enumerator.Current as IEnumerator)) {
                return true;
            }

            // special Current values
            switch (enumerator.Current) {
                case null:
                    break;

                case float seconds:
                    if (_waitingDelay) {
                        _waitingDelay = false;
                        break;
                    }

                    Wait(seconds);
                    return true;

                case int miliI:
                    if (_waitingDelay) {
                        _waitingDelay = false;
                        break;
                    }

                    Wait(miliI);
                    return true;

                case uint miliU:
                    if (_waitingDelay) {
                        _waitingDelay = false;
                        break;
                    }

                    Wait(miliU);
                    return true;

                default:
                    break;
            }

            return enumerator.MoveNext();
        }
    }
}
