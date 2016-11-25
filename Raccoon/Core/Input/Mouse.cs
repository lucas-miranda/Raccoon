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

        private static Mouse instance;

        #endregion Private Static Members

        #region Private Members

        private Dictionary<Button, ButtonState> buttons;

        #endregion Private Members

        #region Constructors

        private Mouse() {
            buttons = new Dictionary<Button, ButtonState>();
            foreach (Button id in System.Enum.GetValues(typeof(Button))) {
                buttons[id] = ButtonState.Released;
            }
        }

        #endregion Constructors

        #region Public Static Properties

        public static Mouse Instance {
            get {
                if (instance == null) {
                    instance = new Mouse();
                }

                return instance;
            }
        }

        #endregion Public Static Properties

        #region Public Properties

        public Vector2 ScreenPosition { get; private set; }
        public int ScreenX { get { return (int) ScreenPosition.X; } }
        public int ScreenY { get { return (int) ScreenPosition.Y; } }
        public Vector2 GamePosition { get; private set; }
        public int GameX { get { return (int) GamePosition.X; } }
        public int GameY { get { return (int) GamePosition.Y; } }
        public int X { get { return GameX; } }
        public int Y { get { return GameY; } }
        public int ScrollWheel { get; private set; }
        public int ScrollWheelDelta { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public bool IsButtonDown(Button button) {
            return buttons[button] == ButtonState.Pressed;
        }

        public bool IsButtonReleased(Button button) {
            return buttons[button] == ButtonState.Released;
        }

        #endregion Public Methods

        #region Internal Methods

        internal void Update(int delta) {
            MouseState state = Microsoft.Xna.Framework.Input.Mouse.GetState();
            ScreenPosition = new Vector2(state.X, state.Y);
            GamePosition = new Vector2(Util.Math.Clamp(ScreenPosition.X - Game.Instance.X, 0, Game.Instance.WindowWidth) / Game.Instance.Scale, Util.Math.Clamp(ScreenPosition.Y - Game.Instance.Y, 0, Game.Instance.WindowHeight) / Game.Instance.Scale);

            buttons[Button.Left] = state.LeftButton;
            buttons[Button.Middle] = state.MiddleButton;
            buttons[Button.Right] = state.RightButton;
            buttons[Button.M4] = state.XButton1;
            buttons[Button.M5] = state.XButton2;

            ScrollWheelDelta = state.ScrollWheelValue - ScrollWheel;
            ScrollWheel = state.ScrollWheelValue;
        }

        #endregion Internal Methods
    }
}
