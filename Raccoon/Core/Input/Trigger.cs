using Microsoft.Xna.Framework;

namespace Raccoon.Input {
    public enum GamePadTriggerButton {
        Left = 0,
        Right
    }

    public class Trigger {
        #region Constructors

        public Trigger(Key key) {
            Key = key;
        } 

        public Trigger(PlayerIndex gamepadIndex, GamePadTriggerButton gamepadTriggerButton) {
            GamePadIndex = gamepadIndex;
            TriggerButton = gamepadTriggerButton;
            UseGamePad = true;
        }

        #endregion Constructors

        #region Public Properties

        public PlayerIndex GamePadIndex { get; set; }
        public GamePadTriggerButton TriggerButton { get; set; }
        public Key Key { get; set; }
        public float Value { get; protected set; }
        public bool UseGamePad { get; set; }
        public bool IsDown { get; protected set; }
        public bool IsPressed { get; protected set; }
        public bool IsReleased { get; protected set; } = true;
        public uint HoldDuration { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            Value = 0;

            if (UseGamePad) {
                if (TriggerButton == GamePadTriggerButton.Left) {
                    Value = Input.GamePadLeftTriggerValue(GamePadIndex);
                } else {
                    Value = Input.GamePadRightTriggerValue(GamePadIndex);
                }
            }

            if (Key != Key.None) {
                Value += Input.IsKeyDown(Key) ? 1f : -1f;
            }

            Value = Util.Math.Clamp(Value, -1f, 1f);

            if (Value > -1f) {
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

        public override string ToString() {
            return $"[Trigger |" + (Key != Key.None ? $" Key: {Key}" : " ") + (UseGamePad ? $" GamePad Id: {GamePadIndex} Trigger Button: {TriggerButton}" : "") + $" | Value: {Value} |{(IsReleased ? " Released" : " ") + (IsPressed ? " Pressed" : "") + (IsDown ? " Down" : "")}]";
        }

        #endregion Public Methods
    }
}
