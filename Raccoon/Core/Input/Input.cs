using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public enum MouseButton {
        None = 0,
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
        #region Public Members

        public static System.Action<char> OnTextInput = delegate { };

        #endregion Public Members

        #region Private Members

        private static readonly System.Lazy<Input> _lazy = new System.Lazy<Input>(() => new Input());

        private bool _activated;

        // keyboard
        private KeyboardState _keyboardState, _keyboardPreviousState;
        private readonly List<Key> _pressedKeys = new List<Key>();
        private readonly Dictionary<Key, char> _specialKeysToChar = new Dictionary<Key, char>();

        // mouse
        private readonly Dictionary<MouseButton, ButtonState> _mouseButtonsState = new Dictionary<MouseButton, ButtonState>(),
                                                              _mouseButtonsLastState = new Dictionary<MouseButton, ButtonState>();
        private Vector2 _mousePosition;
        private (int X, int Y) _mouseAbsolutePosition;
        private bool _lockMouseOnCenter;
        private Rectangle? _customMouseAllowedArea;

        // gamepads
        private readonly Dictionary<PlayerIndex, GamePadState> _gamepadsState = new Dictionary<PlayerIndex, GamePadState>(),
                                                               _gamepadsPreviousState = new Dictionary<PlayerIndex, GamePadState>();

        #endregion Private Members

        #region Constructors

        private Input() {
            // mouse
            foreach (MouseButton id in System.Enum.GetValues(typeof(MouseButton))) {
                _mouseButtonsState[id] = ButtonState.Released;
                _mouseButtonsLastState[id] = ButtonState.Released;
            }

            PressedKeys = _pressedKeys.AsReadOnly();

            // keys to string
            _specialKeysToChar[Key.Space] = ' ';
            _specialKeysToChar[Key.Period] = '.';
            _specialKeysToChar[Key.Comma] = ',';

            TextInputEXT.TextInput += ProcessTextInput;

            Game.Instance.XNAGameWrapper.Activated += (object sender, System.EventArgs e) => {
                _activated = true;
            };
        }

        #endregion Constructors

        #region Public Properties

        public static Input Instance { get { return _lazy.Value; } }

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

        #endregion GamePads

        #endregion Public Properties

        #region Public Methods

        #region Keyboard

        public static bool IsKeyPressed(Key key) {
            if (!Game.Instance.IsActive) {
                return false;
            }

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

        public static bool IsAnyKeyPressed() {
            return PressedKeys.Count > 0;
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

        public static bool IsGamePadButtonPressed(GamePadIndex gamepadIndex, Buttons buttonId) {
            if (gamepadIndex == GamePadIndex.None) {
                return false;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return (!Instance._gamepadsPreviousState.ContainsKey(playerIndex) || Instance._gamepadsPreviousState[playerIndex].IsButtonUp(buttonId))
              && Instance._gamepadsState[playerIndex].IsButtonDown(buttonId);
        }

        public static bool IsGamePadButtonDown(GamePadIndex gamepadIndex, Buttons buttonId) {
            if (gamepadIndex == GamePadIndex.None) {
                return false;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return Instance._gamepadsState[playerIndex].IsButtonDown(buttonId);
        }

        public static bool IsGamePadButtonReleased(GamePadIndex gamepadIndex, Buttons buttonId) {
            if (gamepadIndex == GamePadIndex.None) {
                return false;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return Instance._gamepadsPreviousState[playerIndex].IsButtonDown(buttonId)
              && (!Instance._gamepadsState.ContainsKey(playerIndex) || Instance._gamepadsState[playerIndex].IsButtonUp(buttonId));
        }

        public static bool IsGamePadButtonUp(GamePadIndex gamepadIndex, Buttons buttonId) {
            if (gamepadIndex == GamePadIndex.None) {
                return true;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return Instance._gamepadsState[playerIndex].IsButtonUp(buttonId);
        }

        public static InputButtonState GamePadButtonState(GamePadIndex gamepadIndex, Buttons buttonId) {
            if (gamepadIndex == GamePadIndex.None) {
                return InputButtonState.Up;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            bool isPreviousStateDown = Instance._gamepadsState[playerIndex].IsButtonDown(buttonId),
                 isCurrentStateDown = Instance._gamepadsPreviousState[playerIndex].IsButtonDown(buttonId);

            if (!isPreviousStateDown) {
                if (isCurrentStateDown) {
                    return InputButtonState.Pressed;
                }

                return InputButtonState.Up;
            }

            if (!isCurrentStateDown) {
                return InputButtonState.Released;
            }

            return InputButtonState.Down;
        }

        public static Vector2 GamePadThumbStickValue(GamePadIndex gamepadIndex, GamePadThumbStick thumbStick) {
            if (gamepadIndex == GamePadIndex.None) {
                return Vector2.Zero;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return new Vector2(thumbStick == GamePadThumbStick.Left ? Instance._gamepadsState[playerIndex].ThumbSticks.Left : Instance._gamepadsState[playerIndex].ThumbSticks.Right);
        }

        public static float GamePadTriggerValue(GamePadIndex gamepadIndex, GamePadTriggerButton triggerButton) {
            if (gamepadIndex == GamePadIndex.None) {
                return 0f;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return triggerButton == GamePadTriggerButton.Left ? Instance._gamepadsState[playerIndex].Triggers.Left : Instance._gamepadsState[playerIndex].Triggers.Right;
        }

        public static bool IsGamepadConnected(GamePadIndex gamepadIndex) {
            if (gamepadIndex == GamePadIndex.None) {
                return false;
            }

            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return Instance._gamepadsState.ContainsKey(playerIndex) && Instance._gamepadsState[playerIndex].IsConnected;
        }

        #endregion GamePad

        #region Mouse

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

        #endregion Mouse

        public void Update(int delta) {
            if (!Game.Instance.IsActive) {
                return;
            }

            #region GamePad

            // gamepad states
            GamePadsConnected = 0;

            PlayerIndex lastPlayerIndex = (PlayerIndex) (MaxGamePads - 1);
            for (PlayerIndex gamepadIndex = PlayerIndex.One; gamepadIndex <= lastPlayerIndex; gamepadIndex++) {
                if (_gamepadsState.ContainsKey(gamepadIndex)) {
                    _gamepadsPreviousState[gamepadIndex] = _gamepadsState[gamepadIndex];
                } else {
                    _gamepadsPreviousState.Remove(gamepadIndex);
                }

                GamePadState gamepadState = GamePad.GetState(gamepadIndex);

                if (!gamepadState.IsConnected) {
                    _gamepadsState.Remove(gamepadIndex);
                    continue;
                }

                _gamepadsState[gamepadIndex] = gamepadState;

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
            foreach (KeyValuePair<MouseButton, ButtonState> button in _mouseButtonsState) {
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

                    _mouseButtonsState[MouseButton.Left] = 
                        _mouseButtonsState[MouseButton.Middle] = 
                        _mouseButtonsState[MouseButton.Right] = 
                        _mouseButtonsState[MouseButton.M4] = 
                        _mouseButtonsState[MouseButton.M5] = ButtonState.Released;

                    MouseScrollWheelDelta = 0;
                    return;
                }
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

            #endregion Mouse
        }

        #endregion Public Methods

        #region Private Methods

        private void ProcessTextInput(char charCode) {
            OnTextInput(charCode);
        }

        #endregion Private Methods

        #region Internal Methods

        internal void OnGameActivated() {
            if (LockMouseOnCenter) {
                // reenable it
                Mouse.IsRelativeMouseModeEXT = true;
            }
        }

        internal void OnGameDeactivated() {
            if (LockMouseOnCenter) {
                // temporary disable it
                Mouse.IsRelativeMouseModeEXT = false;
            }
        }

        #endregion Internal Methods
    }
}
