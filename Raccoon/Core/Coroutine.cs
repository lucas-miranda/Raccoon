using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public class Coroutine {
        private static Coroutine instance;
        private List<int> runningRoutines;
        private Dictionary<int, IEnumerator> routines;
        private List<object> tokens;
        private int nextId;

        private Coroutine() {
            routines = new Dictionary<int, IEnumerator>();
            runningRoutines = new List<int>();
            tokens = new List<object>();
        }

        public static Coroutine Instance {
            get {
                if (instance == null) {
                    instance = new Coroutine();
                }

                return instance;
            }
        }

        public int Count { get { return routines.Count; } }

        private bool MoveNext(IEnumerator routine) {
            if (routine.Current is IEnumerator) {
                if (MoveNext(routine.Current as IEnumerator)) {
                    return true;
                }
            }

            return routine.MoveNext();
        }

        public void Update(int delta) {
            if (Count == 0) {
                return;
            }

            for (int i = runningRoutines.Count - 1; i >= 0; i--) {
                int routineId = runningRoutines[i];
                if (MoveNext(routines[routineId])) {
                    continue;
                } else {
                    routines.Remove(routineId);
                    runningRoutines.Remove(routineId);
                }

                if (runningRoutines.Count <= i) {
                    i = runningRoutines.Count - 1;
                }
            }
        }

        public int Start(IEnumerator routine) {
            routines.Add(nextId, routine);
            runningRoutines.Insert(0, nextId);
            return nextId++;
        }

        public void Pause(int id) {
            if (!routines.ContainsKey(id) || !runningRoutines.Contains(id)) {
                return;
            }

            runningRoutines.Remove(id);
        }

        public void Resume(int id) {
            if (!routines.ContainsKey(id) || runningRoutines.Contains(id)) {
                return;
            }

            runningRoutines.Add(id);
        }

        public void Stop(int id) {
            if (routines.ContainsKey(id)) {
                routines.Remove(id);
            }

            int i = runningRoutines.IndexOf(id);
            if (i != -1) {
                runningRoutines.RemoveAt(i);
            }
        }

        public void StopAll() {
            routines.Clear();
            runningRoutines.Clear();
        }

        public bool IsRunning(int id) {
            return runningRoutines.Contains(id);
        }

        public bool Exist(int id) {
            return routines.ContainsKey(id);
        }

        public void EmitToken(object token) {
            tokens.Add(token);
        }

        public void ConsumeToken(object token) {
            tokens.Remove(token);
        }

        public bool HasToken(object token) {
            return tokens.Contains(token);
        }
    }
}
