
namespace Raccoon.Input {
    public abstract class InputDevice {
        public delegate void DeviceEvent();
        public event DeviceEvent OnConnected,
                                 OnDisconnected,
                                 OnReceiveAnyInput;

        public InputDevice() {
        }

        public abstract string Name { get; }
        public bool IsConnected { get; private set; }

        public virtual void Update(int delta) {
        }

        protected void Connect() {
            if (IsConnected) {
                return;
            }

            IsConnected = true;
            Logger.PushSubject("Input Device");
            Logger.Info($"{Name} connected!");
            Logger.PopSubject();
            OnConnected?.Invoke();
        }

        protected void Disconnect() {
            if (!IsConnected) {
                return;
            }

            IsConnected = false;
            Logger.PushSubject("Input Device");
            Logger.Info($"{Name} disconnected.");
            Logger.PopSubject();
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
