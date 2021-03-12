
namespace Raccoon.Input {
    public abstract class AxisInputSource<D> : IInputSource<D> where D : InputDevice {
        #region Constructors

        public AxisInputSource(D device) {
            if (device == null) {
                throw new System.ArgumentNullException(nameof(device));
            }

            Device = device;
        }

        #endregion Constructors

        #region Public Properties

        public D Device { get; private set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }

        #endregion Public Properties

        #region Public Methods

        public virtual void Update(int delta) {
        }

        #endregion Public Methods
    }
}
