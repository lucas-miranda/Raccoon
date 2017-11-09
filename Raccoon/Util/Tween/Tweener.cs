using System.Collections.Generic;

namespace Raccoon.Util.Tween {
    public class Tweener {
        #region Private Static Readonly Members

        private static readonly Tweener _instance = new Tweener();

        #endregion Private Static Readonly Members

        #region Private Members

        private List<Tween> _tweens, _toAdd, _toRemove;

        #endregion Private Members

        #region Constructors

        private Tweener() {
            _tweens = new List<Tween>();
            _toAdd = new List<Tween>();
            _toRemove = new List<Tween>();
        }

        #endregion Constructors

        #region Public Static Properties

        public static Tweener Instance { get { return _instance; } }

        #endregion Public Static Properties

        #region Public Static Methods

        public static Tween Create<T>(T subject, int duration) where T : class {
            Tween t = new Tween(subject, duration);
            Instance.Add(t);
            return t;
        }

        #endregion Public Static Methods

        #region Public Methods

        public void Start() {
            foreach (Tween tween in _tweens) {
                tween.Play();
            }
        }

        public void Update(int delta) {
            if (_toAdd.Count > 0) {
                _tweens.AddRange(_toAdd);
                _toAdd.Clear();
            }

            foreach (Tween tween in _tweens) {
                tween.Update(delta);
                if (tween.HasEnded) {
                    _toRemove.Add(tween);
                }
            }

            if (_toRemove.Count > 0) {
                foreach (Tween tween in _toRemove) {
                    _tweens.Remove(tween);
                }

                _toRemove.Clear();
            }
        }

        public void Add(Tween tween) {
            _toAdd.Add(tween);
        }

        public void Remove(Tween tween) {
            _toRemove.Add(tween);
        }

        public void Play(Tween tween, bool forceReset = true) {
            if (!_tweens.Contains(tween)) {
                _tweens.Add(tween);
            }

            tween.Play(forceReset);
        }

        #endregion Public Methods
    }
}
