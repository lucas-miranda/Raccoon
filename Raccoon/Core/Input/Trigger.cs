namespace Raccoon.Input {
    public class Trigger : Button {
        public Trigger(Key key) : base(key) { } 
        public Trigger(int joystickId, int joystickTriggerAxisId) : base(joystickId, joystickTriggerAxisId) { }

        public float Value { get; protected set; }

        public override void Update() {
            Value = 0;
            if (JoystickId > -1) {
                Value = Input.JoyAxisValue(JoystickId, JoystickButtonId);
            }

            if (Key != Key.None) {
                Value += Input.IsKeyDown(Key) ? 1 : -1;
            }

            Value = Util.Math.Clamp(Value, -1, 1);

            if (Value > -1) {
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

        public override string ToString() {
            return $"[Trigger |" + (Key != Key.None ? $" Key: {Key}" : " ") + (JoystickId != -1 && JoystickButtonId != -1 ? $" JoystickId: {JoystickId} JoystickButtonId: {JoystickButtonId}" : "") + $" | Value: {Value} |{(IsReleased ? " Released" : " ") + (IsPressed ? " Pressed" : "") + (IsDown ? " Down" : "")}]";
        }
    }
}
