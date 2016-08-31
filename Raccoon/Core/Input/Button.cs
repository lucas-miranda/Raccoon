using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public class Button {
        public Button() {
            Key = Key.None;
            JoystickId = -1;
            Pressed = Down = false;
            Released = true;
        }

        public Button(Key key) : this() {
            Key = key;
        }

        public Button(int joystickId) : this() {
            JoystickId = joystickId;
        }

        public Key Key { get; private set; }
        public int JoystickId { get; private set; }
        public bool Down { get; protected set; }
        public bool Pressed { get; protected set; }
        public bool Released { get; protected set; }

        internal void UpdateKeys(KeyboardState keyboardState) {
            Update(keyboardState[(Keys) Key] == KeyState.Down);
        }

        internal void UpdateJoys(JoystickState joystickState) {
            if (JoystickId < 0)
                return;

            Update(joystickState.Buttons[JoystickId] == ButtonState.Pressed);
        }

        internal void Update(bool down) {
            if (down) {
                if (Released) {
                    Pressed = Down = true;
                    Released = false;
                } else if (Pressed) {
                    Pressed = false;
                }
            } else {
                if (!Released) {
                    Released = true;
                    Pressed = Down = false;
                }
            }
        }

        public void SetState(bool down, bool pressed, bool released) {
            Down = down;
            Pressed = pressed;
            Released = released;
        }

        public override string ToString() {
            return $"[Button | Key: {Key} |{(Released ? " Released" : " ") + (Pressed ? " Pressed" : "") + (Down ? " Down" : "")}]";
        }
    }
}
