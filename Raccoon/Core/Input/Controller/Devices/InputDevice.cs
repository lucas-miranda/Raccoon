
namespace Raccoon.Input {
    public abstract class InputDevice {
        public delegate void DeviceEvent();
        public event DeviceEvent OnConnected,
                                 OnDisconnected,
                                 OnReceiveAnyInput;

        public InputDevice() {
        }

        public bool IsConnected { get; private set; }

        public virtual void Update(int delta) {
        }

        protected void Connect() {
            if (IsConnected) {
                return;
            }

            IsConnected = true;
            Logger.Info($"{GetType().Name} connected!");
            OnConnected?.Invoke();
        }

        protected void Disconnect() {
            if (!IsConnected) {
                return;
            }

            IsConnected = false;
            Logger.Info($"{GetType().Name} disconnected.");
            OnDisconnected?.Invoke();
        }

        protected void ReceiveAnyInput() {
            OnReceiveAnyInput?.Invoke();
        }

        protected virtual void Connected() {
        }

        protected virtual void Disconnected() {
        }
    }
}
