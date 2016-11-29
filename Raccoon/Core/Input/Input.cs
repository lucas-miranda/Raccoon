using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Raccoon.Input {
    public class Input {
        private static readonly Input _instance = new Input();

        private Dictionary<int, JoystickState> _joysticksState, _joysticksPreviousState;
        //private Dictionary<int, GamePadState> _gamepadsState, _gamepadsPreviousState;
        private KeyboardState _keyboardState, _keyboardPreviousState;

        private Input() {
            _joysticksState = new Dictionary<int, JoystickState>();
            _joysticksPreviousState = new Dictionary<int, JoystickState>();
            /*_gamepadsState = new Dictionary<int, GamePadState>();
            _gamepadsPreviousState = new Dictionary<int, GamePadState>();*/
        }

        public static Input Instance { get { return _instance; } }
        public static int JoysticksConnected { get; private set; }

        public static bool IsKeyPressed(Key key) {
            return _instance._keyboardPreviousState[(Keys) key] == KeyState.Up && _instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyDown(Key key) {
            return _instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyUp(Key key) {
            return _instance._keyboardState[(Keys) key] == KeyState.Up;
        }

        public static bool IsJoyButtonPressed(int joystickId, int buttonId) {
            return (!_instance._joysticksPreviousState.ContainsKey(joystickId) || _instance._joysticksPreviousState[joystickId].Buttons[buttonId] == ButtonState.Released) && _instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Pressed;
        }

        public static bool IsJoyButtonDown(int joystickId, int buttonId) {
            return _instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Pressed;
        }

        public static bool IsJoyButtonUp(int joystickId, int buttonId) {
            return _instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Released;
        }

        public static float JoyAxisValue(int joystickId, int axisId) {
            return _instance._joysticksState[joystickId].Axes[axisId];
        }

        public static bool IsJoystickConnected(int joystickId) {
            return _instance._joysticksState.ContainsKey(joystickId) && _instance._joysticksState[joystickId].IsConnected;
        }

        public void Update(int delta) {
            // joystick states
            _joysticksPreviousState.Clear();
            int id = 0;
            JoysticksConnected = 0;
            JoystickState joystickState = Joystick.GetState(id);
            while (joystickState.IsConnected) {
                if (_joysticksState.ContainsKey(id)) {
                    _joysticksPreviousState[id] = _joysticksState[id];
                }

                _joysticksState[id] = joystickState;
                joystickState = Joystick.GetState(++id);
                JoysticksConnected++;
            }

            // clean unused states
            if (id < _joysticksState.Count) {
                for (int i = id; i < _joysticksState.Count; i++) {
                    _joysticksState.Remove(i);
                }
            }

            // keyboard state
            _keyboardPreviousState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            // mouse
            Mouse.Instance.Update();
        }
    }
}
