using System.Collections.Generic;
using Raccoon.Util;

namespace Raccoon.Input {
    public class XboxGamepadBackInterfaceTrigger : BackInterfaceTrigger<XboxGamepadDevice> {
        private List<XboxGamepadTriggerInputSource> _sources = new List<XboxGamepadTriggerInputSource>();

        public XboxGamepadBackInterfaceTrigger(XboxGamepadDevice device) : base(device) {
        }

        public XboxGamepadBackInterfaceTrigger Bind(XboxInputLabel.Triggers label) {
            XboxGamepadTriggerInputSource source = Device.CreateTriggerSource(label);
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

        #region Internal Methods

        internal override void Update(int delta) {
            Value = 0f;

            foreach (XboxGamepadTriggerInputSource source in _sources) {
                source.Update(delta);
                Value = Math.Clamp(Value + source.Value, -1f, 1f);
            }
        }

        #endregion Internal Methods
    }
}
