using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public class Button {
        #region Public Members

        public static readonly Button None = new Button();

        #endregion Public Members

        #region Private Members

        private readonly bool _isUsingMouseButton;
        private bool _forceState;

        #endregion Private Members

        #region Constructors

        public Button() {
        }

        public Button(Key key) {
            Key = key;
        }

        public Button(MouseButton mouseButton) {
            MouseButton = mouseButton;
            _isUsingMouseButton = true;
        }

        public Button(PlayerIndex playerIndex, Buttons gamepadButton) {
            PlayerIndex = playerIndex;
            GamePadButton = gamepadButton;
            UseGamePad = true;
        }

        #endregion Constructors

        #region Public Properties

        public Key Key { get; set; } = Key.None;
        public MouseButton MouseButton { get; set; }
        public PlayerIndex PlayerIndex { get; set; }
        public Buttons GamePadButton { get; set; }
        public bool UseGamePad { get; set; }
        public bool IsDown { get; protected set; }
        public bool IsPressed { get; protected set; }
        public bool IsReleased { get; protected set; } = true;
        public uint HoldDuration { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public virtual void Update(int delta) {
            if (_forceState 
              || (Key != Key.None && Input.IsKeyDown(Key)) 
              || (_isUsingMouseButton && Input.IsMouseButtonDown(MouseButton)) 
              || (UseGamePad && Input.IsGamePadButtonDown(PlayerIndex, GamePadButton))) {
                if (IsReleased) {
                    IsPressed = IsDown = true;
                    IsReleased = false;
                    HoldDuration = 0u;
                } else if (IsPressed) {
                    IsPressed = false;
                } else {
                    HoldDuration += (uint) delta;
                }
            } else {
                if (!IsReleased) {
                    IsReleased = true;
                    IsPressed = IsDown = false;
                    HoldDuration += (uint) delta;
                }
            }
        }

        public void ForceState(bool pressed) {
            _forceState = pressed;
        }

        public override string ToString() {
            return $"[Button |" + (Key != Key.None ? $" Key: {Key}" : " ") + (_isUsingMouseButton ? $" MouseButton: {MouseButton}" : " ") + (UseGamePad ? $" PlayerIndex: {PlayerIndex} GamePadButton: {GamePadButton}" : "") + $" |{(IsReleased ? " Released" : " ") + (IsPressed ? " Pressed" : "") + (IsDown ? " Down" : "")}]";
        }

        #endregion Public Methods
    }
}
