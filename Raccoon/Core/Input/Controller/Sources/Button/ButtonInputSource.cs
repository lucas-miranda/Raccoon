
namespace Raccoon.Input {
    public abstract class ButtonInputSource<D> : IInputSource<D> where D : InputDevice {
        public ButtonInputSource(D device) {
            if (device == null) {
                throw new System.ArgumentNullException(nameof(device));
            }

            Device = device;
        }

        public D Device { get; private set; }
        public bool IsDown { get; protected set; }
        public bool IsUp { get { return !IsDown; } protected set { IsDown = !value; } }

        public virtual void Update(int delta) {
        }
    }
}
