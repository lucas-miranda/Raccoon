
namespace Raccoon.Input {
    public class MouseButtonInputSource : ButtonInputSource<MouseDevice> {
        public MouseButtonInputSource(MouseDevice device, MouseButton mouseButton) : base(device) {
            MouseButton = mouseButton;
        }

        public MouseButton MouseButton { get; private set; }

        public override void Update(int delta) {
            base.Update(delta);
            IsDown = Input.IsMouseButtonDown(MouseButton);
        }

        public override string ToString() {
            return MouseButton.ToString();
        }
    }
}
