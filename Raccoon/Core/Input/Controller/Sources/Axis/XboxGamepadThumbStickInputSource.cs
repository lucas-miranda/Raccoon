
namespace Raccoon.Input {
    public class XboxGamepadThumbStickInputSource : GamepadThumbStickInputSource<XboxGamepadDevice> {
        public XboxGamepadThumbStickInputSource(XboxGamepadDevice device, XboxInputLabel.ThumbSticks inputLabel) : base(device) {
            InputLabel = inputLabel;
            ThumbStickId = (int) InputLabel;
        }

        public XboxInputLabel.ThumbSticks InputLabel { get; private set; }

        public override void Update(int delta) {
            base.Update(delta);

            switch (InputLabel) {
                case XboxInputLabel.ThumbSticks.LeftStick:
                    X = Device.ThumbSticks.Left.X;
                    Y = Device.ThumbSticks.Left.Y;
                    break;

                case XboxInputLabel.ThumbSticks.RightStick:
                    X = Device.ThumbSticks.Right.X;
                    Y = Device.ThumbSticks.Right.Y;
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
