
namespace Raccoon.Input {
    public class KeyboardDevice : InputDevice {
        public KeyboardDevice() {
            Connect();
        }

        public override void Update(int delta) {
            base.Update(delta);

            if (Input.IsAnyKeyPressed()) {
                ReceiveAnyInput();
            }
        }
    }
}
