using Raccoon.Util;

namespace Raccoon.Input {
    public class XboxGamepadDevice : GamepadDevice {
        public XboxGamepadDevice(int id) : base(id) {
        }

        public XboxGamepadDevice(GamepadIndex index) : base(index) {
        }

        public XboxGamepadButtons Buttons { get; private set; } = new XboxGamepadButtons();
        public XboxGamepadTriggers Triggers { get; private set; } = new XboxGamepadTriggers();
        public XboxGamepadThumbSticks ThumbSticks { get; private set; } = new XboxGamepadThumbSticks();

        public override void Update(int delta) {
            base.Update(delta);

            if (!RawState.HasValue || !IsConnected) {
                return;
            }

            bool hasReceivedInput = false;

            // buttons
            XboxInputLabel.Buttons buttons = XboxInputLabel.Buttons.None;

            if (RawState.HasValue) {
                if (RawState.Value.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.A;
                }

                if (RawState.Value.Buttons.B == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.B;
                }

                if (RawState.Value.Buttons.X == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.X;
                }

                if (RawState.Value.Buttons.Y == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.Y;
                }

                if (RawState.Value.Buttons.LeftShoulder == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.LB;
                }

                if (RawState.Value.Buttons.RightShoulder == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.RB;
                }

                if (RawState.Value.Buttons.LeftStick == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.LeftStick;
                }

                if (RawState.Value.Buttons.RightStick == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.RightStick;
                }

                if (RawState.Value.Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.Back;
                }

                if (RawState.Value.Buttons.Start == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.Start;
                }

                if (RawState.Value.Buttons.BigButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.BigButton;
                }

                if (RawState.Value.DPad.Up == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.DUp;
                }

                if (RawState.Value.DPad.Right == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.DRight;
                }

                if (RawState.Value.DPad.Down == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.DDown;
                }

                if (RawState.Value.DPad.Left == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
                    buttons |= XboxInputLabel.Buttons.DLeft;
                }

                if ((ulong) buttons != 0UL) {
                    hasReceivedInput = true;
                }

                // triggers
                Triggers = new XboxGamepadTriggers(RawState.Value.Triggers);

                if (!hasReceivedInput && (!Math.EqualsEstimate(Triggers.Left, 0f) || !Math.EqualsEstimate(Triggers.Right, 0f))) {
                    hasReceivedInput = true;
                }

                // thumbsticks
                ThumbSticks = new XboxGamepadThumbSticks(RawState.Value.ThumbSticks);

                if (!hasReceivedInput && (!Vector2.EqualsEstimate(ThumbSticks.Left, Vector2.Zero) || !Vector2.EqualsEstimate(ThumbSticks.Right, Vector2.Zero))) {
                    hasReceivedInput = true;
                }
            } else {
                // triggers
                Triggers = new XboxGamepadTriggers();

                // thumbsticks
                ThumbSticks = new XboxGamepadThumbSticks();
            }

            Buttons = new XboxGamepadButtons(buttons);

            if (hasReceivedInput) {
                ReceiveAnyInput();
            }
        }

        public XboxGamepadThumbStickInputSource CreateThumbStickSource(XboxInputLabel.ThumbSticks label) {
            return new XboxGamepadThumbStickInputSource(this, label);
        }

        public XboxGamepadButtonInputSource CreateButtonSource(XboxInputLabel.Buttons label) {
            return new XboxGamepadButtonInputSource(this, label);
        }

        public XboxGamepadButtonInputSource CreateButtonSource(XboxInputLabel.DPad label) {
            return new XboxGamepadButtonInputSource(this, label.ToButton());
        }

        public XboxGamepadTriggerInputSource CreateTriggerSource(XboxInputLabel.Triggers label) {
            return new XboxGamepadTriggerInputSource(this, label);
        }
    }
}
