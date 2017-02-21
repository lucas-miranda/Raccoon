using System.Collections.Generic;

using Microsoft.Xna.Framework.Input;

namespace Raccoon {
    public enum MouseButton {
        Left,
        Middle,
        Right,
        M4,
        M5
    }

    public class Input {
        private static readonly Input _instance = new Input();

        private Dictionary<int, JoystickState> _joysticksState, _joysticksPreviousState;
        private KeyboardState _keyboardState, _keyboardPreviousState;
        private Dictionary<MouseButton, ButtonState> _mouseButtonsState, _mouseButtonsLastState;
        private Dictionary<Key, char> _specialKeysToChar = new Dictionary<Key, char>();
        private bool _activated;

        private Input() {
            // joystick
            _joysticksState = new Dictionary<int, JoystickState>();
            _joysticksPreviousState = new Dictionary<int, JoystickState>();

            // mouse
            _mouseButtonsState = new Dictionary<MouseButton, ButtonState>();
            _mouseButtonsLastState = new Dictionary<MouseButton, ButtonState>();
            foreach (MouseButton id in System.Enum.GetValues(typeof(MouseButton))) {
                _mouseButtonsState[id] = ButtonState.Released;
                _mouseButtonsLastState[id] = ButtonState.Released;
            }

            // keys to string
            _specialKeysToChar[Key.Space] = ' ';
            _specialKeysToChar[Key.Period] = '.';
            _specialKeysToChar[Key.Comma] = ',';

            Game.Instance.Core.Activated += (object sender, System.EventArgs e) => {
                _activated = true;
            };
        }

        public static Input Instance { get { return _instance; } }
        public static int JoysticksConnected { get; private set; }
        public static Vector2 MousePosition { get; private set; }
        public static int MouseScrollWheel { get; private set; }
        public static int MouseScrollWheelDelta { get; private set; }
        public static string KeyboardText { get; private set; } = "";
        public static Key[] PressedKeys { get; private set; } = new Key[0];

        public static bool IsKeyPressed(Key key) {
            return _instance._keyboardPreviousState[(Keys) key] == KeyState.Up && _instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyDown(Key key) {
            return _instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyReleased(Key key) {
            return _instance._keyboardPreviousState[(Keys) key] == KeyState.Down && _instance._keyboardState[(Keys) key] == KeyState.Up;
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

        public static bool IsJoyButtonReleased(int joystickId, int buttonId) {
            return _instance._joysticksPreviousState[joystickId].Buttons[buttonId] == ButtonState.Pressed && (!_instance._joysticksState.ContainsKey(joystickId) || _instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Released);
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

        public static bool IsMouseButtonPressed(MouseButton button) {
            return _instance._mouseButtonsLastState[button] == ButtonState.Released && _instance._mouseButtonsState[button] == ButtonState.Pressed;
        }

        public static bool IsMouseButtonDown(MouseButton button) {
            return _instance._mouseButtonsState[button] == ButtonState.Pressed;
        }

        public static bool IsMouseButtonReleased(MouseButton button) {
            return _instance._mouseButtonsLastState[button] == ButtonState.Pressed && _instance._mouseButtonsState[button] == ButtonState.Released;
        }

        public static bool IsMouseButtonUp(MouseButton button) {
            return _instance._mouseButtonsState[button] == ButtonState.Released;
        }

        public void Update(int delta) {
            if (!Game.Instance.HasFocus) {
                return;
            }

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

            if (KeyboardText.Length > 0) {
                KeyboardText = "";
            }

            Keys[] _xnaPressedKeys = _keyboardState.GetPressedKeys();
            PressedKeys = new Key[_xnaPressedKeys.Length];
            for (int i = 0; i < _xnaPressedKeys.Length; i++) {
                PressedKeys[i] = (Key) _xnaPressedKeys[i];
            }

            if (PressedKeys.Length > 0) {
                foreach (Key key in PressedKeys) {
                    string str = "";
                    if (_specialKeysToChar.ContainsKey(key)) {
                        str = _specialKeysToChar[key].ToString();
                    } else if ((int) key >= 48 && (int) key <= 90) {
                        str = key.ToString();
                        if ((int) key <= 57) {
                            str = str.Remove(0, 1);
                        } else if ((int) key >= 65) {
                            str = IsKeyDown(Key.LeftShift) ? str : str.ToLower();
                        }
                    } else {
                        continue;
                    }

                    KeyboardText += str;
                }
            }

            // mouse
            MouseState XNAMouseState = Mouse.GetState();

            // positions
            MousePosition = new Vector2(Util.Math.Clamp(XNAMouseState.X, 0, Game.Instance.WindowWidth) / Game.Instance.Scale, Util.Math.Clamp(XNAMouseState.Y, 0, Game.Instance.WindowHeight) / Game.Instance.Scale);

            // buttons
            foreach (KeyValuePair<MouseButton, ButtonState> button in _mouseButtonsState) {
                _mouseButtonsLastState[button.Key] = button.Value;
            }

            // ignore out of screen mouse interactions
            if (XNAMouseState.X < 0 || XNAMouseState.X > Game.Instance.WindowWidth || XNAMouseState.Y < 0 || XNAMouseState.Y > Game.Instance.WindowHeight) {
                _mouseButtonsState[MouseButton.Left] = _mouseButtonsState[MouseButton.Middle] = _mouseButtonsState[MouseButton.Right] = _mouseButtonsState[MouseButton.M4] = _mouseButtonsState[MouseButton.M5] = ButtonState.Released;
                MouseScrollWheelDelta = 0;
                return;
            }

            _mouseButtonsState[MouseButton.Left] = XNAMouseState.LeftButton;
            _mouseButtonsState[MouseButton.Middle] = XNAMouseState.MiddleButton;
            _mouseButtonsState[MouseButton.Right] = XNAMouseState.RightButton;
            _mouseButtonsState[MouseButton.M4] = XNAMouseState.XButton1;
            _mouseButtonsState[MouseButton.M5] = XNAMouseState.XButton2;

            // scroll
            if (_activated) {
                MouseScrollWheelDelta = 0;
                MouseScrollWheel = XNAMouseState.ScrollWheelValue;
                _activated = false;
                return;
            }

            MouseScrollWheelDelta = XNAMouseState.ScrollWheelValue - MouseScrollWheel;
            MouseScrollWheel = XNAMouseState.ScrollWheelValue;
        }
    }
}
