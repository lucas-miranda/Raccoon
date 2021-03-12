using System.Collections.Generic;
using Raccoon.Util;

namespace Raccoon.Input {
    public class XboxGamepadBackInterfaceButton : BackInterfaceButton<XboxGamepadDevice> {
        #region Private Members

        private List<XboxGamepadButtonInputSource> _sources = new List<XboxGamepadButtonInputSource>();
        private List<XboxGamepadTriggerInputSource> _triggerSources = new List<XboxGamepadTriggerInputSource>();

        #endregion Private Members

        #region Constructors

        public XboxGamepadBackInterfaceButton(XboxGamepadDevice device) : base(device) {
        }

        #endregion Constructors

        #region Public Methods

        public XboxGamepadBackInterfaceButton Bind(XboxInputLabel.Buttons label) {
            XboxGamepadButtonInputSource source = Device.CreateButtonSource(label);
            _sources.Add(source);
            SourceAdded(source);
            return this;
        }

        public XboxGamepadBackInterfaceButton Bind(XboxInputLabel.Triggers label) {
            XboxGamepadTriggerInputSource source = Device.CreateTriggerSource(label);
            _triggerSources.Add(source);
            SourceAdded(source);
            return this;
        }

        public XboxGamepadBackInterfaceButton Bind(XboxInputLabel.DPad label) {
            XboxGamepadButtonInputSource source = Device.CreateButtonSource(label);
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
            Value = 0f;

            foreach (XboxGamepadButtonInputSource source in _sources) {
                source.Update(delta);

                if (source.IsDown) {
                    IsDown = true;
                    Value = 1f;
                }
            }

            foreach (XboxGamepadTriggerInputSource source in _triggerSources) {
                source.Update(delta);
                Value = Math.Clamp(Value + source.Value, -1f, 1f);

                if (source.IsDown) {
                    IsDown = true;
                }
            }
        }

        #endregion Internal Methods
    }
}
