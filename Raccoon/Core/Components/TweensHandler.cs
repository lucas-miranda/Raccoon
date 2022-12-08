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
                _tweens.Lock();
                foreach (Tween tween in _tweens) {
                    if (tween.IsDisposed) {
                        _tweens.Remove(tween);
                        continue;
                    }

                    Tweener.Play(tween, false);
                }
                _tweens.Unlock();
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

            _tweens.Lock();
            foreach (Tween tween in _tweens) {
                if (tween.HasEnded || tween.IsDisposed) {
                    _tweens.Remove(tween);
                    continue;
                } else if (!Tweener.IsRegistered(tween)) {
                    continue;
                }

                InternalRemove(tween);
            }
            _tweens.Unlock();
        }

        public override void Update(int delta) {
            if (Entity.Scene == null) {
                return;
            }

            _tweens.Lock();
            foreach (Tween tween in _tweens) {
                if (tween.HasEnded || tween.IsDisposed) {
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
            } else if (_tweens.Contains(tween)) {
                return tween;
            }

            _tweens.Add(tween);
            return tween;
        }

        public bool Unregister(Tween tween) {
            if (_tweens.Remove(tween)) {
                InternalRemove(tween);
                return true;
            }

            return false;
        }

        public Tween Play(Tween tween, bool forceReset = true) {
            Register(tween);
            Tweener.Play(tween, forceReset);
            return tween;
        }

        public bool Remove(Tween tween) {
            if (!_tweens.Contains(tween)) {
                return false;
            }

            InternalRemove(tween);
            return true;
        }

        public void Clear(bool canForceDispose = false) {
            foreach (Tween tween in _tweens) {
                if (!Tweener.Remove(tween)) {
                    tween.Pause();

                    if (canForceDispose || tween.CanDisposeWhenRemoved) {
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

        public void PauseAll() {
            foreach (Tween tween in _tweens) {
                tween.Pause();
            }
        }

        public void ResumeAll() {
            if (_pausedByControlGroup) {
                return;
            }

            foreach (Tween tween in _tweens) {
                tween.Resume();
            }
        }

        public void ResetAll() {
            foreach (Tween tween in _tweens) {
                tween.Reset();
            }
        }

        public bool Contains(Tween tween) {
            return _tweens.Contains(tween);
        }

        #endregion Public Methods

        #region Private Methods

        public void InternalRemove(Tween tween) {
            // remove tween but don't dispose it
            if (tween.CanDisposeWhenRemoved) {
                tween.CanDisposeWhenRemoved = false; // to avoid disposing when it shouldn't
                Tweener.Remove(tween);
                tween.CanDisposeWhenRemoved = true;
            } else {
                Tweener.Remove(tween);
            }
        }

        #endregion Private Methods
    }
}
