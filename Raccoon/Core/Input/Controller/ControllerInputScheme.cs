using System.Collections.Generic;

namespace Raccoon.Input {
    /// <summary>
    /// </summary>
    public sealed class ControllerInputScheme<T> {
        #region Private Members

        private Dictionary<T, Command> _commandsRef;

        /// <summary>Every registered controller by it's unique <see cref="Raccoon.Input.InputDevice">InputDevice</see>.</summary>
        private Dictionary<System.Type, InputDeviceController<T>> _registeredControllers = new Dictionary<System.Type, InputDeviceController<T>>();
        private List<InputDeviceController<T>> _controllers = new List<InputDeviceController<T>>(),
                                               _blockedControllers = new List<InputDeviceController<T>>();

        #endregion Private Members

        #region Constructors

        internal ControllerInputScheme(Dictionary<T, Command> commands) {
            _commandsRef = commands;
        }

        #endregion Constructors

        #region Public Properties

        public bool IsActive { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public InputDeviceController<T, D> GetDevice<D>() where D : InputDevice {
            return (InputDeviceController<T, D>) _registeredControllers[typeof(D)];
        }

        public bool TryGetDevice<D>(out InputDeviceController<T, D> controller) where D : InputDevice {
            if (_registeredControllers.TryGetValue(typeof(D), out InputDeviceController<T> c)) {
                if (!(c is InputDeviceController<T, D> definedC)) {
                    throw new System.InvalidOperationException($"Registered InputDeviceController<{typeof(T)}>, at device type '{typeof(D).Name}', isn't of requested type 'InputDeviceController<{typeof(T)}, {typeof(D)}>'.\nIt's type is '{c.GetType()}'.");
                }

                controller = definedC;
                return true;
            }

            controller = null;
            return false;
        }

        public bool HasDevice<D>() where D : InputDevice {
            return _registeredControllers.ContainsKey(typeof(D));
        }

        public InputDeviceController<T, D> RegisterDevice<D>(D device) where D : InputDevice {
            if (device == null) {
                throw new System.ArgumentNullException(nameof(device));
            }

            if (_registeredControllers.ContainsKey(typeof(D))) {
                throw new System.ArgumentException($"Device '{typeof(D).Name}' is already registered. Duplicated entries isn't allowed.", nameof(device));
            }

            InputDeviceController<T, D> controller = new InputDeviceController<T, D>(device);

            controller.OnBackInterfaceAdded += DeviceControllerBackInterfaceAdded;
            controller.OnBackInterfaceRemoved += DeviceControllerBackInterfaceRemoved;

            _controllers.Add(controller);
            _registeredControllers.Add(typeof(D), controller);
            return controller;
        }

        public InputDeviceController<T, D> RegisterDevice<D>() where D : InputDevice, new() {
            if (_registeredControllers.ContainsKey(typeof(D))) {
                throw new System.ArgumentException($"Device '{typeof(D).Name}' is already registered. Duplicated entries isn't allowed.");
            }

            InputDeviceController<T, D> controller = new InputDeviceController<T, D>(new D());

            controller.OnBackInterfaceAdded += DeviceControllerBackInterfaceAdded;
            controller.OnBackInterfaceRemoved += DeviceControllerBackInterfaceRemoved;

            _controllers.Add(controller);
            _registeredControllers.Add(typeof(D), controller);
            return controller;
        }

        public C RegisterDevice<D, C>(D device) 
          where D : InputDevice 
          where C : InputDeviceController<T, D>
        {
            if (device == null) {
                throw new System.ArgumentNullException(nameof(device));
            }

            if (_registeredControllers.ContainsKey(typeof(D))) {
                throw new System.ArgumentException($"Device '{typeof(D).Name}' is already registered. Duplicated entries isn't allowed.");
            }

            C controller = (C) System.Activator.CreateInstance(typeof(C), new object[] { device });

            controller.OnBackInterfaceAdded += DeviceControllerBackInterfaceAdded;
            controller.OnBackInterfaceRemoved += DeviceControllerBackInterfaceRemoved;

            _controllers.Add(controller);
            _registeredControllers.Add(typeof(D), controller);
            return controller;
        }

        public C RegisterDevice<D, C>() 
          where D : InputDevice, new()
          where C : InputDeviceController<T, D>
        {
            if (_registeredControllers.ContainsKey(typeof(D))) {
                throw new System.ArgumentException($"Device '{typeof(D).Name}' is already registered. Duplicated entries isn't allowed.");
            }

            C controller = (C) System.Activator.CreateInstance(typeof(C), new object[] { new D() });

            controller.OnBackInterfaceAdded += DeviceControllerBackInterfaceAdded;
            controller.OnBackInterfaceRemoved += DeviceControllerBackInterfaceRemoved;

            _controllers.Add(controller);
            _registeredControllers.Add(typeof(D), controller);
            return controller;
        }

        public bool IsDeviceAllowed<D>() where D : InputDevice {
            if (_registeredControllers.TryGetValue(typeof(D), out InputDeviceController<T> deviceController)) {
                return deviceController.IsEnabled;
            }

            return false;
        }

        public bool IsDeviceBlocked<D>() where D : InputDevice {
            if (_registeredControllers.TryGetValue(typeof(D), out InputDeviceController<T> deviceController)) {
                return !deviceController.IsEnabled;
            }

            return true;
        }

        public bool IsAnyDeviceAllowed(params System.Type[] inputDeviceTypes) {
            if (inputDeviceTypes.Length == 0) {
                throw new System.ArgumentException("Empty input device types.", nameof(inputDeviceTypes));
            }

            foreach (System.Type inputDeviceType in inputDeviceTypes) {
                if (!typeof(InputDevice).IsAssignableFrom(inputDeviceType)) {
                    throw new System.ArgumentException($"Type '{inputDeviceType?.Name ?? "null"}' isn't an {nameof(InputDevice)} type.");
                }

                if (_registeredControllers.TryGetValue(inputDeviceType, out InputDeviceController<T> deviceController) 
                 && deviceController.IsEnabled
                ) {
                    return true;
                }
            }

            return false;
        }

        public bool IsAnyDeviceBlocked(params System.Type[] inputDeviceTypes) {
            if (inputDeviceTypes.Length == 0) {
                throw new System.ArgumentException("Empty input device types.", nameof(inputDeviceTypes));
            }

            foreach (System.Type inputDeviceType in inputDeviceTypes) {
                if (!typeof(InputDevice).IsAssignableFrom(inputDeviceType)) {
                    throw new System.ArgumentException($"Type '{inputDeviceType?.Name ?? "null"}' isn't an {nameof(InputDevice)} type.");
                }

                if (!_registeredControllers.TryGetValue(inputDeviceType, out InputDeviceController<T> deviceController) 
                 || !deviceController.IsEnabled
                ) {
                    return true;
                }
            }

            return false;
        }

        public bool IsDevicesAllowed(params System.Type[] inputDeviceTypes) {
            if (inputDeviceTypes.Length == 0) {
                throw new System.ArgumentException("Empty input device types.", nameof(inputDeviceTypes));
            }

            foreach (System.Type inputDeviceType in inputDeviceTypes) {
                if (!typeof(InputDevice).IsAssignableFrom(inputDeviceType)) {
                    throw new System.ArgumentException($"Type '{inputDeviceType?.Name ?? "null"}' isn't an {nameof(InputDevice)} type.");
                }

                if (!_registeredControllers.TryGetValue(inputDeviceType, out InputDeviceController<T> deviceController) 
                 || !deviceController.IsEnabled
                ) {
                    return false;
                }
            }

            return true;
        }

        public bool IsDevicesBlocked(params System.Type[] inputDeviceTypes) {
            if (inputDeviceTypes.Length == 0) {
                throw new System.ArgumentException("Empty input device types.", nameof(inputDeviceTypes));
            }

            foreach (System.Type inputDeviceType in inputDeviceTypes) {
                if (!typeof(InputDevice).IsAssignableFrom(inputDeviceType)) {
                    throw new System.ArgumentException($"Type '{inputDeviceType?.Name ?? "null"}' isn't an {nameof(InputDevice)} type.");
                }

                if (_registeredControllers.TryGetValue(inputDeviceType, out InputDeviceController<T> deviceController) 
                 && deviceController.IsEnabled
                ) {
                    return false;
                }
            }

            return true;
        }

        public void AllowAllDevices() {
            foreach (InputDeviceController<T> controller in _blockedControllers) {
                controller.Enable();
            }

            _controllers.AddRange(_blockedControllers);
            _blockedControllers.Clear();
        }

        public void BlockAllDevices() {
            foreach (InputDeviceController<T> controller in _controllers) {
                controller.Disable();
            }

            _blockedControllers.AddRange(_controllers);
            _controllers.Clear();
        }

        public bool AllowDevice<D>() {
            for (int i = 0; i < _blockedControllers.Count; i++) {
                InputDeviceController<T> controller = _blockedControllers[i];

                if (controller.InputDevice is D) {
                    _controllers.Add(controller);
                    _blockedControllers.RemoveAt(i);
                    controller.Enable();
                    return true;
                }
            }

            return false;
        }

        public bool BlockDevice<D>() {
            for (int i = 0; i < _controllers.Count; i++) {
                InputDeviceController<T> controller = _controllers[i];

                if (controller.InputDevice is D) {
                    _blockedControllers.Add(controller);
                    _controllers.RemoveAt(i);
                    controller.Disable();
                    return true;
                }
            }

            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private void DeviceControllerBackInterfaceAdded<D>(T commandLabel, InputBackInterface<D> backInterface) where D : InputDevice {
            //Logger.Info($"device controller back interface added => is active? {IsActive}; label: {commandLabel}, backInterface: {backInterface}");
            if (IsActive) {
                if (!_commandsRef.TryGetValue(commandLabel, out Command command)) {
                    throw new System.InvalidOperationException($"There is no {nameof(Command)} associated with label '{commandLabel}', but exists an InputDeviceController<{typeof(D)}> at that label.");
                }

                command.RegisterBackInterface(backInterface);
            }
        }

        private void DeviceControllerBackInterfaceRemoved<D>(T commandLabel, InputBackInterface<D> backInterface) where D : InputDevice {
            if (IsActive) {
                if (!_commandsRef.TryGetValue(commandLabel, out Command command)) {
                    throw new System.InvalidOperationException($"There is no {nameof(Command)} associated with label '{commandLabel}', but exists an InputDeviceController<{typeof(D)}> at that label.");
                }

                command.DeregisterBackInterface(backInterface);
            }
        }

        #endregion Private Methods

        #region Internal Methods

        internal void Begin() {
            // ensure every command is using InputInterfaces from this scheme
            IsActive = true;
            foreach (KeyValuePair<T, Command> commandEntry in _commandsRef) {
                UpdateCommandBackInterfaces(commandEntry.Key, commandEntry.Value);
            }
        }

        internal void End() {
            // clear every command back interface before ending
            IsActive = false;
            foreach (Command command in _commandsRef.Values) {
                command.InputBackInterfacesChanged(null);
            }
        }

        internal void Update(int delta) {
            foreach (InputDeviceController<T> controller in _controllers) {
                controller.Update(delta);
            }
        }

        internal void UpdateCommandBackInterfaces(T label, Command command) {
            List<InputBackInterface> backInterfaces = new List<InputBackInterface>();

            foreach (InputDeviceController<T> controller in _controllers) {
                if (controller.TryGetBaseInterface(label, out InputBackInterface inputInterface)) {
                    backInterfaces.Add(inputInterface);
                }
            }

            command.InputBackInterfacesChanged(backInterfaces);
        }

        #endregion Internal Methods
    }
}
