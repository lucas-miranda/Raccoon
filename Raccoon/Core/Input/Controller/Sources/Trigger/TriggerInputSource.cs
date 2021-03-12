
namespace Raccoon.Input {
    public abstract class TriggerInputSource<D> : IInputSource<D> where D : InputDevice {
        public TriggerInputSource(D device) {
            if (device == null) {
                throw new System.ArgumentNullException(nameof(device));
            }

            Device = device;
        }

        public D Device { get; private set; }
        public float Value { get; protected set; }
        public bool IsDown { get; protected set; }
        public bool IsUp { get { return !IsDown; } protected set { IsDown = !value; } }

        public virtual void Update(int delta) {
        }
    }
}
