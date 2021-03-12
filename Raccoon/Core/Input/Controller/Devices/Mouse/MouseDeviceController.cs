
namespace Raccoon.Input {
    public class MouseDeviceController<T> : InputDeviceController<T, MouseDevice> {
        public MouseDeviceController(MouseDevice device) : base(device) {
        }

        public MouseButtonBackInterfaceAxis CreateButtonAxisInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new MouseButtonBackInterfaceAxis(Device));
        }

        public MouseBackInterfaceButton CreateButtonInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new MouseBackInterfaceButton(Device));
        }
    }
}
