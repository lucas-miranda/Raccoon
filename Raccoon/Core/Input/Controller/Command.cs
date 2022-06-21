using System.Collections.Generic;

namespace Raccoon.Input {
    public class Command {
        #region Constructors

        public Command() {
        }

        #endregion Constructors

        #region Private Properties

        private Dictionary<System.Type, InputInterface> InputInterfaces { get; set; } = new Dictionary<System.Type, InputInterface>();

        #endregion Private Properties

        #region Public Methods

        public void Update(int delta) {
            foreach (InputInterface inputInterface in InputInterfaces.Values) {
                inputInterface.Update(delta);
            }
        }

        public T As<T>() where T : InputInterface {
            if (!InputInterfaces.TryGetValue(typeof(T), out InputInterface inputInterface)) {
                throw new System.ArgumentException($"There is no input interface of type '{typeof(T)}'.\nAvailable types are: {string.Join(", ", InputInterfaces.Keys)}.", nameof(T));
            }

            return (T) inputInterface;
        }

        public bool ExistsAs<T>() where T : InputInterface {
            return InputInterfaces.ContainsKey(typeof(T));
        }


        public bool TryAs<T>(out T interfaceT) where T : InputInterface {
            if (InputInterfaces.TryGetValue(typeof(T), out InputInterface i)) {
                interfaceT = (T) i;
                return true;
            }

            interfaceT = null;
            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private bool TryGetInterface<I>(out I inputInterface) where I : InputInterface {
            if (InputInterfaces.TryGetValue(typeof(I), out InputInterface retrievedInterface)) {
                if (!(retrievedInterface is I i)) {
                    throw new System.InvalidOperationException($"Registered {nameof(InputInterface)}, at type '{typeof(I).Name}', isn't of that type.\nRegistered type: {retrievedInterface.GetType().Name}");
                }

                inputInterface = i;
                return true;
            }

            inputInterface = null;
            return false;
        }

        private I GetOrCreateInterface<I>() where I : InputInterface, new() {
            I inputInterface;

            if (InputInterfaces.TryGetValue(typeof(I), out InputInterface retrievedInterface)) {
                if (!(retrievedInterface is I i)) {
                    throw new System.InvalidOperationException($"Registered {nameof(InputInterface)}, at type '{typeof(I).Name}', isn't of that type.\nRegistered type: {retrievedInterface.GetType().Name}");
                }

                inputInterface = i;
            } else {
                inputInterface = new I();
                InputInterfaces.Add(typeof(I), inputInterface);
            }

            return inputInterface;
        }

        #endregion Private Methods

        #region Internal Methods

        internal bool RegisterBackInterface(InputBackInterface backInterface) {
            if (backInterface is IBackInterfaceAxis axisBackInterface) {
                Axis axis = GetOrCreateInterface<Axis>();
                axis.RegisterSource(axisBackInterface);
                return true;
            } else if (backInterface is IBackInterfaceButton buttonBackInterface) {
                Button button = GetOrCreateInterface<Button>();
                button.RegisterSource(buttonBackInterface);
                return true;
            } else if (backInterface is IBackInterfaceTrigger triggerBackInterface) {
                Trigger trigger = GetOrCreateInterface<Trigger>();
                trigger.RegisterSource(triggerBackInterface);
                return true;
            }

            return false;
        }

        internal bool DeregisterBackInterface(InputBackInterface backInterface) {
            if (backInterface is IBackInterfaceAxis axisBackInterface) {
                if (TryGetInterface<Axis>(out Axis axis)) {
                    axis.DeregisterSource(axisBackInterface);
                    return true;
                }
            } else if (backInterface is IBackInterfaceButton buttonBackInterface) {
                if (TryGetInterface<Button>(out Button button)) {
                    button.DeregisterSource(buttonBackInterface);
                    return true;
                }
            } else if (backInterface is IBackInterfaceTrigger triggerBackInterface) {
                if (TryGetInterface<Trigger>(out Trigger trigger)) {
                    trigger.DeregisterSource(triggerBackInterface);
                    return true;
                }
            }

            return false;
        }

        internal void InputBackInterfacesChanged(IEnumerable<InputBackInterface> backInterfaces) {
            InputInterfaces.Clear();

            if (backInterfaces != null) {
                foreach (InputBackInterface backInterface in backInterfaces) {
                    RegisterBackInterface(backInterface);
                }
            }
        }

        #endregion Internal Methods
    }
}
