
namespace Raccoon.Input {
    public class XboxGamepadTriggerInputSource : GamepadTriggerInputSource<XboxGamepadDevice> {
        public XboxGamepadTriggerInputSource(XboxGamepadDevice device, XboxInputLabel.Triggers inputLabel) : base(device) {
            TriggerId = (int) inputLabel;
            InputLabel = inputLabel;
        }

        public XboxInputLabel.Triggers InputLabel { get; private set; }

        public override void Update(int delta) {
            base.Update(delta);

            switch (InputLabel) {
                case XboxInputLabel.Triggers.LT:
                    Value = Device.Triggers.Left;
                    IsDown = Device.Triggers.Left > 0f;
                    break;

                case XboxInputLabel.Triggers.RT:
                    Value = Device.Triggers.Right;
                    IsDown = Device.Triggers.Right > 0f;
                    break;

                default:
                    throw new System.NotImplementedException(InputLabel.ToString());
            }
        }

        public override string ToString() {
            return InputLabel.ToString();
        }
    }
}
