using System.Collections.Generic;

using Raccoon.Util;

namespace Raccoon.Input {
    public class Trigger : InputInterface, IInputTrigger {
        #region Private Members

        private HashSet<IBackInterfaceTrigger> _sources = new HashSet<IBackInterfaceTrigger>();

        #endregion Private Members

        #region Constructors

        public Trigger() : base() {
        }

        #endregion Constructors

        #region Public Properties

        public float Value { get; private set; }
        //public float Deadzone { get; private set; }
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

            foreach (IBackInterfaceTrigger source in _sources) {
                Value = Math.Clamp(Value + source.Value, -1f, 1f);

                if (Value > Math.Epsilon) {
                    isAnySourceDown = true;
                }
            }

            if (IsUp) {
                if (isAnySourceDown) {
                    IsPressed = IsDown = true;
                    State = ButtonState.Pressed;
                } else if (IsReleased) {
                    IsReleased = false;
                    State = ButtonState.Up;
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
                    IsPressed = false;
                    IsReleased = IsUp = true;
                    State = ButtonState.Released;
                }
            }
        }

        internal void RegisterSource(IBackInterfaceTrigger source) {
            _sources.Add(source);
        }

        internal void DeregisterSource(IBackInterfaceTrigger source) {
            _sources.Remove(source);
        }

        #endregion Internal Methods
    }
}
