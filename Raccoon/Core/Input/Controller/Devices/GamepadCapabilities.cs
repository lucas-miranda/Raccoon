
namespace Raccoon.Input {
    public struct GamepadCapabilities {
        #region Public Members

        public static readonly GamepadCapabilities Empty = new GamepadCapabilities();

        public int Id;
        public bool IsGamepadConnected;
        public XboxInputLabel.Buttons AvailableButtons;
        public XboxInputLabel.Triggers AvailableTriggers;
        public GamepadFeature AvailableFeatures;
        public GamepadKind Kind;

        #endregion Public Members

        #region Constructors

        public GamepadCapabilities(int gamepadIndex) {
            if (gamepadIndex < 0 || gamepadIndex >= Input.MaxGamePads) {
                throw new System.ArgumentException($"Gamepad index '{gamepadIndex}' is out of valid range [0, {Input.MaxGamePads - 1}]");
            }

                Microsoft.Xna.Framework.Input.GamePadCapabilities capabilities = Microsoft.Xna.Framework.Input.GamePad.GetCapabilities((Microsoft.Xna.Framework.PlayerIndex) gamepadIndex);

            Id = gamepadIndex;
            IsGamepadConnected = capabilities.IsConnected;
            Kind = (GamepadKind) ((int) capabilities.GamePadType + ((int) GamepadKind.Unknown - (int) Microsoft.Xna.Framework.Input.GamePadType.Unknown));

            #region Buttons

            AvailableButtons = XboxInputLabel.Buttons.None;

            if (capabilities.HasAButton) {
                AvailableButtons |= XboxInputLabel.Buttons.A;
            }

            if (capabilities.HasBButton) {
                AvailableButtons |= XboxInputLabel.Buttons.B;
            }

            if (capabilities.HasXButton) {
                AvailableButtons |= XboxInputLabel.Buttons.X;
            }

            if (capabilities.HasYButton) {
                AvailableButtons |= XboxInputLabel.Buttons.Y;
            }

            if (capabilities.HasLeftShoulderButton) {
                AvailableButtons |= XboxInputLabel.Buttons.LB;
            }

            if (capabilities.HasRightShoulderButton) {
                AvailableButtons |= XboxInputLabel.Buttons.RB;
            }

            if (capabilities.HasDPadUpButton) {
                AvailableButtons |= XboxInputLabel.Buttons.DUp;
            }

            if (capabilities.HasDPadRightButton) {
                AvailableButtons |= XboxInputLabel.Buttons.DRight;
            }

            if (capabilities.HasDPadDownButton) {
                AvailableButtons |= XboxInputLabel.Buttons.DDown;
            }

            if (capabilities.HasDPadLeftButton) {
                AvailableButtons |= XboxInputLabel.Buttons.DLeft;
            }

            if (capabilities.HasBackButton) {
                AvailableButtons |= XboxInputLabel.Buttons.Back;
            }

            if (capabilities.HasStartButton) {
                AvailableButtons |= XboxInputLabel.Buttons.Start;
            }

            if (capabilities.HasBigButton) {
                AvailableButtons |= XboxInputLabel.Buttons.BigButton;
            }

            #endregion Buttons

            #region Triggers

            AvailableTriggers = XboxInputLabel.Triggers.None;
            if (capabilities.HasLeftTrigger) {
                AvailableTriggers |= XboxInputLabel.Triggers.LT;
            }

            if (capabilities.HasRightTrigger) {
                AvailableTriggers |= XboxInputLabel.Triggers.RT;
            }

            #endregion Triggers

            #region Features

            AvailableFeatures = GamepadFeature.None;

            if (capabilities.HasLeftXThumbStick) {
                AvailableFeatures |= GamepadFeature.LeftThumbStickHorizontalAxis;
            }

            if (capabilities.HasLeftYThumbStick) {
                AvailableFeatures |= GamepadFeature.LeftThumbStickVerticalAxis;
            }

            if (capabilities.HasRightXThumbStick) {
                AvailableFeatures |= GamepadFeature.RightThumbStickHorizontalAxis;
            }

            if (capabilities.HasRightYThumbStick) {
                AvailableFeatures |= GamepadFeature.RightThumbStickVerticalAxis;
            }

            if (capabilities.HasLeftVibrationMotor) {
                AvailableFeatures |= GamepadFeature.LeftVibrationMotor;
            }

            if (capabilities.HasRightVibrationMotor) {
                AvailableFeatures |= GamepadFeature.RightVibrationMotor;
            }

            if (capabilities.HasVoiceSupport) {
                AvailableFeatures |= GamepadFeature.VoiceSupport;
            }

            if (capabilities.HasLightBarEXT) {
                AvailableFeatures |= GamepadFeature.LightBar;
            }

            if (capabilities.HasTriggerVibrationMotorsEXT) {
                AvailableFeatures |= GamepadFeature.TriggerVibrationMotors;
            }

            if (capabilities.HasMisc1EXT) {
                AvailableFeatures |= GamepadFeature.Misc1;
            }

            if (capabilities.HasPaddle1EXT) {
                AvailableFeatures |= GamepadFeature.Paddle1;
            }

            if (capabilities.HasPaddle2EXT) {
                AvailableFeatures |= GamepadFeature.Paddle2;
            }

            if (capabilities.HasPaddle3EXT) {
                AvailableFeatures |= GamepadFeature.Paddle3;
            }

            if (capabilities.HasPaddle4EXT) {
                AvailableFeatures |= GamepadFeature.Paddle4;
            }

            if (capabilities.HasTouchPadEXT) {
                AvailableFeatures |= GamepadFeature.TouchPad;
            }

            if (capabilities.HasGyroEXT) {
                AvailableFeatures |= GamepadFeature.Gyro;
            }

            if (capabilities.HasAccelerometerEXT) {
                AvailableFeatures |= GamepadFeature.Accelerometer;
            }

            #endregion Features
        }

        public GamepadCapabilities(GamepadIndex index) : this((int) index) {
        }

        #endregion Constructors

        #region Public Properties

        public GamepadIndex Index {
            get {
                if (Id < 0 || Id > (int) GamepadIndex.Sixteen) {
                    return GamepadIndex.Other;
                }

                return (GamepadIndex) Id;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public bool HasButton(XboxInputLabel.Buttons button) {
            return (AvailableButtons & button) == button;
        }

        public bool HasTrigger(XboxInputLabel.Triggers trigger) {
            return (AvailableTriggers & trigger) == trigger;
        }

        public bool HasDirectional(XboxInputLabel.DPad directional) {
            return HasButton(directional.ToButton());
        }

        public bool HasFeature(GamepadFeature feature) {
            return (AvailableFeatures & feature) == feature;
        }

        #endregion Public Methods
    }
}
