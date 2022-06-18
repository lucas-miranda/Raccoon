using Raccoon.Util.Tween;
using Raccoon.Util.Collections;

namespace Raccoon.Components {
    public class TweensHandler : Component {
        #region Private Members

        private Locker<Tween> _tweens = new Locker<Tween>();
        private bool _pausedByControlGroup;

        #endregion Private Members

        #region Constructors

        public TweensHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public int TweensCount { get { return _tweens.Count; } }

        /// <summary>
        /// Registers Tween at Tweener automatically when entering at scene and remove from it when removed from scene.
        /// </summary>
        public bool AutoRegister { get; set; } = true;

        #endregion Public Properties

        #region Public Methods

        public override void OnRemoved() {
            base.OnRemoved();
            Clear();
        }

        public override void OnSceneAdded() {
            base.OnSceneAdded();

            if (AutoRegister) {
                foreach (Tween tween in _tweens) {
                    PlayTween(tween, false);
                }
            }
        }

        public override void OnSceneRemoved(bool wipe) {
            base.OnSceneRemoved(wipe);

            if (!AutoRegister) {
                return;
            }

            if (wipe) {
                Clear();
                return;
            }

            foreach (Tween tween in _tweens) {
                if (tween.CanDisposeWhenRemoved) {
                    tween.CanDisposeWhenRemoved = false; // to avoid disposing when it shouldn't
                    RemoveTween(tween);
                    tween.CanDisposeWhenRemoved = true;
                } else {
                    RemoveTween(tween);
                }
            }
        }

        public override void Update(int delta) {
            if (Entity.Scene == null) {
                return;
            }

            _tweens.Lock();
            foreach (Tween tween in _tweens) {
                if (tween.HasEnded) {
                    _tweens.Remove(tween);
                }
            }
            _tweens.Unlock();
        }

        public override void Render() {
        }

#if DEBUG
        public override void DebugRender() {
        }
#endif

        public override void Paused() {
            base.Paused();
            _pausedByControlGroup = true;

            foreach (Tween tween in _tweens) {
                tween.Pause();
            }
        }

        public override void Resumed() {
            base.Resumed();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;

                foreach (Tween tween in _tweens) {
                    tween.Play(false);
                }
            }
        }

        public override void ControlGroupUnregistered() {
            base.ControlGroupUnregistered();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;

                foreach (Tween tween in _tweens) {
                    tween.Play(false);
                }
            }
        }

        public Tween Register(Tween tween) {
            if (tween == null) {
                throw new System.ArgumentNullException(nameof(tween));
            }

            _tweens.Add(tween);
            return tween;
        }

        public Tween Play(Tween tween, bool forceReset = true) {
            Register(tween);
            Tweener.Play(tween, forceReset);
            return tween;
        }

        public void Clear() {
            foreach (Tween tween in _tweens) {
                if (!Tweener.Remove(tween)) {
                    tween.Pause();

                    if (tween.CanDisposeWhenRemoved) {
                        tween.Dispose();
                    }
                }
            }

            _tweens.Clear();
        }

        public void PlayAll(bool forceReset = true) {
            if (_pausedByControlGroup) {
                return;
            }

            foreach (Tween tween in _tweens) {
                tween.Play(forceReset);
            }
        }

        public void ResetAll() {
            foreach (Tween tween in _tweens) {
                tween.Reset();
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void PlayTween(Tween tween, bool forceReset) {
            Tweener.Play(tween, forceReset);
        }

        private void RemoveTween(Tween tween) {
            Tweener.Remove(tween);
        }

        #endregion Private Methods
    }
}
