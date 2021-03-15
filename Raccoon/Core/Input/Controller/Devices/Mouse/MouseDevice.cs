
namespace Raccoon.Input {
    public class MouseDevice : InputDevice {
        public MouseDevice() {
            Connect();
        }

        public override string Name { get { return "Mouse"; } }

        public override void Update(int delta) {
            base.Update(delta);
        }
    }
}
