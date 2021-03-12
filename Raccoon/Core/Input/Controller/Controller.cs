using System.Collections.Generic;

namespace Raccoon.Input {
    /*public enum PlayStationLabel {
        Triangle, Square, Circle, Cross,
        L1, R1, L2, R2, L3, R3,
        LeftStick, RightStick, DUp, DRight, DDown, DLeft,
        Select, Start, PS
    }*/

    /// <summary>
    /// Acts as a highly configurable user input interface.
    /// Supporting multiple actions, called Command, and input schemes, called ControllerInputScheme, which relate input sources to it's commands.
    /// </summary>
    /// <typeparam name="T">Command identification label type.</typeparam>
    /// <typeparam name="S">Scheme identification label type.</typeparam>
    public class Controller<T, S> {
        #region Public Members

        public event System.Action OnConnected, 
                                   OnDisconnected;

        public event System.Action<S> OnSchemeChanged; 

        #endregion Public Members

        #region Private Members

        private Dictionary<T, Command> _commands = new Dictionary<T, Command>();
        private Dictionary<S, ControllerInputScheme<T>> _schemes = new Dictionary<S, ControllerInputScheme<T>>();

        #endregion Private Members

        #region Constructors

        public Controller() {
            // commands auto-initialization
            if (typeof(T).IsEnum) {
                foreach (object commandLabel in System.Enum.GetValues(typeof(T))) {
                    CreateCommand((T) commandLabel);
                }
            }

            // schemes auto-initialization
            if (typeof(S).IsEnum) {
                foreach (object schemeLabel in System.Enum.GetValues(typeof(S))) {
                    RegisterScheme((S) schemeLabel);
                }
            }
        }

        #endregion Constructors

        #region Public Properties

        public bool Enabled { get; set; } = true;
        public bool IsConnected { get; private set; }
        public S CurrentSchemeLabel { get; private set; }
        public ControllerInputScheme<T> CurrentScheme { get; private set; }

        public Command this[T commandLabel] {
            get {
                return _commands[commandLabel];
            }
        }

        #endregion Public Properties

        #region Public Methods

        public virtual void Update(int delta) {
            CurrentScheme?.Update(delta);

            foreach (Command command in _commands.Values) {
                command.Update(delta);
            }
        }

        public ControllerInputScheme<T> GetScheme(S schemeLabel) {
            return _schemes[schemeLabel];
        }

        public ControllerInputScheme<T> RegisterScheme(S schemeLabel) {
            if (_schemes.ContainsKey(schemeLabel)) {
                throw new System.InvalidOperationException($"Already exists a scheme with label '{schemeLabel.ToString()}'.");
            }

            ControllerInputScheme<T> scheme = new ControllerInputScheme<T>(_commands);
            _schemes.Add(schemeLabel, scheme);

            if (CurrentScheme == null) {
                CurrentScheme = scheme;
                CurrentSchemeLabel = schemeLabel;
                CurrentScheme.Begin();
            }

            return scheme;
        }

        public void SwitchScheme(S schemeLabel) {
            if (!_schemes.TryGetValue(schemeLabel, out ControllerInputScheme<T> scheme)) {
                throw new System.ArgumentException($"There is no scheme registered with label '{schemeLabel}'.", nameof(schemeLabel));
            }

            if (CurrentScheme != null) {
                CurrentScheme.End();
                CurrentScheme = null;
            }

            CurrentScheme = scheme;
            CurrentSchemeLabel = schemeLabel;
            CurrentScheme.Begin();
            OnSchemeChanged?.Invoke(CurrentSchemeLabel);
        }

        public void AllowAllDevices() {
            foreach (ControllerInputScheme<T> scheme in _schemes.Values) {
                scheme.AllowAllDevices();
            }
        }

        public void BlockAllDevices() {
            foreach (ControllerInputScheme<T> scheme in _schemes.Values) {
                scheme.BlockAllDevices();
            }
        }

        public void AllowDevice<D>() where D : InputDevice {
            foreach (ControllerInputScheme<T> scheme in _schemes.Values) {
                scheme.AllowDevice<D>();
            }
        }

        public void BlockDevice<D>() where D : InputDevice {
            foreach (ControllerInputScheme<T> scheme in _schemes.Values) {
                scheme.BlockDevice<D>();
            }
        }

        public bool IsDeviceAllowed<D>() where D : InputDevice {
            if (CurrentScheme == null) {
                return false;
            }

            return CurrentScheme.IsDeviceAllowed<D>();
        }

        public bool IsDeviceBlocked<D>() where D : InputDevice {
            if (CurrentScheme == null) {
                return false;
            }

            return CurrentScheme.IsDeviceBlocked<D>();
        }

        public bool IsDevicesAllowed(params System.Type[] inputDeviceTypes) {
            if (CurrentScheme == null) {
                return false;
            }

            return CurrentScheme.IsDevicesAllowed(inputDeviceTypes);
        }

        public bool IsDevicesBlocked(params System.Type[] inputDeviceTypes) {
            if (CurrentScheme == null) {
                return false;
            }

            return CurrentScheme.IsDevicesBlocked(inputDeviceTypes);
        }

        public I Command<I>(T commandLabel) where I : InputInterface {
            try {
                return _commands[commandLabel].As<I>();
            } catch (System.Exception e) {
                throw new System.ArgumentException($"When trying to get input interface of type '{typeof(I)}' at label '{commandLabel}'", nameof(commandLabel), e);
            }
        }

        public Axis Axis(T commandLabel) {
            try {
                return _commands[commandLabel].As<Axis>();
            } catch (System.Exception e) {
                throw new System.ArgumentException($"When trying to get {nameof(Axis)} at label '{commandLabel}'", nameof(commandLabel), e);
            }
        }

        public Button Button(T commandLabel) {
            try {
                return _commands[commandLabel].As<Button>();
            } catch (System.Exception e) {
                throw new System.ArgumentException($"When trying to get {nameof(Button)} at label '{commandLabel}'", nameof(commandLabel), e);
            }
        }

        public Trigger Trigger(T commandLabel) {
            try {
                return _commands[commandLabel].As<Trigger>();
            } catch (System.Exception e) {
                throw new System.ArgumentException($"When trying to get {nameof(Trigger)} at label '{commandLabel}'", nameof(commandLabel), e);
            }
        }

        public Command CreateCommand(T label) {
            if (_commands.ContainsKey(label)) {
                throw new System.ArgumentException($"Already exists a command with label '{label}'");
            }

            Command command = new Command();
            _commands.Add(label, command);

            // notify current scheme
            CommandAdded(label, command);

            return command;
        }

        public override string ToString() {
            return $"Enabled? {Enabled}, Connected? {IsConnected}, Scheme label: {CurrentSchemeLabel}; Schemes: {_schemes.Count};";
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Connected() {
            OnConnected?.Invoke();
        }

        protected virtual void Disconnected() {
            OnDisconnected?.Invoke();
        }

        #endregion Protected Methods

        #region Internal Methods

        internal void Connect() {
            if (IsConnected) {
                return;
            }

            IsConnected = true;
            Connected();
        }

        internal void Disconnect() {
            if (!IsConnected) {
                return;
            }

            IsConnected = false;
            Disconnected();
        }

        internal void CommandAdded(T label, Command command) {
            CurrentScheme?.UpdateCommandBackInterfaces(label, command);
        }

        internal void CommandRemoved(T label, Command command) {
            command.InputBackInterfacesChanged(null);
        }

        #endregion Internal Methods
    }

    public class Controller<T> : Controller<T, string> {
        public Controller() : base() {
        }
    }

    public class Controller : Controller<string, string> {
        public Controller() : base() {
        }
    }
}
