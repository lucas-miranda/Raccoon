using System.Collections;

namespace Raccoon.Components {
    public class CoroutineHandler : Component {
        private bool _pausedByControlGroup;

        public CoroutineHandler() {
        }

        public Coroutine Coroutine { get; private set; }

        public bool IsRunning {
            get {
                return Coroutine != null && Coroutine.IsRunning;
            }
        }

        public override void OnRemoved() {
            base.OnRemoved();
            Clear();
        }

        public override void OnSceneAdded() {
            base.OnSceneAdded();
            if (Coroutine != null) {
                Coroutines.Instance.Start(Coroutine);
            }
        }

        public override void OnSceneRemoved(bool wipe) {
            base.OnSceneRemoved(wipe);
            if (wipe) {
                Clear();
            } else {
                Coroutines.Instance.Remove(Coroutine);
            }
        }

        public override void Update(int delta) {
            if (Entity.Scene == null) {
                return;
            }

            if (Coroutine != null && Coroutine.HasEnded) {
                Coroutine = null;
            }
        }

        public override void Render() {
        }

        public override void DebugRender() {
        }

        public override void Paused() {
            base.Paused();
            _pausedByControlGroup = true;
            Coroutine?.Pause();
        }

        public override void Resumed() {
            base.Resumed();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;
                Coroutine?.Resume();
            }
        }

        public override void ControlGroupUnregistered() {
            base.ControlGroupUnregistered();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;
                Coroutine?.Resume();
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

        public void Stop() {
            if (Coroutine == null) {
                return;
            }

            Coroutine.Stop();
            Coroutine = null;
        }

        public void Pause() {
            if (Coroutine == null) {
                return;
            }

            Coroutine.Pause();
        }

        public void Resume() {
            if (Coroutine == null || _pausedByControlGroup) {
                return;
            }

            Coroutine.Resume();
        }

        public void Clear() {
            if (Coroutine == null) {
                return;
            }

            Coroutines.Instance.Remove(Coroutine);
            Stop();
        }

        private Coroutine RegisterCoroutine(Coroutine coroutine) {
            if (Coroutine != null) {
                Coroutine.Stop();
            }

            Coroutine = coroutine;
            return Coroutine;
        }
    }
}
