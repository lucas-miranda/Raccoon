using System.Collections.Generic;

using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public enum MouseButton {
        Left,
        Middle,
        Right,
        M4,
        M5
    }

    public enum InputButtonState {
        None,
        Up,
        Released,
        Pressed,
        Down
    }

    public class Input {
        private static readonly System.Lazy<Input> _lazy = new System.Lazy<Input>(() => new Input());

        private Dictionary<int, JoystickState> _joysticksState = new Dictionary<int, JoystickState>(), _joysticksPreviousState = new Dictionary<int, JoystickState>();
        private KeyboardState _keyboardState, _keyboardPreviousState;
        private Dictionary<MouseButton, ButtonState> _mouseButtonsState = new Dictionary<MouseButton, ButtonState>(), _mouseButtonsLastState = new Dictionary<MouseButton, ButtonState>();
        private Dictionary<Key, char> _specialKeysToChar = new Dictionary<Key, char>();
        private bool _activated;
        private string _keyboardTextBuffer = "";
        private Vector2 _mousePosition;

        private Input() {
            // mouse
            foreach (MouseButton id in System.Enum.GetValues(typeof(MouseButton))) {
                _mouseButtonsState[id] = ButtonState.Released;
                _mouseButtonsLastState[id] = ButtonState.Released;
            }

            // keys to string
            _specialKeysToChar[Key.Space] = ' ';
            _specialKeysToChar[Key.Period] = '.';
            _specialKeysToChar[Key.Comma] = ',';

            Game.Instance.Core.Window.TextInput += ProcessTextInput;
            Game.Instance.Core.Activated += (object sender, System.EventArgs e) => {
                _activated = true;
            };
        }

        public static Input Instance { get { return _lazy.Value; } }
        public static int JoysticksConnected { get; private set; }
        public static Vector2 MouseMovement { get; private set; }
        public static int MouseScrollWheel { get; private set; }
        public static int MouseScrollWheelDelta { get; private set; }
        public static bool LockMouseOnWindow { get; set; }
        public static bool LockMouseOnCenter { get; set; }
        public static string KeyboardText { get; private set; } = "";
        public static Key[] PressedKeys { get; private set; } = new Key[0];

        public static Vector2 MousePosition {
            get {
                return Instance._mousePosition;
            }

            set {
                Instance._mousePosition = new Vector2(Util.Math.Clamp(value.X, 0, Game.Instance.WindowWidth) / Game.Instance.Scale, Util.Math.Clamp(value.Y, 0, Game.Instance.WindowHeight) / Game.Instance.Scale);
                Mouse.SetPosition((int) Util.Math.Clamp(value.X, 0, Game.Instance.WindowWidth), (int) Util.Math.Clamp(value.Y, 0, Game.Instance.WindowHeight));
            }
        }

        public static bool IsKeyPressed(Key key) {
            return Instance._keyboardPreviousState[(Keys) key] == KeyState.Up && Instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyDown(Key key) {
            return Instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyReleased(Key key) {
            return Instance._keyboardPreviousState[(Keys) key] == KeyState.Down && Instance._keyboardState[(Keys) key] == KeyState.Up;
        }

        public static bool IsKeyUp(Key key) {
            return Instance._keyboardState[(Keys) key] == KeyState.Up;
        }

        public static InputButtonState KeyboardState(Key key) {
            KeyState previousState = Instance._keyboardPreviousState[(Keys) key],
                     currentState = Instance._keyboardState[(Keys) key];

            if (previousState == KeyState.Up) {
                if (currentState == KeyState.Down) {
                    return InputButtonState.Pressed;
                }

                return InputButtonState.Up;
            }

            if (currentState == KeyState.Up) {
                return InputButtonState.Released;
            }

            return InputButtonState.Down;
        }

        public static bool IsJoyButtonPressed(int joystickId, int buttonId) {
            return (!Instance._joysticksPreviousState.ContainsKey(joystickId) || Instance._joysticksPreviousState[joystickId].Buttons[buttonId] == ButtonState.Released) && Instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Pressed;
        }

        public static bool IsJoyButtonDown(int joystickId, int buttonId) {
            return Instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Pressed;
        }

        public static bool IsJoyButtonReleased(int joystickId, int buttonId) {
            return Instance._joysticksPreviousState[joystickId].Buttons[buttonId] == ButtonState.Pressed && (!Instance._joysticksState.ContainsKey(joystickId) || Instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Released);
        }

        public static bool IsJoyButtonUp(int joystickId, int buttonId) {
            return Instance._joysticksState[joystickId].Buttons[buttonId] == ButtonState.Released;
        }

        public static InputButtonState JoyButtonState(int joystickId, int buttonId) {
            ButtonState previousState = Instance._joysticksState[joystickId].Buttons[buttonId],
                        currentState = Instance._joysticksPreviousState[joystickId].Buttons[buttonId];

            if (previousState == ButtonState.Released) {
                if (currentState == ButtonState.Pressed) {
                    return InputButtonState.Pressed;
                }

                return InputButtonState.Up;
            }

            if (currentState == ButtonState.Released) {
                return InputButtonState.Released;
            }

            return InputButtonState.Down;
        }

        public static float JoyAxisValue(int joystickId, int axisId) {
            return Instance._joysticksState[joystickId].Axes[axisId];
        }

        public static bool IsJoystickConnected(int joystickId) {
            return Instance._joysticksState.ContainsKey(joystickId) && Instance._joysticksState[joystickId].IsConnected;
        }

        public static bool IsMouseButtonPressed(MouseButton button) {
            return Instance._mouseButtonsLastState[button] == ButtonState.Released && Instance._mouseButtonsState[button] == ButtonState.Pressed;
        }

        public static bool IsMouseButtonDown(MouseButton button) {
            return Instance._mouseButtonsState[button] == ButtonState.Pressed;
        }

        public static bool IsMouseButtonReleased(MouseButton button) {
            return Instance._mouseButtonsLastState[button] == ButtonState.Pressed && Instance._mouseButtonsState[button] == ButtonState.Released;
        }

        public static bool IsMouseButtonUp(MouseButton button) {
            return Instance._mouseButtonsState[button] == ButtonState.Released;
        }

        public static InputButtonState MouseButtonState(MouseButton button) {
            ButtonState previousState = Instance._mouseButtonsLastState[button],
                        currentState = Instance._mouseButtonsState[button];

            if (previousState == ButtonState.Released) {
                if (currentState == ButtonState.Pressed) {
                    return InputButtonState.Pressed;
                }

                return InputButtonState.Up;
            }

            if (currentState == ButtonState.Released) {
                return InputButtonState.Released;
            }

            return InputButtonState.Down;
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
            
            Keys[] _xnaPressedKeys = _keyboardState.GetPressedKeys();
            PressedKeys = new Key[_xnaPressedKeys.Length];
            for (int i = 0; i < _xnaPressedKeys.Length; i++) {
                PressedKeys[i] = (Key) _xnaPressedKeys[i];
            }

            // keyboard text input
            KeyboardText = "";
            if (_keyboardTextBuffer.Length > 0) {
                KeyboardText = _keyboardTextBuffer;
                _keyboardTextBuffer = "";
            }

            // mouse
            MouseState XNAMouseState = Mouse.GetState();

            // buttons
            foreach (KeyValuePair<MouseButton, ButtonState> button in _mouseButtonsState) {
                _mouseButtonsLastState[button.Key] = button.Value;
            }

            // ignore out of screen mouse interactions
            if (XNAMouseState.X < 0 || XNAMouseState.X > Game.Instance.WindowWidth || XNAMouseState.Y < 0 || XNAMouseState.Y > Game.Instance.WindowHeight) {
                _mouseButtonsState[MouseButton.Left] = _mouseButtonsState[MouseButton.Middle] = _mouseButtonsState[MouseButton.Right] = _mouseButtonsState[MouseButton.M4] = _mouseButtonsState[MouseButton.M5] = ButtonState.Released;
                MouseScrollWheelDelta = 0;

                if (LockMouseOnWindow) {
                    Mouse.SetPosition(Util.Math.Clamp(XNAMouseState.X, 0, Game.Instance.WindowWidth), Util.Math.Clamp(XNAMouseState.Y, 0, Game.Instance.WindowHeight));
                }
                return;
            }

            // positions
            Vector2 newMousePosition = new Vector2(Util.Math.Clamp(XNAMouseState.X, 0, Game.Instance.WindowWidth) / Game.Instance.Scale, Util.Math.Clamp(XNAMouseState.Y, 0, Game.Instance.WindowHeight) / Game.Instance.Scale);
            MouseMovement = newMousePosition - _mousePosition;

            if (LockMouseOnCenter) {
                _mousePosition = new Vector2(Game.Instance.WindowWidth / (2 * Game.Instance.Scale), Game.Instance.WindowHeight / (2 * Game.Instance.Scale));
                Mouse.SetPosition(Game.Instance.WindowWidth / 2, Game.Instance.WindowHeight / 2);
            } else {
                _mousePosition = newMousePosition;
            }

            _mouseButtonsState[MouseButton.Left] = XNAMouseState.LeftButton;
            _mouseButtonsState[MouseButton.Middle] = XNAMouseState.MiddleButton;
            _mouseButtonsState[MouseButton.Right] = XNAMouseState.RightButton;
            _mouseButtonsState[MouseButton.M4] = XNAMouseState.XButton1;
            _mouseButtonsState[MouseButton.M5] = XNAMouseState.XButton2;

            // scroll
            if (_activated) { // reset scroll wheel values if mouse is coming out of game screen
                MouseScrollWheelDelta = 0;
                MouseScrollWheel = XNAMouseState.ScrollWheelValue;
                _activated = false;
                return;
            }

            MouseScrollWheelDelta = XNAMouseState.ScrollWheelValue - MouseScrollWheel;
            MouseScrollWheel = XNAMouseState.ScrollWheelValue;
        }

        private void ProcessTextInput(object sender, Microsoft.Xna.Framework.TextInputEventArgs e) {
            _keyboardTextBuffer += e.Character.ToString();
        }
    }
}
