namespace Raccoon.Input {
    public abstract class BackInterfaceAxis<D> : InputBackInterface<D>, IBackInterfaceAxis where D : InputDevice {
        #region Private Members

        private float _x, _y;

        #endregion Private Members

        #region Constructors

        public BackInterfaceAxis(D device) : base(device) {
        }

        #endregion Constructors

        #region Public Properties

        public bool IsHorizontalInverted { get; set; }
        public bool IsVerticalInverted { get; set; }
        public Vector2 Value { get { return new Vector2(X, Y); } }

        public float X { 
            get { 
                return _x;
            }

            protected set {
                if (IsHorizontalInverted) {
                    _x = -value;
                } else {
                    _x = value;
                }
            }
        }

        public float Y {
            get { 
                return _y;
            }

            protected set {
                if (IsVerticalInverted) {
                    _y = -value;
                } else {
                    _y = value;
                }
            }
        }

        public bool InvertBoth { 
            get {
                return IsHorizontalInverted && IsVerticalInverted;
            }

            set {
                IsHorizontalInverted = IsHorizontalInverted = value;
            }
        }

        #endregion Public Properties

        public BackInterfaceAxis<D> InvertHorizontal(bool invert = true) {
            IsHorizontalInverted = invert;
            return this;
        }

        public BackInterfaceAxis<D> InvertVertical(bool invert = true) {
            IsVerticalInverted = invert;
            return this;
        }
    }
}
