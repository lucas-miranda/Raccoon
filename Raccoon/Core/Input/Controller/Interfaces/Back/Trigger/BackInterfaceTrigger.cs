namespace Raccoon.Input {
    public abstract class BackInterfaceTrigger<D> : InputBackInterface<D>, IBackInterfaceTrigger where D : InputDevice {
        #region Constructors

        public BackInterfaceTrigger(D device) : base(device) {
        }

        #endregion Constructors

        #region Public Properties

        public float Value { get; protected set; }

        #endregion Public Properties
    }
}
