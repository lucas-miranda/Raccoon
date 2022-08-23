using System.Collections;

using Raccoon.Util.Collections;

namespace Raccoon.Components {
    public class CoroutinesHandler : Component {
        #region Private Members

        private Locker<Coroutine> _coroutines = new Locker<Coroutine>();
        private bool _pausedByControlGroup;

        #endregion Private Members

        #region Constructors

        public CoroutinesHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public int CoroutinesCount { get { return _coroutines.Count; } }

        #endregion Public Properties

        #region Public Methods

        public override void OnRemoved() {
            base.OnRemoved();
            Clear();
        }

        public override void OnSceneAdded() {
            base.OnSceneAdded();

            foreach (Coroutine coroutine in _coroutines) {
                Coroutines.Instance.Start(coroutine);
            }
        }

        public override void OnSceneRemoved(bool wipe) {
            base.OnSceneRemoved(wipe);

            if (wipe) {
                Clear();
                return;
            }

            _coroutines.Lock();
            foreach (Coroutine coroutine in _coroutines) {
                Coroutines.Instance.Remove(coroutine);

                if (coroutine.IsRunning && coroutine.HasEnded) {
                    _coroutines.Remove(coroutine);
                }
            }
            _coroutines.Unlock();
        }

        public override void Update(int delta) {
            if (Entity.Scene == null) {
                return;
            }

            _coroutines.Lock();
            foreach (Coroutine coroutine in _coroutines) {
                if (coroutine.HasEnded) {
                    _coroutines.Remove(coroutine);
                }
            }
            _coroutines.Unlock();
        }

        public override void Render() {
        }

        public override void DebugRender() {
        }

        public override void Paused() {
            base.Paused();
            _pausedByControlGroup = true;

            foreach (Coroutine coroutine in _coroutines) {
                coroutine.Pause();
            }
        }

        public override void Resumed() {
            base.Resumed();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;

                foreach (Coroutine coroutine in _coroutines) {
                    coroutine.Resume();
                }
            }
        }

        public override void ControlGroupUnregistered() {
            base.ControlGroupUnregistered();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;

                foreach (Coroutine coroutine in _coroutines) {
                    coroutine.Resume();
                }
            }
        }

        public Coroutine Start(IEnumerator coroutine) {
            return RegisterCoroutine(Coroutines.Instance.Start(coroutine));
        }

        public Coroutine Start(System.Func<IEnumerator> coroutine) {
            return RegisterCoroutine(Coroutines.Instance.Start(coroutine));
        }

        public Coroutine Start(Coroutine coroutine) {
            return RegisterCoroutine(Coroutines.Instance.Start(coroutine));
        }

        public Coroutine RegisterCoroutine(Coroutine coroutine) {
            _coroutines.Add(coroutine);
            return coroutine;
        }

        public bool RemoveCoroutine(Coroutine coroutine) {
            if (_coroutines.Remove(coroutine)) {
                coroutine.Stop();
                return true;
            }

            return false;
        }

        public void Clear() {
            foreach (Coroutine c in _coroutines) {
                c.Stop();
            }

            _coroutines.Clear();
        }

        public void PauseAll() {
            foreach (Coroutine coroutine in _coroutines) {
                coroutine.Pause();
            }
        }

        public void ResumeAll() {
            if (_pausedByControlGroup) {
                return;
            }

            foreach (Coroutine coroutine in _coroutines) {
                coroutine.Resume();
            }
        }

        #endregion Public Methods
    }
}
