namespace Raccoon {
    public class Button {
        private bool _forceState;

        public Button() { }

        public Button(Key key) {
            Key = key;
        }

        public Button(int joystickId, int joystickButtonId) {
            JoystickId = joystickId;
            JoystickButtonId = joystickButtonId;
        }

        public Key Key { get; set; } = Key.None;
        public int JoystickId { get; set; } = -1;
        public int JoystickButtonId { get; set; } = -1;
        public bool IsDown { get; protected set; }
        public bool IsPressed { get; protected set; }
        public bool IsReleased { get; protected set; } = true;

        public virtual void Update() {
            if (_forceState || (Key != Key.None && Input.IsKeyDown(Key)) || (JoystickId > -1 && Input.IsJoyButtonDown(JoystickId, JoystickButtonId))) {
                if (IsReleased) {
                    IsPressed = IsDown = true;
                    IsReleased = false;
                } else if (IsPressed) {
                    IsPressed = false;
                }
            } else {
                if (!IsReleased) {
                    IsReleased = true;
                    IsPressed = IsDown = false;
                }
            }
        }

        public void ForceState(bool pressed) {
            _forceState = pressed;
        }

        public override string ToString() {
            return $"[Button |" + (Key != Key.None ? $" Key: {Key}" : " ") + (JoystickId != -1 && JoystickButtonId != -1 ? $" JoystickId: {JoystickId} JoystickButtonId: {JoystickButtonId}" : "") + $" |{(IsReleased ? " Released" : " ") + (IsPressed ? " Pressed" : "") + (IsDown ? " Down" : "")}]";
        }
    }
}
