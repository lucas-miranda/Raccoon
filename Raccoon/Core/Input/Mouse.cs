using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public class Mouse {
        #region Public Enum

        public enum Button {
            Left,
            Middle,
            Right,
            M4,
            M5
        }

        #endregion Public Enum

        #region Private Static Members

        private static readonly Mouse _instance = new Mouse();

        #endregion Private Static Members

        #region Private Members

        private Dictionary<Button, ButtonState> _buttonsState, _buttonsLastState;

        #endregion Private Members

        #region Constructors

        private Mouse() {
            _buttonsState = new Dictionary<Button, ButtonState>();
            _buttonsLastState = new Dictionary<Button, ButtonState>();
            foreach (Button id in System.Enum.GetValues(typeof(Button))) {
                _buttonsState[id] = ButtonState.Released;
                _buttonsLastState[id] = ButtonState.Released;
            }
        }

        #endregion Constructors

        #region Public Static Properties

        public static Mouse Instance { get { return _instance; } }
        public static Vector2 ScreenPosition { get; private set; }
        public static Vector2 Position { get; private set; }
        public static float ScreenX { get { return ScreenPosition.X; } }
        public static float ScreenY { get { return ScreenPosition.Y; } }
        public static float X { get { return Position.X; } }
        public static float Y { get { return Position.Y; } }
        public static int ScrollWheel { get; private set; }
        public static int ScrollWheelDelta { get; private set; }

        #endregion Public Properties

        #region Public Static Methods

        public static bool IsButtonPressed(Button button) {
            return _instance._buttonsLastState[button] == ButtonState.Released && _instance._buttonsState[button] == ButtonState.Pressed;
        }

        public static bool IsButtonDown(Button button) {
            return _instance._buttonsState[button] == ButtonState.Pressed;
        }

        public static bool IsButtonReleased(Button button) {
            return _instance._buttonsState[button] == ButtonState.Released;
        }

        #endregion Public Methods

        #region Public Methods

        public void Update() {
            MouseState mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            // positions
            ScreenPosition = new Vector2(mouseState.X, mouseState.Y);
            Position = new Vector2(Util.Math.Clamp(mouseState.X, 0, Game.Instance.WindowWidth) / Game.Instance.Scale, Util.Math.Clamp(mouseState.Y, 0, Game.Instance.WindowHeight) / Game.Instance.Scale);

            // buttons
            foreach (KeyValuePair<Button, ButtonState> button in _buttonsState) {
                _buttonsLastState[button.Key] = button.Value;
            }

            _buttonsState[Button.Left] = mouseState.LeftButton;
            _buttonsState[Button.Middle] = mouseState.MiddleButton;
            _buttonsState[Button.Right] = mouseState.RightButton;
            _buttonsState[Button.M4] = mouseState.XButton1;
            _buttonsState[Button.M5] = mouseState.XButton2;

            // scroll
            ScrollWheelDelta = mouseState.ScrollWheelValue - ScrollWheel;
            ScrollWheel = mouseState.ScrollWheelValue;
        }

        #endregion Internal Methods
    }
}
