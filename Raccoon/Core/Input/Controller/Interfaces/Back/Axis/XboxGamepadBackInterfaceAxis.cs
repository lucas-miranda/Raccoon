using System.Collections;
using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon.Input {
    public class XboxGamepadBackInterfaceAxis : BackInterfaceAxis<XboxGamepadDevice> {
        private List<AxisInputSource<XboxGamepadDevice>> _sources = new List<AxisInputSource<XboxGamepadDevice>>();

        public XboxGamepadBackInterfaceAxis(XboxGamepadDevice device) : base(device) {
        }

        public XboxGamepadBackInterfaceAxis Bind(XboxInputLabel.ThumbSticks label) {
            _sources.Add(Device.CreateThumbStickSource(label));
            return this;
        }

        public XboxGamepadBackInterfaceAxis Bind(XboxInputLabel.Buttons? upLabel, XboxInputLabel.Buttons? rightLabel, XboxInputLabel.Buttons? downLabel, XboxInputLabel.Buttons? leftLabel) {
            IEnumerable sources = _sources;
            ButtonAxisInputSource<XboxGamepadDevice> buttonAxis = FindSource<ButtonAxisInputSource<XboxGamepadDevice>>(ref sources);

            if (buttonAxis == null) {
                buttonAxis = new ButtonAxisInputSource<XboxGamepadDevice>(Device);
                _sources.Add(buttonAxis);
                SourceAdded(buttonAxis);
            }

            bool isSourceModified = false;

            if (upLabel.HasValue) {
                buttonAxis.AddUp(Device.CreateButtonSource(upLabel.Value));
                isSourceModified = true;
            }

            if (rightLabel.HasValue) {
                buttonAxis.AddRight(Device.CreateButtonSource(rightLabel.Value));
                isSourceModified = true;
            }

            if (downLabel.HasValue) {
                buttonAxis.AddDown(Device.CreateButtonSource(downLabel.Value));
                isSourceModified = true;
            }

            if (leftLabel.HasValue) {
                buttonAxis.AddLeft(Device.CreateButtonSource(leftLabel.Value));
                isSourceModified = true;
            }

            if (isSourceModified) {
                SourceModified(buttonAxis);
            }

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
            X = Y = 0f;
            foreach (AxisInputSource<XboxGamepadDevice> source in _sources) {
                source.Update(delta);
                X = Math.Clamp(X + source.X, -1f, 1f);
                Y = Math.Clamp(Y + source.Y, -1f, 1f);
            }
        }

        #endregion Internal Methods
    }
}
