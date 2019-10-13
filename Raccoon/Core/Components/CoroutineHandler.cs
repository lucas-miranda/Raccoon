using System.Collections;

namespace Raccoon.Components {
    public class CoroutineHandler : Component {
        public Coroutine Coroutine { get; private set; }
        public bool IsRunning {
            get {
                if (Coroutine == null) {
                    return false;
                }

                return Coroutine.IsRunning;
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
            if (Coroutine == null) {
                return;
            }

            Coroutine.Resume();
        }

        public void Clear() {
            Stop();
            Coroutine = null;
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
