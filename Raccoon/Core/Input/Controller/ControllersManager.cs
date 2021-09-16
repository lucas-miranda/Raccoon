using System.Collections.Generic;

namespace Raccoon.Input {
    public class ControllersManager<T, S> {
        #region Private Members

        private List<int> _indexes = new List<int>();
        private Dictionary<int, Controller<T, S>> _controllers = new Dictionary<int, Controller<T, S>>();

        #endregion Private Members

        #region Constructors

        public ControllersManager() {
        }

        #endregion Constructors

        #region Public Properties

        public int? MaxAllowedControllers { get; set; }
        public int RegisteredControllers { get { return _controllers.Count; } }

        public int ConnectedControllers {
            get {
                int count = 0;

                foreach (Controller<T, S> controller in _controllers.Values) {
                    if (controller.IsConnected) {
                        count += 1;
                    }
                }

                return count;
            }
        }

        public int DisconnectedControllers {
            get {
                int count = 0;

                foreach (Controller<T, S> controller in _controllers.Values) {
                    if (!controller.IsConnected) {
                        count += 1;
                    }
                }

                return count;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Update(int delta) {
            foreach (Controller<T, S> controller in _controllers.Values) {
                if (!controller.Enabled || !controller.IsConnected) {
                    continue;
                }

                controller.Update(delta);
            }
        }

        public int? FindControllerIndex(Controller<T, S> controller) {
            foreach (KeyValuePair<int, Controller<T, S>> entry in _controllers) {
                if (entry.Value == controller) {
                    return entry.Key;
                }
            }

            return null;
        }

        public Controller<T, S> RequestController(int customIndex) {
            RequestNextControllerIndex(customIndex);
            Controller<T, S> controller = new Controller<T, S>();
            _controllers.Add(customIndex, controller);
            return controller;
        }

        public Controller<T, S> RequestController(out int index) {
            index = RequestNextControllerIndex();
            Controller<T, S> controller = new Controller<T, S>();
            _controllers.Add(index, controller);
            return controller;
        }

        public Controller<T, S> RequestController() {
            return RequestController(out _);
        }

        public bool RemoveController(int index) {
            _indexes.Remove(index);
            return _controllers.Remove(index);
        }

        public bool RemoveController(Controller<T, S> controller) {
            int index = -1;

            foreach (KeyValuePair<int, Controller<T, S>> entry in _controllers) {
                if (entry.Value == controller) {
                    index = entry.Key;
                }
            }

            if (index < 0) {
                return false;
            }

            return RemoveController(index);
        }

        public void RemoveAllControllers() {
            _controllers.Clear();
            _indexes.Clear();
        }

        public void ConnectController(int index) {
            if (!_controllers.TryGetValue(index, out Controller<T, S> controller)) {
                throw new System.ArgumentException($"There is no Controller registered at index {index}.");
            }

            controller.Connect();
        }

        public void ConnectController(Controller<T, S> controller) {
            if (controller == null) {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (!FindControllerIndex(controller).HasValue) {
                throw new System.ArgumentException("Controller isn't registered.");
            }

            controller.Connect();
        }

        public void DisconnectController(int index) {
            if (!_controllers.TryGetValue(index, out Controller<T, S> controller)) {
                throw new System.ArgumentException($"There is no Controller registered at index {index}.");
            }

            controller.Disconnect();
        }

        public void DisconnectController(Controller<T, S> controller) {
            if (controller == null) {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (!FindControllerIndex(controller).HasValue) {
                throw new System.ArgumentException("Controller isn't registered.");
            }

            controller.Disconnect();
        }

        #endregion Public Methods

        #region Private Methods

        private int RequestNextControllerIndex(int? customIndex = null) {
            if (MaxAllowedControllers.HasValue && _controllers.Count >= MaxAllowedControllers.Value) {
                throw new System.InvalidOperationException($"Max allowed controllers reached.\nMax: {MaxAllowedControllers.Value}\nRegistered: {RegisteredControllers}\nConnected: {ConnectedControllers}\nDisconnected: {DisconnectedControllers}");
            }

            int index = customIndex.GetValueOrDefault(0),
                n;

            for (n = 0; n < _indexes.Count; n++) {
                int registeredIndex = _indexes[n];

                if (registeredIndex == index) {
                    // try next one
                    index += 1;

                    continue;
                } else if (registeredIndex > index) {
                    // accept index
                    break;
                }
            }

            _indexes.Insert(n, index);
            return index;
        }

        #endregion Private Methods
    }
}
