using Raccoon.Graphics;

namespace Raccoon.Util.Graphics {
    public class Particle : Entity {
        #region Private Members

        private Animation _animation;
        private uint _timeToEnd, _duration, _delay;

        // simple movement
        private bool _isMovementEnabled;
        private Vector2 _movementDirection, _velocity, _maxVelocity, _acceleration;

        #endregion Private Members

        #region Constructors

        public Particle() {
        }

        #endregion Constructors

        #region Public Properties

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

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
            base.Update(delta);
            if (Scene == null) {
                return;
            }

            if (!IsRunning) {
                if (Timer < _delay) {
                    return;
                }

                Run();
            } else if (Timer >= _timeToEnd) {
                RemoveSelf();
                return;
            }

            if (_isMovementEnabled) {
                float dt = delta / 1000f; 
                Vector2 v = _velocity;

                if (_maxVelocity.X <= 0f) {
                    v.X += _acceleration.X * dt;
                } else {
                    v.X = Math.Approach(v.X, _maxVelocity.X, _acceleration.X * dt);
                }

                if (_maxVelocity.Y <= 0f) {
                    v.Y += _acceleration.Y * dt;
                } else {
                    v.Y = Math.Approach(v.Y, _maxVelocity.Y, _acceleration.Y * dt);
                }

                _velocity = v;

                Vector2 displacement = _movementDirection * _velocity * dt;
                Transform.Position += displacement;
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

        public void PrepareSimpleMovement(Vector2 movementDirection, float maxVelocity, float acceleration) {
            _isMovementEnabled = true;
            _movementDirection = movementDirection;
            _maxVelocity = new Vector2(maxVelocity);
            _acceleration = new Vector2(acceleration);
        }

        public void PrepareSimpleMovement(Vector2 movementDirection, float acceleration) {
            PrepareSimpleMovement(movementDirection, 0f, acceleration);
        }

        #endregion Public Methods

        #region Private Methods

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

        #endregion Private Methods
    }
}
