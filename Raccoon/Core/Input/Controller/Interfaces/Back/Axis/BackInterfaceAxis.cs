namespace Raccoon.Input {
    public abstract class BackInterfaceAxis<D> : InputBackInterface<D>, IBackInterfaceAxis where D : InputDevice {
        #region Constructors

        public BackInterfaceAxis(D device) : base(device) {
        }

        #endregion Constructors

        #region Public Properties

        public float X { get; protected set; }
        public float Y { get; protected set; }
        public Vector2 Value { get { return new Vector2(X, Y); } }

        #endregion Public Properties
    }
}
