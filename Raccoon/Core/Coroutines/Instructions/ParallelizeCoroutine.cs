using System.Collections;

namespace Raccoon {
    public class ParallelizeCoroutine : CoroutineInstruction {
        private int _currentRoutineIndex;

        public ParallelizeCoroutine(params IEnumerator[] routines) {
            Routines = routines;
            IsRoutinesAlive = new bool[routines.Length];
            for (int i = 0; i < routines.Length; i++) {
                IsRoutinesAlive[i] = true;
            }
        }

        public IEnumerator[] Routines { get; private set; }
        public bool[] IsRoutinesAlive { get; private set; }
        public override object Current { get { return null; } }

        public override IEnumerator RetrieveRoutine() {
            for (_currentRoutineIndex = 0; _currentRoutineIndex < Routines.Length; _currentRoutineIndex++) {
                if (!IsRoutinesAlive[_currentRoutineIndex]) {
                    continue;
                }

                yield return Routines[_currentRoutineIndex];
            }

            yield return Signal.Continue;
        }

        public override bool MoveNext() {
            foreach (bool isRoutineAlive in IsRoutinesAlive) {
                if (isRoutineAlive) {
                    return true;
                }
            }

            return false;
        }

        public override void RoutineMoveNextCallback(bool moveNextRet) {
            if (!moveNextRet) {
                IsRoutinesAlive[_currentRoutineIndex] = false;
            }
        }
    }
}
