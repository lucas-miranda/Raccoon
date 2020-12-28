using Raccoon.Util.Collections;

namespace Raccoon.Util.Tween {
    public class Tweener {
        #region Private Members

        private Locker<Tween> _tweens = new Locker<Tween>();

        #endregion Private Members

        #region Constructors

        private Tweener() {
        }

        #endregion Constructors

        #region Public Static Properties

        public static Tweener Instance { get; } = new Tweener();
        public static bool IsRunning { get; set; } = true;

        #endregion Public Static Properties

        #region Public Methods

        public static Tween Create<T>(T subject, int duration, bool additional = false) where T : class {
            return Add(new Tween(subject, duration, additional: additional));
        }

        public static Tween Add(Tween tween) {
            Instance._tweens.Add(tween);
            return tween;
        }

        public static bool Remove(Tween tween) {
            if (Instance._tweens.Remove(tween)) {
                tween.Pause();

                if (tween.CanDisposeWhenRemoved) {
                    tween.Dispose();
                }

                return true;
            }

            return false;
        }

        public static void Play(Tween tween, bool forceReset = true) {
            if (!Instance._tweens.Contains(tween)) {
                Instance._tweens.Add(tween);
            }

            tween.Play(forceReset);
        }

        public static void Clear() {
            foreach (Tween tween in Instance._tweens) {
                if (tween.CanDisposeWhenRemoved) {
                    tween.Dispose();
                }
            }
            Instance._tweens.Clear();
        }

        #endregion Public Methods

        #region Internal Methods

        internal void Start() {
            foreach (Tween tween in _tweens) {
                tween.Play();
            }
        }

        internal void Update(int delta) {
            if (!IsRunning) {
                return;
            }

            _tweens.Lock();
            foreach (Tween tween in _tweens) {
                tween.Update(delta);

                if (tween.HasEnded) {
                    _tweens.Remove(tween);
                    if (tween.CanDisposeWhenRemoved) {
                        tween.Dispose();
                    }
                }
            }
            _tweens.Unlock();
        }

        #endregion Internal Methods
    }
}
