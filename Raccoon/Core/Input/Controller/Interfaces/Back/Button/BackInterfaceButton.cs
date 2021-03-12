namespace Raccoon.Input {
    public abstract class BackInterfaceButton<D> : InputBackInterface<D>, IBackInterfaceButton where D : InputDevice {
        #region Constructors

        public BackInterfaceButton(D device) : base(device) {
        }

        #endregion Constructors

        #region Public Properties

        public float Value { get; protected set; }
        public bool IsDown { get; protected set; }
        public bool IsUp { get { return !IsDown; } protected set { IsDown = !value; } }

        #endregion Public Properties
    }
}
