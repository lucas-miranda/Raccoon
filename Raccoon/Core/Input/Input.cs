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

        private bool _activated;

        // keyboard
        private KeyboardState _keyboardState, _keyboardPreviousState;
        private readonly List<Key> _pressedKeys = new List<Key>();
        private readonly Dictionary<Key, char> _specialKeysToChar = new Dictionary<Key, char>();

        // mouse
        private readonly Dictionary<MouseButton, Microsoft.Xna.Framework.Input.ButtonState> _mouseButtonsState = new Dictionary<MouseButton, Microsoft.Xna.Framework.Input.ButtonState>(),
                                                              _mouseButtonsLastState = new Dictionary<MouseButton, Microsoft.Xna.Framework.Input.ButtonState>();

        private Vector2 _mousePosition;
        private (int X, int Y) _mouseAbsolutePosition;
        private bool _lockMouseOnCenter;
        private Rectangle? _customMouseAllowedArea;

        // gamepads
        private readonly Dictionary<PlayerIndex, Microsoft.Xna.Framework.Input.GamePadState> _gamepadsState = new Dictionary<PlayerIndex, Microsoft.Xna.Framework.Input.GamePadState>(),
                                                               _gamepadsPreviousState = new Dictionary<PlayerIndex, Microsoft.Xna.Framework.Input.GamePadState>();

        #endregion Private Members

        #region Constructors

        private Input() {
            // mouse
            foreach (MouseButton id in System.Enum.GetValues(typeof(MouseButton))) {
                _mouseButtonsState[id] = Microsoft.Xna.Framework.Input.ButtonState.Released;
                _mouseButtonsLastState[id] = Microsoft.Xna.Framework.Input.ButtonState.Released;
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

            return (!Instance._gamepadsPreviousState.ContainsKey(playerIndex) || Instance._gamepadsPreviousState[playerIndex].IsButtonUp(buttonId))
              && Instance._gamepadsState[playerIndex].IsButtonDown(buttonId);
        }

        public static bool IsGamePadButtonDown(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return Instance._gamepadsState[playerIndex].IsButtonDown(buttonId);
        }

        public static bool IsGamePadButtonReleased(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return Instance._gamepadsPreviousState[playerIndex].IsButtonDown(buttonId)
              && (!Instance._gamepadsState.ContainsKey(playerIndex) || Instance._gamepadsState[playerIndex].IsButtonUp(buttonId));
        }

        public static bool IsGamePadButtonUp(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            return Instance._gamepadsState[playerIndex].IsButtonUp(buttonId);
        }

        public static ButtonState GamePadButtonState(GamepadIndex gamepadIndex, Buttons buttonId) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();

            bool isPreviousStateDown = Instance._gamepadsState[playerIndex].IsButtonDown(buttonId),
                 isCurrentStateDown = Instance._gamepadsPreviousState[playerIndex].IsButtonDown(buttonId);

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
            return new Vector2(thumbStick == GamepadThumbStick.Left ? Instance._gamepadsState[playerIndex].ThumbSticks.Left : Instance._gamepadsState[playerIndex].ThumbSticks.Right);
        }

        public static float GamePadTriggerValue(GamepadIndex gamepadIndex, GamepadTriggerButton triggerButton) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return triggerButton == GamepadTriggerButton.Left ? Instance._gamepadsState[playerIndex].Triggers.Left : Instance._gamepadsState[playerIndex].Triggers.Right;
        }

        public static bool IsGamepadConnected(GamepadIndex gamepadIndex) {
            PlayerIndex playerIndex = gamepadIndex.ToPlayerIndex();
            return Instance._gamepadsState.ContainsKey(playerIndex) && Instance._gamepadsState[playerIndex].IsConnected;
        }

        #endregion GamePad

        #region Mouse

        public static bool IsMouseButtonPressed(MouseButton button) {
            return Instance._mouseButtonsLastState[button] == Microsoft.Xna.Framework.Input.ButtonState.Released && Instance._mouseButtonsState[button] == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        }

        public static bool IsMouseButtonDown(MouseButton button) {
            return Instance._mouseButtonsState[button] == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        }

        public static bool IsMouseButtonReleased(MouseButton button) {
            return Instance._mouseButtonsLastState[button] == Microsoft.Xna.Framework.Input.ButtonState.Pressed && Instance._mouseButtonsState[button] == Microsoft.Xna.Framework.Input.ButtonState.Released;
        }

        public static bool IsMouseButtonUp(MouseButton button) {
            return Instance._mouseButtonsState[button] == Microsoft.Xna.Framework.Input.ButtonState.Released;
        }

        public static ButtonState MouseButtonState(MouseButton button) {
            Microsoft.Xna.Framework.Input.ButtonState previousState = Instance._mouseButtonsLastState[button],
                                                      currentState = Instance._mouseButtonsState[button];

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
            foreach (KeyValuePair<MouseButton, Microsoft.Xna.Framework.Input.ButtonState> button in _mouseButtonsState) {
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
                        _mouseButtonsState[MouseButton.M5] = Microsoft.Xna.Framework.Input.ButtonState.Released;

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
            OnTextInput?.Invoke(charCode);
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

        internal static bool HasGamepad(GamepadIndex index) {
            return Instance._gamepadsState.ContainsKey(index.ToPlayerIndex());
        }

        internal static Microsoft.Xna.Framework.Input.GamePadState GetGamepadState(GamepadIndex index) {
            PlayerIndex playerIndex = index.ToPlayerIndex();

            if (!Instance._gamepadsState.TryGetValue(playerIndex, out Microsoft.Xna.Framework.Input.GamePadState xnaGamePadState)) {
                throw new System.ArgumentException($"There is no gamepad state at XNA {nameof(PlayerIndex)} '{playerIndex}' (GamepadIndex: {index}).");
            }

            return xnaGamePadState;
        }

        internal static bool TryGetGamepadState(GamepadIndex index, out Microsoft.Xna.Framework.Input.GamePadState xnaGamePadState) {
            return Instance._gamepadsState.TryGetValue(index.ToPlayerIndex(), out xnaGamePadState);
        }

        #endregion Internal Methods
    }
}
