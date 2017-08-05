using System;
using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public class Coroutine {
        private static readonly Lazy<Coroutine> _lazy = new Lazy<Coroutine>(() => new Coroutine());
        private List<int> _runningRoutines = new List<int>();
        private Dictionary<int, IEnumerator> _routines = new Dictionary<int, IEnumerator>();
        private List<object> _tokens = new List<object>();
        private int _nextId;

        private Coroutine() {
        }

        public static Coroutine Instance { get { return _lazy.Value; } }

        public int Count { get { return _routines.Count; } }

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

            for (int i = _runningRoutines.Count - 1; i >= 0; i--) {
                int routineId = _runningRoutines[i];
                if (MoveNext(_routines[routineId])) {
                    continue;
                } else {
                    _routines.Remove(routineId);
                    _runningRoutines.Remove(routineId);
                }

                if (_runningRoutines.Count <= i) {
                    i = _runningRoutines.Count - 1;
                }
            }
        }

        public int Start(IEnumerator routine) {
            _routines.Add(_nextId, routine);
            _runningRoutines.Insert(0, _nextId);
            return _nextId++;
        }

        public void Pause(int id) {
            if (!_routines.ContainsKey(id) || !_runningRoutines.Contains(id)) {
                return;
            }

            _runningRoutines.Remove(id);
        }

        public void Resume(int id) {
            if (!_routines.ContainsKey(id) || _runningRoutines.Contains(id)) {
                return;
            }

            _runningRoutines.Add(id);
        }

        public void Stop(int id) {
            if (_routines.ContainsKey(id)) {
                _routines.Remove(id);
            }

            int i = _runningRoutines.IndexOf(id);
            if (i != -1) {
                _runningRoutines.RemoveAt(i);
            }
        }

        public void StopAll() {
            _routines.Clear();
            _runningRoutines.Clear();
        }

        public bool IsRunning(int id) {
            return _runningRoutines.Contains(id);
        }

        public bool Exist(int id) {
            return _routines.ContainsKey(id);
        }

        public void EmitToken(object token) {
            _tokens.Add(token);
        }

        public void ConsumeToken(object token) {
            _tokens.Remove(token);
        }

        public bool HasToken(object token) {
            return _tokens.Contains(token);
        }
    }
}
