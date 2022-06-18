using Raccoon.Util.Tween;

namespace Raccoon.Components {
    public class TweenHandler : Component {
        private bool _pausedByControlGroup;

        public TweenHandler() {
        }

        public Tween Tween { get; private set; }

        /// <summary>
        /// Registers Tween at Tweener automatically when entering at scene and remove from it when removed from scene.
        /// </summary>
        public bool AutoRegister { get; set; } = true;

        public bool IsPlaying {
            get {
                return Tween != null && Tween.IsPlaying;
            }
        }

        public override void OnRemoved() {
            base.OnRemoved();
            Clear();
        }

        public override void OnSceneAdded() {
            base.OnSceneAdded();

            if (AutoRegister && Tween != null) {
                Play(false);
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

            if (Tween == null) {
                return;
            }

            if (Tween.CanDisposeWhenRemoved) {
                Tween.CanDisposeWhenRemoved = false; // to avoid disposing when it shouldn't
                Tweener.Remove(Tween);
                Tween.CanDisposeWhenRemoved = true;
            } else {
                Tweener.Remove(Tween);
            }
        }

        public override void Update(int delta) {
            if (Entity.Scene == null) {
                return;
            }

            if (Tween != null && Tween.HasEnded) {
                Tween = null;
            }
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
            Tween?.Pause();
        }

        public override void Resumed() {
            base.Resumed();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;
                Tween?.Play(false);
            }
        }

        public override void ControlGroupUnregistered() {
            base.ControlGroupUnregistered();

            if (_pausedByControlGroup) {
                _pausedByControlGroup = false;
                Tween?.Play();
            }
        }

        public void Reset() {
            if (Tween == null) {
                throw new System.InvalidOperationException("There is no Tween registered to reset.");
            }

            Tween.Reset();
        }

        public Tween Register(Tween tween) {
            if (tween == null) {
                throw new System.ArgumentNullException(nameof(tween));
            }

            if (Tween != null) {
                if (tween == Tween) {
                    return Tween;
                }

                Clear();
            }

            Tween = tween;
            return tween;
        }

        public Tween Play(bool forceReset = true) {
            if (Tween == null) {
                throw new System.InvalidOperationException("There is no Tween registered to play.");
            }

            Tweener.Play(Tween, forceReset);
            return Tween;
        }

        public Tween Play(Tween tween, bool forceReset = true) {
            Register(tween);
            Tweener.Play(Tween, forceReset);
            return Tween;
        }

        public void Pause() {
            if (Tween == null) {
                return;
            }

            Tween.Pause();
        }

        public void Clear() {
            if (Tween == null) {
                return;
            }

            if (!Tweener.Remove(Tween)) {
                Tween.Pause();

                if (Tween.CanDisposeWhenRemoved) {
                    Tween.Dispose();
                }
            }

            Tween = null;
        }
    }
}
