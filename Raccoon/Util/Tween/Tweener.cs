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
            Tween tween = Add(new Tween(subject, duration, additional: additional));

            return tween;
        }

        public static Tween Add(Tween tween) {
            Instance._tweens.Add(tween);
            return tween;
        }

        public static bool Remove(Tween tween) {
            if (Instance._tweens.Remove(tween)) {
                tween.Pause();
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

            foreach (Tween tween in _tweens) {
                tween.Update(delta);

                if (tween.HasEnded) {
                    _tweens.Remove(tween);
                }
            }
        }

        #endregion Internal Methods
    }
}
