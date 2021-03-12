using System.Collections.Generic;
using Raccoon.Util;

namespace Raccoon.Input {
    public class Button : InputInterface, IInputButton {
        #region Private Members

        private HashSet<IBackInterfaceButton> _buttonSources = new HashSet<IBackInterfaceButton>();

        #endregion Private Members

        #region Constructors

        public Button() : base() {
        }

        #endregion Constructors

        #region Public Properties

        public float Value { get; private set; }
        public ButtonState State { get; private set; }
        public bool IsPressed { get; private set; }
        public bool IsReleased { get; private set; }
        public bool IsDown { get; private set; }
        public bool IsUp { get { return !IsDown; } private set { IsDown = !value; } }
        public uint HoldDuration { get; private set; }

        #endregion Public Properties

        #region Internal Methods

        internal override void Update(int delta) {
            bool isAnySourceDown = false;
            Value = 0f;

            foreach (IBackInterfaceButton source in _buttonSources) {
                if (source.IsDown) {
                    isAnySourceDown = true;
                    Value = Math.Clamp(Value + source.Value, -1f, 1f);
                }
            }

            if (IsUp) {
                if (isAnySourceDown) {
                    IsPressed = IsDown = true;
                    State = ButtonState.Pressed;
                } else if (IsReleased) {
                    IsReleased = false;
                    State = ButtonState.Down;
                    HoldDuration = 0U;
                }
            } else {
                if (isAnySourceDown) {
                    if (IsPressed) {
                        IsPressed = false;
                        State = ButtonState.Down;
                    }

                    HoldDuration += (uint) delta;
                } else {
                    IsReleased = IsUp = true;
                    State = ButtonState.Released;
                }
            }
        }

        internal void RegisterSource(IBackInterfaceButton source) {
            _buttonSources.Add(source);
        }

        internal void DeregisterSource(IBackInterfaceButton source) {
            _buttonSources.Remove(source);
        }

        #endregion Internal Methods
    }
}
