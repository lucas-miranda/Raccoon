using Raccoon.Util;

namespace Raccoon.Input {
    public class MouseButtonBackInterfaceAxis : BackInterfaceAxis<MouseDevice> {
        private ButtonAxisInputSource<MouseDevice> _source;

        public MouseButtonBackInterfaceAxis(MouseDevice device) : base(device) {
            _source = new ButtonAxisInputSource<MouseDevice>(device);
        }

        public MouseButtonBackInterfaceAxis Bind(Direction direction, MouseButton mouseButton) {
            bool isSourceModified = false;
            MouseButtonInputSource source = new MouseButtonInputSource(Device, mouseButton);

            if ((direction & Direction.Up) == Direction.Up) {
                _source.AddUp(source);
                isSourceModified = true;
            }

            if ((direction & Direction.Right) == Direction.Right) {
                _source.AddRight(source);
                isSourceModified = true;
            }

            if ((direction & Direction.Down) == Direction.Down) {
                _source.AddDown(source);
                isSourceModified = true;
            }

            if ((direction & Direction.Left) == Direction.Left) {
                _source.AddLeft(source);
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

        #region Internal Methods

        internal override void Update(int delta) {
            _source.Update(delta);
            X = Math.Clamp(_source.X, -1f, 1f);
            Y = Math.Clamp(_source.Y, -1f, 1f);
        }

        #endregion Internal Methods
    }
}
