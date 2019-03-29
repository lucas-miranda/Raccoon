namespace Raccoon.Input {
    public class Axis {
        private float _forceX, _forceY;
        private bool _forceState;

        public Axis(float deadzone = 0.1f) {
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

        public Axis(int joystickId, int joystickHorizontalAxisId, int joystickVerticalAxisId, float deadzone = 0.1f) : this(deadzone) {
            JoystickId = joystickId;
            JoystickAxesIds = new int[] { joystickHorizontalAxisId, joystickVerticalAxisId };
        }

        public int JoystickId { get; set; } = -1;
        public int[] JoystickAxesIds { get; set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public float DeadZone { get; set; } = 0.1f;
        public Button Up { get; set; }
        public Button Right { get; set; }
        public Button Down { get; set; }
        public Button Left { get; set; }
        public Vector2 Value { get { return new Vector2(X, Y); } }

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
                        X = -1;
                    }
                }

                if (Right != null) {
                    Right.Update(delta);
                    if (Right.IsDown) {
                        X += 1;
                    }
                }

                if (Up != null) {
                    Up.Update(delta);
                    if (Up.IsDown) {
                        Y = -1;
                    }
                }

                if (Down != null) {
                    Down.Update(delta);
                    if (Down.IsDown) {
                        Y += 1;
                    }
                }

                // joystick axes
                if (JoystickId > -1) {
                    X += Input.JoyAxisValue(JoystickId, JoystickAxesIds[0]);
                    Y += Input.JoyAxisValue(JoystickId, JoystickAxesIds[1]);
                }
            }

            // deadzone
            if (DeadZone > 0 && Value.LengthSquared() <= DeadZone * DeadZone) {
                X = Y = 0;
            }

            X = Util.Math.Clamp(X, -1, 1);
            Y = Util.Math.Clamp(Y, -1, 1);
        }

        public void ForceState(float x, float y) {
            _forceState = true;
            _forceX = x;
            _forceY = y;
        }

        public override string ToString() {
            return $"[Axis | X: {X}, Y: {Y}]";
        }
    }
}
