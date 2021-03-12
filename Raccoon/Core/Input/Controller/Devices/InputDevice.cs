
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
            OnConnected?.Invoke();
        }

        protected void Disconnect() {
            if (!IsConnected) {
                return;
            }

            IsConnected = false;
            OnDisconnected?.Invoke();
        }

        protected void ReceiveAnyInput() {
            OnReceiveAnyInput?.Invoke();
        }
    }
}
