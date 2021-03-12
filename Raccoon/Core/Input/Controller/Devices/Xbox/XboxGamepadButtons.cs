
namespace Raccoon.Input {
    public struct XboxGamepadButtons {
        #region Public Members

        public XboxInputLabel.Buttons Buttons;

        #endregion Public Members

        #region Constructors

        public XboxGamepadButtons(XboxInputLabel.Buttons buttons) {
            Buttons = buttons;
        }

        internal XboxGamepadButtons(Microsoft.Xna.Framework.Input.GamePadState xnaState) {
            XboxInputLabel.Buttons buttons = XboxInputLabel.Buttons.None;

            if (xnaState.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.A;
            }

            if (xnaState.Buttons.B == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.B;
            }

            if (xnaState.Buttons.X == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.X;
            }

            if (xnaState.Buttons.Y == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.Y;
            }

            if (xnaState.Buttons.LeftShoulder == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.LB;
            }

            if (xnaState.Buttons.RightShoulder == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.RB;
            }

            if (xnaState.Buttons.LeftStick == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.LeftStick;
            }

            if (xnaState.Buttons.RightStick == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.RightStick;
            }

            if (xnaState.Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.Back;
            }

            if (xnaState.Buttons.Start == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.Start;
            }

            if (xnaState.Buttons.BigButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.BigButton;
            }

            if (xnaState.DPad.Up == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.DUp;
            }

            if (xnaState.DPad.Right == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.DRight;
            }

            if (xnaState.DPad.Down == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.DDown;
            }

            if (xnaState.DPad.Left == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                buttons |= XboxInputLabel.Buttons.DLeft;
            }

            Buttons = buttons;
        }

        #endregion Constructors

        #region Public Methods

        public bool IsDown(XboxInputLabel.Buttons inputLabel) {
            return (Buttons & inputLabel) == inputLabel;
        }

        public bool IsUp(XboxInputLabel.Buttons inputLabel) {
            return (Buttons & inputLabel) != inputLabel;
        }

        #endregion Public Methods
    }
}
