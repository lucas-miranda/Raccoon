
namespace Raccoon.Input {
    public class KeyboardBackInterfaceButton : BackInterfaceButton<KeyboardDevice> {
        #region Private Members

        private KeyboardButtonInputSource _source;

        #endregion Private Members

        #region Constructors

        public KeyboardBackInterfaceButton(KeyboardDevice device) : base(device) {
        }

        #endregion Constructors

        #region Public Methods

        public KeyboardBackInterfaceButton Bind(Key key) {
            if (_source != null) {
                _source.Push(key);
                SourceModified(_source);
            } else {
                _source = new KeyboardButtonInputSource(Device, key);
                SourceAdded(_source);
            }

            return this;
        }

        public KeyboardBackInterfaceButton Bind(params Key[] keys) {
            if (_source != null) {
                _source.PushRange(keys);
                SourceModified(_source);
            } else {
                _source = new KeyboardButtonInputSource(Device, keys);
                SourceAdded(_source);
            }

            return this;
        }

        public override string ToString() {
            return _source == null ? "none" : _source.ToString();
        }

        #endregion Public Methods

        #region Internal Methods

        internal override void Update(int delta) {
            if (_source != null) {
                _source.Update(delta);
                IsDown = _source.IsDown;
            }
        }

        #endregion Internal Methods
    }
}
