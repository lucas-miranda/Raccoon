using System.Collections.Generic;

namespace Raccoon.Input {
    public class MouseBackInterfaceButton : BackInterfaceButton<MouseDevice> {
        #region Private Members

        private List<MouseButtonInputSource> _sources = new List<MouseButtonInputSource>();

        #endregion Private Members

        #region Constructors

        public MouseBackInterfaceButton(MouseDevice device) : base(device) {
        }

        #endregion Constructors

        #region Public Methods

        public MouseBackInterfaceButton Bind(MouseButton mouseButton) {
            MouseButtonInputSource source = new MouseButtonInputSource(Device, mouseButton);
            _sources.Add(source);
            SourceAdded(source);
            return this;
        }

        public override string ToString() {
            if (_sources.Count == 0) {
                return "none";
            }

            return string.Join("; ", _sources);
        }

        #endregion Public Methods

        #region Internal Methods

        internal override void Update(int delta) {
            IsUp = true;
            foreach (MouseButtonInputSource source in _sources) {
                source.Update(delta);

                if (source.IsDown) {
                    IsDown = source.IsDown;
                }
            }
        }

        #endregion Internal Methods
    }
}
