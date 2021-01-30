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

        /// <summary>
        /// Force Coroutine to take a step foward.
        /// </summary>
        public void Step() {
            if (!IsRunning || HasEnded || DelayInterval > 0) {
                return;
            }

            if (!MoveNext(Enumerator)) {
                Stop();
            }
        }

        private bool MoveNext(IEnumerator enumerator) {
            if (enumerator == null) {
                return false;
            }

            // checks if need to run a nested coroutine
            if (enumerator.Current is IEnumerator nestedEnumerator && !(enumerator.Current is CoroutineInstruction)) {
                if (MoveNext(nestedEnumerator)) {
                    return true;
                }
            } else {
                // special Current values
                if (enumerator.Current != null) {
                    if (enumerator.Current is CoroutineInstruction instruction) {
                        if (instruction.MoveNext()) {
                            IEnumerator instructionEnumerator = instruction.RetrieveRoutine();

                            if (instructionEnumerator == null) {
                                return true;
                            }

                            bool isRunningInstruction = true;

                            do {
                                instructionEnumerator.MoveNext();

                                if (instructionEnumerator.Current is CoroutineInstruction.Signal signal) {
                                    if (signal == CoroutineInstruction.Signal.Continue) {
                                        isRunningInstruction = false;
                                    }
                                } else if (instructionEnumerator.Current is IEnumerator instructionInternalEnumerator) {
                                    instruction.RoutineMoveNextCallback(MoveNext(instructionInternalEnumerator));
                                }
                            } while (isRunningInstruction);
                            
                            return true;
                        }
                    } else if (enumerator.Current is float seconds) {
                        if (!_waitingDelay) {
                            Wait(seconds);
                            return true;
                        }

                        _waitingDelay = false;
                    } else if (enumerator.Current is int miliI) {
                        if (!_waitingDelay) {
                            Wait(miliI);
                            return true;
                        }

                        _waitingDelay = false;
                    } else if (enumerator.Current is uint miliU) {
                        if (_waitingDelay) {
                            Wait(miliU);
                            return true;
                        }

                        _waitingDelay = false;
                    }
                }
            }

            return enumerator.MoveNext();
        }
    }
}
