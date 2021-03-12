
namespace Raccoon.Input {
    public class XboxGamepadDeviceController<T> : InputDeviceController<T, XboxGamepadDevice> {
        public XboxGamepadDeviceController(XboxGamepadDevice device) : base(device) {
        }

        public XboxGamepadBackInterfaceAxis CreateAxisInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new XboxGamepadBackInterfaceAxis(Device));
        }

        public XboxGamepadBackInterfaceButton CreateButtonInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new XboxGamepadBackInterfaceButton(Device));
        }

        public XboxGamepadBackInterfaceTrigger CreateTriggerInterface(T commandLabel) {
            return RegisterInterface(commandLabel, new XboxGamepadBackInterfaceTrigger(Device));
        }
    }
}
