
namespace Raccoon.Input {
    public class KeyboardDeviceController<T> : InputDeviceController<T, KeyboardDevice> {
        public KeyboardDeviceController(KeyboardDevice device) : base(device) {
        }

        public KeyboardBackInterfaceAxis CreateAxisInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new KeyboardBackInterfaceAxis(Device));
        }

        public KeyboardBackInterfaceButton CreateButtonInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new KeyboardBackInterfaceButton(Device));
        }
    }
}
