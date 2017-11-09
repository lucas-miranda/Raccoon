using System;
using System.Collections;
using System.Collections.Generic;

namespace Raccoon {
    public class Coroutine {
        #region Private Members

        private static readonly Lazy<Coroutine> _lazy = new Lazy<Coroutine>(() => new Coroutine());

        private List<int> _runningRoutines = new List<int>();
        private Dictionary<int, IEnumerator> _routines = new Dictionary<int, IEnumerator>();
        private List<object> _tokens = new List<object>();
        private int _nextId;

        #endregion Private Members

        #region Constructors

        private Coroutine() {
        }

        #endregion Constructors

        #region Public Static Properties

        public static Coroutine Instance { get { return _lazy.Value; } }

        #endregion Public Static Properties

        #region Public Properties

        public int Count { get { return _routines.Count; } }

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            foreach (int routineId in _runningRoutines) {
                if (MoveNext(_routines[routineId])) {
                    continue;
                }

                _routines.Remove(routineId);
                _runningRoutines.Remove(routineId);
            }
        }

        public int Start(IEnumerator routine) {
            _routines.Add(_nextId, routine);
            _runningRoutines.Insert(0, _nextId);
            return _nextId++;
        }

        public void Pause(int id) {
            if (!Exists(id) || !IsRunning(id)) {
                return;
            }

            _runningRoutines.Remove(id);
        }

        public void Resume(int id) {
            if (!Exists(id) || IsRunning(id)) {
                return;
            }

            _runningRoutines.Add(id);
        }

        public void Stop(int id) {
            if (Exists(id)) {
                _routines.Remove(id);
            }

            _runningRoutines.Remove(id);
        }

        public void StopAll() {
            _routines.Clear();
            _runningRoutines.Clear();
        }

        public bool IsRunning(int id) {
            return _runningRoutines.Contains(id);
        }

        public bool Exists(int id) {
            return _routines.ContainsKey(id);
        }

        public void EmitToken(object token) {
            _tokens.Add(token);
        }

        public bool ConsumeToken(object token) {
            return _tokens.Remove(token);
        }

        public bool HasToken(object token) {
            return _tokens.Contains(token);
        }

        #endregion Public Methods

        #region Private Methods
        
        private bool MoveNext(IEnumerator routine) {
            // checks if need to run a sub-routine
            if (routine.Current is IEnumerator && MoveNext(routine.Current as IEnumerator)) {
                return true;
            }

            return routine.MoveNext();
        }

        #endregion Private Methods
    }
}
