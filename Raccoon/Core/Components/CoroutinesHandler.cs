using System.Collections;

using Raccoon.Util.Collections;

namespace Raccoon.Components {
    public class CoroutinesHandler : Component {
        #region Private Members

        public Locker<Coroutine> _coroutines = new Locker<Coroutine>();

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
            } else {
                foreach (Coroutine coroutine in _coroutines) {
                    Coroutines.Instance.Remove(coroutine);
                }
            }
        }

        public override void Update(int delta) {
            if (Entity.Scene == null) {
                return;
            }

            foreach (Coroutine coroutine in _coroutines) {
                if (coroutine.HasEnded) {
                    _coroutines.Remove(coroutine);
                }
            }
        }

        public override void Render() {
        }

        public override void DebugRender() {
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
            coroutine.Stop();
            return _coroutines.Remove(coroutine);
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
            foreach (Coroutine coroutine in _coroutines) {
                coroutine.Resume();
            }
        }

        #endregion Public Methods
    }
}
