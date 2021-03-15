using Raccoon.Graphics;

namespace Raccoon.Input {
    using XNAGamepad = Microsoft.Xna.Framework.Input.GamePad;

    public class GamepadDevice : InputDevice {
        #region Private Members

        private string _name;
        private Color _lightbar = Color.Black;
        private float _leftMotorVibration,
                      _rightMotorVibration,
                      _leftTriggerVibration,
                      _rightTriggerVibration;

        #endregion Private Members

        #region Constructors

        public GamepadDevice(int id) {
            Id = id;
        }

        public GamepadDevice(GamepadIndex index) {
            if (index == GamepadIndex.Other) {
                throw new System.InvalidOperationException($"To use other indices, please use {nameof(GamepadDevice)}(int id) constructor directly.");
            }

            Id = (int) index;
        }

        #endregion Constructors

        #region Public Properties

        public override string Name { get { return _name; } }
        public int Id { get; private set; }
        public GamepadCapabilities Capabilities { get; private set; }
        public GamepadIndex Index { get { return Capabilities.Index; } }

        public (float X, float Y, float Z) Gyro {
            get {
                if (XNAGamepad.GetGyroEXT(PlayerIndex, out Microsoft.Xna.Framework.Vector3 gyro)) {
                    return (gyro.X, gyro.Y, gyro.Z);
                }

                return (0f, 0f, 0f);
            }
        }

        public (float X, float Y, float Z) Accelerometer {
            get {
                if (XNAGamepad.GetAccelerometerEXT(PlayerIndex, out Microsoft.Xna.Framework.Vector3 accelerometer)) {
                    return (accelerometer.X, accelerometer.Y, accelerometer.Z);
                }

                return (0f, 0f, 0f);
            }
        }

        public Color LightBar {
            get {
                return _lightbar;
            }

            set {
                _lightbar = value;
                XNAGamepad.SetLightBarEXT(PlayerIndex, _lightbar);
            }
        }

        public float LeftMotorVibration {
            get {
                return _leftMotorVibration;
            }
            
            set {
                SetMotorVibration(value, RightMotorVibration);
            }
        }

        public float RightMotorVibration {
            get {
                return _rightMotorVibration;
            }
            
            set {
                SetMotorVibration(LeftMotorVibration, value);
            }
        }

        public float LeftTriggerVibration {
            get {
                return _leftTriggerVibration;
            }
            
            set {
                SetTriggerVibration(value, RightTriggerVibration);
            }
        }

        public float RightTriggerVibration {
            get {
                return _rightTriggerVibration;
            }
            
            set {
                SetTriggerVibration(LeftTriggerVibration, value);
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected Microsoft.Xna.Framework.PlayerIndex PlayerIndex { get { return (Microsoft.Xna.Framework.PlayerIndex) Id; } }
        protected Microsoft.Xna.Framework.Input.GamePadState? RawState { get; private set; }

        #endregion Protected Properties

        #region Public Methods

        public override void Update(int delta) {
            base.Update(delta);

            if (Input.TryGetGamepadState((GamepadIndex) Id, out Microsoft.Xna.Framework.Input.GamePadState state)) {
                RawState = state;
            } else if (RawState != null) {
                RawState = null;
            }

            if (IsConnected) {
                if (!RawState.HasValue || !RawState.Value.IsConnected) {
                    Disconnect();
                }
            } else {
                if (RawState.HasValue && RawState.Value.IsConnected) {
                    Connect();
                }
            }
        }

        public void SetMotorVibration(float left, float right) {
            _leftMotorVibration = left;
            _rightMotorVibration = right;
            XNAGamepad.SetVibration(PlayerIndex, left, right);
        }

        public void SetTriggerVibration(float left, float right) {
            _leftTriggerVibration = left;
            _rightTriggerVibration = right;
            XNAGamepad.SetTriggerVibrationEXT(PlayerIndex, left, right);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Connected() {
            base.Connected();
            Capabilities = new GamepadCapabilities(Id);
            _name = Microsoft.Xna.Framework.Input.GamePad.GetGUIDEXT((Microsoft.Xna.Framework.PlayerIndex) Id);
        }

        protected override void Disconnected() {
            base.Disconnected();
            Capabilities = GamepadCapabilities.Empty;
            _name = string.Empty;
            _lightbar = Color.Black;
            _leftMotorVibration = _rightMotorVibration = 0f;
            _leftTriggerVibration = _rightTriggerVibration = 0f;
        }

        #endregion Protected Methods
    }
}
