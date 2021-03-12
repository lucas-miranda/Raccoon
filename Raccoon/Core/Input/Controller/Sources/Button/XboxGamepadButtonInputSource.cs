namespace Raccoon.Input {
    public class XboxGamepadButtonInputSource : GamepadButtonInputSource<XboxGamepadDevice> {
        public XboxGamepadButtonInputSource(XboxGamepadDevice device, XboxInputLabel.Buttons inputLabel) : base(device) {
            ButtonId = (int) inputLabel;
            InputLabel = inputLabel;
        }

        public XboxInputLabel.Buttons InputLabel { get; private set; }

        public override void Update(int delta) {
            base.Update(delta);
            IsDown = Device.Buttons.IsDown(InputLabel);
        }

        public override string ToString() {
            return InputLabel.ToString();
        }
    }
}
