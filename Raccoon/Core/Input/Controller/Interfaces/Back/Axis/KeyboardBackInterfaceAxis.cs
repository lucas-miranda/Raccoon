using Raccoon.Util;

namespace Raccoon.Input {
    public class KeyboardBackInterfaceAxis : BackInterfaceAxis<KeyboardDevice> {
        #region Private Members

        private ButtonAxisInputSource<KeyboardDevice> _source;

        #endregion Private Members

        #region Constructors

        public KeyboardBackInterfaceAxis(KeyboardDevice device) : base(device) {
            _source = new ButtonAxisInputSource<KeyboardDevice>(device);
        }

        #endregion Constructors

        #region Public Properties

        public ButtonAxisInputSource<KeyboardDevice> Source { get { return _source; } }

        #endregion Public Properties

        #region Public Methods

        public KeyboardBackInterfaceAxis Bind(Key? up, Key? right, Key? down, Key? left) {
            bool isSourceModified = false;

            if (up.HasValue) {
                _source.AddUp(new KeyboardButtonInputSource(Device, up.Value));
                isSourceModified = true;
            }

            if (right.HasValue) {
                _source.AddRight(new KeyboardButtonInputSource(Device, right.Value));
                isSourceModified = true;
            }

            if (down.HasValue) {
                _source.AddDown(new KeyboardButtonInputSource(Device, down.Value));
                isSourceModified = true;
            }

            if (left.HasValue) {
                _source.AddLeft(new KeyboardButtonInputSource(Device, left.Value));
                isSourceModified = true;
            }

            if (isSourceModified) {
                SourceModified(_source);
            }

            return this;
        }

        public override string ToString() {
            return _source == null ? "none" : _source.ToString();
        }

        #endregion Public Methods

        #region Internal Methods

        internal override void Update(int delta) {
            _source.Update(delta);
            X = Math.Clamp(_source.X, -1f, 1f);
            Y = Math.Clamp(_source.Y, -1f, 1f);
        }

        #endregion Internal Methods
    }
}
