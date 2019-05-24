using Raccoon.Graphics;

namespace Raccoon.Util.Graphics {
    public class Particle : Entity {
        private Animation _animation;
        private uint _timeToEnd, _duration, _delay;

        public Particle() {
        }

        public bool IsRunning { get; private set; }

        public Animation Animation {
            get {
                return _animation;
            }

            set {
                Graphic = _animation = value;
                Graphic.Visible = false;
            }
        }

        public override void Update(int delta) {
            base.Update(delta);
            if (Scene == null) {
                return;
            }

            if (!IsRunning) {
                if (Timer >= _delay) {
                    Run();
                }
            } else if (Timer >= _timeToEnd) {
                RemoveSelf();
            }
        }

        public void Prepare(uint duration, uint delay, string animationKey) {
            _duration = duration;

            if (Animation != null) {
                Animation.Play(animationKey);
                Animation.Pause();

                if (_duration != 0) {
                    Animation.PlaybackSpeed = _duration / (float) Animation.CurrentTrack.TotalDuration;
                } else {
                    _duration = (uint) Animation.CurrentTrack.TotalDuration;
                }
            }

            _delay = delay;
        }

        private void Run() {
            if (IsRunning) {
                return;
            }

            IsRunning = true;
            Graphic.Visible = true;
            _timeToEnd = Timer + _duration;

            if (Animation != null) {
                Animation.Play();
            }
        }
    }
}
