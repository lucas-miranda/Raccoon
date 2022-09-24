using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public class Input {
        #region Public Members

        public static System.Action<char> OnTextInput;

        #endregion Public Members

        #region Private Members

        private bool _activated = true;

        // keyboard
        private KeyboardState _keyboardState, _keyboardPreviousState;
        private readonly List<Key> _pressedKeys = new List<Key>();
        private readonly Dictionary<int, char> _specialKeysToChar = new Dictionary<int, char>();

        // mouse
        private readonly Dictionary<int, Microsoft.Xna.Framework.Input.ButtonState> _mouseButtonsState = new Dictionary<int, Microsoft.Xna.Framework.Input.ButtonState>(),
                                                                                    _mouseButtonsLastState = new Dictionary<int, Microsoft.Xna.Framework.Input.ButtonState>();

        private Vector2 _mousePosition;
        private (int X, int Y) _mouseAbsolutePosition;
        private bool _lockMouseOnCenter;
        private Rectangle? _customMouseAllowedArea;

        // gamepads
        private readonly Dictionary<int, Microsoft.Xna.Framework.Input.GamePadState> _gamepadsState = new Dictionary<int, Microsoft.Xna.Framework.Input.GamePadState>(),
                                                                                     _gamepadsPreviousState = new Dictionary<int, Microsoft.Xna.Framework.Input.GamePadState>();

        private int? _maxAllowedGamepads;

        #endregion Private Members

        #region Constructors

        private Input() {
            // mouse
            foreach (MouseButton id in System.Enum.GetValues(typeof(MouseButton))) {
                _mouseButtonsState[(int) id] = Microsoft.Xna.Framework.Input.ButtonState.Released;
                _mouseButtonsLastState[(int) id] = Microsoft.Xna.Framework.Input.ButtonState.Released;
            }

            PressedKeys = _pressedKeys.AsReadOnly();

            // keys to string
            _specialKeysToChar[(int) Key.Space] = ' ';
            _specialKeysToChar[(int) Key.Period] = '.';
            _specialKeysToChar[(int) Key.Comma] = ',';

            TextInputEXT.TextInput += ProcessTextInput;
        }

        #endregion Constructors

        #region Public Properties

        public static Input Instance { get; private set; }

        #region Mouse

        public static Vector2 MouseMovement { get; private set; }
        public static int MouseScrollWheel { get; private set; }
        public static int MouseScrollWheelDelta { get; private set; }
        public static bool LockMouseOnWindow { get; set; }

        public static bool LockMouseOnCenter {
            get {
                return Instance._lockMouseOnCenter;
            }

            set {
                Instance._lockMouseOnCenter =
                    Mouse.IsRelativeMouseModeEXT = value;
            }
        }

        public static Vector2 MouseAbsolutePosition {
            get {
                return new Vector2(
                    Instance._mouseAbsolutePosition.X,
                    Instance._mouseAbsolutePosition.Y
                );
            }

            set {
                if (LockMouseOnCenter) {
                    return;
                }

                int x = (int) Util.Math.Floor(value.X),
                    y = (int) Util.Math.Floor(value.Y);

                Instance._mouseAbsolutePosition = (x, y);
                Mouse.SetPosition(x, y);
            }
        }

        public static Vector2 MousePosition {
            get {
                return Instance._mousePosition;
            }

            set {
                if (CustomMouseAllowedArea.HasValue) {
                    Instance._mousePosition = new Vector2(
                        Util.Math.Clamp(Util.Math.Floor(value.X), Util.Math.Max(0, CustomMouseAllowedArea.Value.Left), Util.Math.Min(Game.Instance.Width, CustomMouseAllowedArea.Value.Right)),
                        Util.Math.Clamp(Util.Math.Floor(value.Y), Util.Math.Max(0, CustomMouseAllowedArea.Value.Top),  Util.Math.Min(Game.Instance.Height, CustomMouseAllowedArea.Value.Bottom))
                    );
                } else {
                    Instance._mousePosition = new Vector2(
                        Util.Math.Clamp(Util.Math.Floor(value.X), 0, Game.Instance.Width),
                        Util.Math.Clamp(Util.Math.Floor(value.Y), 0, Game.Instance.Height)
                    );
                }


                float scale = Game.Instance.PixelScale * Game.Instance.KeepProportionsScale;

                Mouse.SetPosition(
                    (int) Util.Math.Clamp(Util.Math.Round(Instance._mousePosition.X * scale), 0, Game.Instance.WindowWidth),
                    (int) Util.Math.Clamp(Util.Math.Round(Instance._mousePosition.Y * scale), 0, Game.Instance.WindowHeight)
                );
            }
        }

        public static Rectangle? CustomMouseAllowedArea {
            get {
                return Instance._customMouseAllowedArea;
            }

            set {
                Instance._customMouseAllowedArea = value;

                if (!LockMouseOnCenter && LockMouseOnWindow
                 && Instance._customMouseAllowedArea.HasValue && !Instance._customMouseAllowedArea.Value.Contains(MousePosition)) {
                    MousePosition = Util.Math.Clamp(MousePosition, Instance._customMouseAllowedArea.Value);
                }
            }
        }

        #endregion Mouse

        #region Keyboard

        public static ReadOnlyCollection<Key> PressedKeys { get; private set; }

        #endregion Keyboard

        #region GamePads

        public static int MaxGamePads { get { return System.Enum.GetNames(typeof(PlayerIndex)).Length; } }
        public static int GamePadsConnected { get; private set; }

        public static int? MaxAllowedGamepads {
            get {
                return Instance._maxAllowedGamepads;
            }

            set {
                if (value.HasValue) {
                    value = Util.Math.Max(0, value.Value);
                }

                Instance._maxAllowedGamepads = value;
            }
        }

        #endregion GamePads

        #endregion Public Properties

        #region Public Methods

        public static void Initialize() {
            if (Instance != null) {
                return;
            }

            Instance = new Input();
        }

        public static void Deinitialize() {
            if (Instance == null) {
                return;
            }

            Instance = null;
        }

        #region Keyboard

        public static bool IsKeyPressed(Key key) {
            if (!Game.Instance.IsActive) {
                return false;
            }

            return Instance._keyboardPreviousState[(Keys) key] == KeyState.Up
                && Instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyDown(Key key) {
            return Instance._keyboardState[(Keys) key] == KeyState.Down;
        }

        public static bool IsKeyReleased(Key key) {
            return Instance._keyboardPreviousState[(Keys) key] == KeyState.Down
                && Instance._keyboardState[(Keys) key] == KeyState.Up;
        }

        public static bool IsKeyUp(Key key) {
            return Instance._keyboardState[(Keys) key] == KeyState.Up;
        }

        public static bool IsAnyKeyPressed() {
            return PressedKeys.Count > 0;
        }

        public static ButtonState KeyboardState(Key key) {
            KeyState previousState = Instance._keyboardPreviousState[(Keys) key],
                     currentState = Instance._keyboardState[(Keys) key];

            if (previousState == KeyState.Up) {
                if (currentState == KeyState.Down) {
                    return ButtonState.Pressed;
                }

                return ButtonState.Up;
            }

            if (currentState == KeyState.Up) {
                return ButtonState.Released;
            }

            return ButtonState.Down;
        }

        public static void SetInputLocation(Rectangle rectangle) {
            TextInputEXT.SetInputRectangle(rectangle);
        }

        public static void StartTextInput() {
            TextInputEXT.StartTextInput();
        }

        public static void StopTextInput() {
            TextInputEXT.StopTextInput();
        }

        #endregion Keyboard

        #region GamePad

        public static bool IsGamePadButtonPressed(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return (!Instance._gamepadsPreviousState.ContainsKey((int) playerIndex) || Instance._gamepadsPreviousState[(int) playerIndex].IsButtonUp(buttonId))
              && Instance._gamepadsState[(int) playerIndex].IsButtonDown(buttonId);
        }

        public static bool IsGamePadButtonDown(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return Instance._gamepadsState[(int) playerIndex].IsButtonDown(buttonId);
        }

        public static bool IsGamePadButtonReleased(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return Instance._gamepadsPreviousState[(int) playerIndex].IsButtonDown(buttonId)
              && (!Instance._gamepadsState.ContainsKey((int) playerIndex) || Instance._gamepadsState[(int) playerIndex].IsButtonUp(buttonId));
        }

        public static bool IsGamePadButtonUp(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return Instance._gamepadsState[(int) playerIndex].IsButtonUp(buttonId);
        }

        public static ButtonState GamePadButtonState(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            bool isPreviousStateDown = Instance._gamepadsState[(int) playerIndex].IsButtonDown(buttonId),
                 isCurrentStateDown = Instance._gamepadsPreviousState[(int) playerIndex].IsButtonDown(buttonId);

            if (!isPreviousStateDown) {
                if (isCurrentStateDown) {
                    return ButtonState.Pressed;
                }

                return ButtonState.Up;
            }

            if (!isCurrentStateDown) {
                return ButtonState.Released;
            }

            return ButtonState.Down;
        }

        public static Vector2 GamePadThumbStickValue(GamepadIndex gamepadIndex, GamepadThumbStick thumbStick) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return new Vector2(thumbStick == GamepadThumbStick.Left ? Instance._gamepadsState[(int) playerIndex].ThumbSticks.Left : Instance._gamepadsState[(int) playerIndex].ThumbSticks.Right);
        }

        public static float GamePadTriggerValue(GamepadIndex gamepadIndex, GamepadTriggerButton triggerButton) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return triggerButton == GamepadTriggerButton.Left ? Instance._gamepadsState[(int) playerIndex].Triggers.Left : Instance._gamepadsState[(int) playerIndex].Triggers.Right;
        }

        public static bool IsGamepadConnected(GamepadIndex gamepadIndex) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return Instance._gamepadsState.ContainsKey((int) playerIndex) && Instance._gamepadsState[(int) playerIndex].IsConnected;
        }

        #endregion GamePad

        #region Mouse

        public static bool IsMouseButtonPressed(MouseButton button) {
            return Instance._mouseButtonsLastState[(int) button] == Microsoft.Xna.Framework.Input.ButtonState.Released
                && Instance._mouseButtonsState[(int) button] == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        }

        public static bool IsMouseButtonDown(MouseButton button) {
            return Instance._mouseButtonsState[(int) button] == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        }

        public static bool IsMouseButtonReleased(MouseButton button) {
            return Instance._mouseButtonsLastState[(int) button] == Microsoft.Xna.Framework.Input.ButtonState.Pressed
                && Instance._mouseButtonsState[(int) button] == Microsoft.Xna.Framework.Input.ButtonState.Released;
        }

        public static bool IsMouseButtonUp(MouseButton button) {
            return Instance._mouseButtonsState[(int) button] == Microsoft.Xna.Framework.Input.ButtonState.Released;
        }

        public static ButtonState MouseButtonState(MouseButton button) {
            Microsoft.Xna.Framework.Input.ButtonState previousState = Instance._mouseButtonsLastState[(int) button],
                                                      currentState = Instance._mouseButtonsState[(int) button];

            if (previousState == Microsoft.Xna.Framework.Input.ButtonState.Released) {
                if (currentState == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    return ButtonState.Pressed;
                }

                return ButtonState.Up;
            }

            if (currentState == Microsoft.Xna.Framework.Input.ButtonState.Released) {
                return ButtonState.Released;
            }

            return ButtonState.Down;
        }

        #endregion Mouse

        public void Update(int delta) {
            if (!Game.Instance.IsActive || !_activated) {
                return;
            }

            #region GamePad

            // gamepad states
            GamePadsConnected = 0;
            int lastPlayerIndex = Util.Math.Max(0, (MaxAllowedGamepads.HasValue ? MaxAllowedGamepads.Value : MaxGamePads) - 1);

            for (int gamepadIndex = (int) PlayerIndex.One; gamepadIndex <= lastPlayerIndex; gamepadIndex++) {
                bool isRegistered;

                if (_gamepadsState.ContainsKey((int) gamepadIndex)) {
                    isRegistered = true;
                    _gamepadsPreviousState[(int) gamepadIndex] = _gamepadsState[(int) gamepadIndex];
                } else {
                    isRegistered = false;
                    _gamepadsPreviousState.Remove((int) gamepadIndex);
                }

                Microsoft.Xna.Framework.Input.GamePadState gamepadState = GamePad.GetState((PlayerIndex) gamepadIndex);

                if (!gamepadState.IsConnected) {
                    if (isRegistered) {
                        _gamepadsState.Remove((int) gamepadIndex);
                    }

                    continue;
                }

                _gamepadsState[(int) gamepadIndex] = gamepadState;
                GamePadsConnected++;
            }

            #endregion GamePad

            #region Keyboard

            // keyboard state
            _keyboardPreviousState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            _pressedKeys.Clear();
            Keys[] _xnaPressedKeys = _keyboardState.GetPressedKeys();

            for (int i = 0; i < _xnaPressedKeys.Length; i++) {
                _pressedKeys.Add((Key) _xnaPressedKeys[i]);
            }

            #endregion Keyboard

            #region Mouse

            // mouse
            MouseState XNAMouseState = Mouse.GetState();

            // buttons
            foreach (KeyValuePair<int, Microsoft.Xna.Framework.Input.ButtonState> button in _mouseButtonsState) {
                _mouseButtonsLastState[button.Key] = button.Value;
            }

            // positions
            float scale = Game.Instance.PixelScale * Game.Instance.KeepProportionsScale;
            (int X, int Y) newMouseAbsolutePosition = (XNAMouseState.X, XNAMouseState.Y);

            if (LockMouseOnCenter) {
                MouseMovement = new Vector2(
                    newMouseAbsolutePosition.X / scale,
                    newMouseAbsolutePosition.Y / scale
                );

                int halfWindowWidth  = Game.Instance.WindowWidth  / 2,
                    halfWindowHeight = Game.Instance.WindowHeight / 2;

                _mouseAbsolutePosition = (halfWindowWidth, halfWindowHeight);
                _mousePosition = Util.Math.Round(new Vector2(halfWindowWidth, halfWindowHeight) / scale);
            } else {
                (int X, int Y) expectedMousePos = newMouseAbsolutePosition;

                if (LockMouseOnWindow) {
                    if (CustomMouseAllowedArea.HasValue) {
                        float s = Game.Instance.PixelScale * Game.Instance.KeepProportionsScale;
                        Rectangle allowedArea = CustomMouseAllowedArea.Value;

                        expectedMousePos = (
                            Util.Math.Clamp(
                                XNAMouseState.X,
                                Util.Math.Max(0, (int) Util.Math.Round(allowedArea.Left * s)),
                                Util.Math.Min(Game.Instance.WindowWidth, (int) Util.Math.Round(allowedArea.Right * s))
                            ),
                            Util.Math.Clamp(
                                XNAMouseState.Y,
                                Util.Math.Max(0, (int) Util.Math.Round(allowedArea.Top * s)),
                                Util.Math.Min(Game.Instance.WindowHeight, (int) Util.Math.Round(allowedArea.Bottom * s))
                            )
                        );
                    } else {
                        expectedMousePos = (
                            Util.Math.Clamp(XNAMouseState.X, 0, Game.Instance.WindowWidth),
                            Util.Math.Clamp(XNAMouseState.Y, 0, Game.Instance.WindowHeight)
                        );
                    }
                }

                if (expectedMousePos.X != newMouseAbsolutePosition.X || expectedMousePos.Y != newMouseAbsolutePosition.Y) {
                    Mouse.SetPosition(expectedMousePos.X, expectedMousePos.Y);
                    newMouseAbsolutePosition = expectedMousePos;
                }

                MouseMovement = new Vector2(
                    (newMouseAbsolutePosition.X - _mouseAbsolutePosition.X) / scale,
                    (newMouseAbsolutePosition.Y - _mouseAbsolutePosition.Y) / scale
                );

                _mouseAbsolutePosition = newMouseAbsolutePosition;
                _mousePosition = new Vector2(
                    Util.Math.Clamp(Util.Math.Round(newMouseAbsolutePosition.X / scale), 0, Game.Instance.Width),
                    Util.Math.Clamp(Util.Math.Round(newMouseAbsolutePosition.Y / scale), 0, Game.Instance.Height)
                );

                // ignore out of screen mouse interactions
                if (XNAMouseState.X < 0 || XNAMouseState.X > Game.Instance.WindowWidth || XNAMouseState.Y < 0 || XNAMouseState.Y > Game.Instance.WindowHeight) {
                    // mouse buttons and scroll was reset because went out of bounds, so it should stop at this phase

                    _mouseButtonsState[(int) MouseButton.Left] =
                        _mouseButtonsState[(int) MouseButton.Middle] =
                        _mouseButtonsState[(int) MouseButton.Right] =
                        _mouseButtonsState[(int) MouseButton.M4] =
                        _mouseButtonsState[(int) MouseButton.M5] = Microsoft.Xna.Framework.Input.ButtonState.Released;

                    MouseScrollWheelDelta = 0;
                    return;
                }
            }

            _mouseButtonsState[(int) MouseButton.Left] = XNAMouseState.LeftButton;
            _mouseButtonsState[(int) MouseButton.Middle] = XNAMouseState.MiddleButton;
            _mouseButtonsState[(int) MouseButton.Right] = XNAMouseState.RightButton;
            _mouseButtonsState[(int) MouseButton.M4] = XNAMouseState.XButton1;
            _mouseButtonsState[(int) MouseButton.M5] = XNAMouseState.XButton2;

            // scroll
            MouseScrollWheelDelta = XNAMouseState.ScrollWheelValue - MouseScrollWheel;
            MouseScrollWheel = XNAMouseState.ScrollWheelValue;

            #endregion Mouse
        }

        #endregion Public Methods

        #region Private Methods

        private void ProcessTextInput(char charCode) {
            OnTextInput?.Invoke(charCode);
        }

        #endregion Private Methods

        #region Internal Methods

        internal void OnGameActivated() {
            _activated = true;

            if (LockMouseOnCenter) {
                // reenable it
                Mouse.IsRelativeMouseModeEXT = true;
            }

            // ensure mouse state is correct when activating game again
            MouseState XNAMouseState = Mouse.GetState();
            MouseScrollWheelDelta = 0;
            MouseScrollWheel = XNAMouseState.ScrollWheelValue;
        }

        internal void OnGameDeactivated() {
            if (LockMouseOnCenter) {
                // temporary disable it
                Mouse.IsRelativeMouseModeEXT = false;
            }
            _activated = false;
        }

        internal static bool HasGamepad(GamepadIndex index) {
            return Instance._gamepadsState.ContainsKey((int) index.ToPlayerIndex());
        }

        internal static Microsoft.Xna.Framework.Input.GamePadState GetGamepadState(GamepadIndex index) {
            PlayerIndex playerIndex = index.ToPlayerIndex();

            if (!Instance._gamepadsState.TryGetValue((int) playerIndex, out Microsoft.Xna.Framework.Input.GamePadState xnaGamePadState)) {
                throw new System.ArgumentException($"There is no gamepad state at XNA {nameof(PlayerIndex)} '{playerIndex}' (GamepadIndex: {index}).");
            }

            return xnaGamePadState;
        }

        internal static bool TryGetGamepadState(GamepadIndex index, out Microsoft.Xna.Framework.Input.GamePadState xnaGamePadState) {
            return Instance._gamepadsState.TryGetValue((int) index.ToPlayerIndex(), out xnaGamePadState);
        }

        #endregion Internal Methods
    }
}
