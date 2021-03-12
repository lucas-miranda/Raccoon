using System.Collections.Generic;

namespace Raccoon.Input {
    public class InputDeviceController<T, D> : InputDeviceController<T> where D : InputDevice {
        public event System.Action<T, InputBackInterface<D>> OnBackInterfaceAdded,
                                                             OnBackInterfaceRemoved;

        private Dictionary<T, InputBackInterface<D>> _backInterfaces = new Dictionary<T, InputBackInterface<D>>();

        public InputDeviceController(D device) {
            if (device == null) {
                throw new System.ArgumentNullException(nameof(device));
            }

            Device = device;
            DeviceType = Device.GetType();
        }

        public D Device { get; private set; }
        public override InputDevice InputDevice { get { return Device; } }

        public override void Update(int delta) {
            Device.Update(delta);
            foreach (InputBackInterface<D> inputInterface in _backInterfaces.Values) {
                inputInterface.Update(delta);
            }
        }

        public InputBackInterface<D> GetInterface(T commandLabel) {
            return _backInterfaces[commandLabel];
        }

        public override bool TryGetBaseInterface(T commandLabel, out InputBackInterface inputBackInterface) {
            if (_backInterfaces.TryGetValue(commandLabel, out InputBackInterface<D> i)) {
                inputBackInterface = i;
                return true;
            }

            inputBackInterface = null;
            return false;
        }

        public bool TryGetInterface(T commandLabel, out InputBackInterface<D> inputBackInterface) {
            return _backInterfaces.TryGetValue(commandLabel, out inputBackInterface);
        }

        public I CreateInterface<I>(T commandLabel) where I : InputBackInterface<D> {
            if (_backInterfaces.ContainsKey(commandLabel)) {
                throw new System.ArgumentException($"Already exists an input interface with provided command label '{commandLabel}'.", nameof(commandLabel));
            }

            I inputBackInterface = (I) System.Activator.CreateInstance(typeof(T), new object[] { Device });
            _backInterfaces.Add(commandLabel, inputBackInterface);
            BackInterfaceAdded(commandLabel, inputBackInterface);
            return inputBackInterface;
        }

        public I RegisterInterface<I>(T commandLabel, I inputBackInterface) where I : InputBackInterface<D> {
            if (_backInterfaces.ContainsKey(commandLabel)) {
                throw new System.ArgumentException($"Already exists an input interface with provided command label '{commandLabel}'.", nameof(commandLabel));
            }

            _backInterfaces.Add(commandLabel, inputBackInterface);
            BackInterfaceAdded(commandLabel, inputBackInterface);
            return inputBackInterface;
        }

        public IEnumerable<KeyValuePair<T, InputBackInterface<D>>> BackInterfaces() {
            foreach (KeyValuePair<T, InputBackInterface<D>> entry in _backInterfaces) {
                yield return entry;
            }
        }
        
        private void BackInterfaceAdded(T commandLabel, InputBackInterface<D> backInterface) {
            OnBackInterfaceAdded?.Invoke(commandLabel, backInterface);
        }

        private void BackInterfaceRemoved(T commandLabel, InputBackInterface<D> backInterface) {
            OnBackInterfaceRemoved?.Invoke(commandLabel, backInterface);
        }
    }

    public abstract class InputDeviceController<T> {
        internal InputDeviceController() {
        }

        public bool IsEnabled { get; protected set; }
        public System.Type DeviceType { get; protected set; }
        public abstract InputDevice InputDevice { get; }

        public abstract void Update(int delta);
        public abstract bool TryGetBaseInterface(T commandLabel, out InputBackInterface inputInterface);

        public void Enable() {
            if (IsEnabled) {
                return;
            }

            IsEnabled = true;
            Enabled();
        }

        public void Disable() {
            if (!IsEnabled) {
                return;
            }

            IsEnabled = false;
            Disabled();
        }

        protected virtual void Enabled() {
        }

        protected virtual void Disabled() {
        }
    }
}
