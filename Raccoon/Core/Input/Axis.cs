namespace Raccoon.Input {
    public class Axis {
        #region Private Members

        private float _forceX, _forceY;
        private bool _forceState;

        #endregion Private Members

        #region Constructors

        public Axis(float deadzone = .1f) {
            DeadZone = deadzone;
        }

        public Axis(Button up, Button right, Button down, Button left) {
            Up = up;
            Right = right;
            Down = down;
            Left = left;
        }

        public Axis(Key up, Key right, Key down, Key left) : this(new Button(up), new Button(right), new Button(down), new Button(left)) {
        }

        public Axis(GamePadIndex gamepadIndex, GamePadThumbStick thumbstick, float deadzone = .1f) : this(deadzone) {
            GamePadIndex = gamepadIndex;
            ThumbStick = thumbstick;
        }

        #endregion Constructors

        #region Public Properties

        public GamePadIndex GamePadIndex { get; set; }
        public GamePadThumbStick ThumbStick { get; set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public float DeadZone { get; set; } = .1f;
        public Button Up { get; set; }
        public Button Right { get; set; }
        public Button Down { get; set; }
        public Button Left { get; set; }
        public Vector2 Value { get { return new Vector2(X, Y); } }

        #endregion Public Properties

        #region Public Methods

        public virtual void Update(int delta) {
            X = Y = 0;

            if (_forceState) {
                X = _forceX;
                Y = _forceY;
            } else {
                // buttons
                if (Left != null) {
                    Left.Update(delta);
                    if (Left.IsDown) {
                        X = -1f;
                    }
                }

                if (Right != null) {
                    Right.Update(delta);
                    if (Right.IsDown) {
                        X += 1f;
                    }
                }

                if (Up != null) {
                    Up.Update(delta);
                    if (Up.IsDown) {
                        Y = -1f;
                    }
                }

                if (Down != null) {
                    Down.Update(delta);
                    if (Down.IsDown) {
                        Y += 1f;
                    }
                }

                // joystick axes
                if (GamePadIndex != GamePadIndex.None) {
                    Vector2 thumbStick = Input.GamePadThumbStickValue(GamePadIndex, ThumbStick);

                    X += thumbStick.X;
                    Y += thumbStick.Y;
                }
            }

            // deadzone
            if (DeadZone > 0f && Value.LengthSquared() <= DeadZone * DeadZone) {
                X = Y = 0f;
            }

            X = Util.Math.Clamp(X, -1f, 1f);
            Y = Util.Math.Clamp(Y, -1f, 1f);
        }

        public void ForceState(float x, float y) {
            _forceState = true;
            _forceX = x;
            _forceY = y;
        }

        public override string ToString() {
            return $"[Axis | X: {X}, Y: {Y}]";
        }

        #endregion Public Methods
    }
}
