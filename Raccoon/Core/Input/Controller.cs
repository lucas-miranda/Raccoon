using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Raccoon.Input {
    /*public enum PlayStationLabel {
        Triangle, Square, Circle, Cross,
        L1, R1, L2, R2, L3, R3,
        LeftStick, RightStick, DUp, DRight, DDown, DLeft,
        Select, Start, PS
    }*/

    public class Controller {
        #region Public Members

        public event System.Action OnConnected, OnDisconnected;

        #endregion Public Members

        #region Private Members

        private bool _wasConnected;

        #endregion Private Members

        #region Constructors

        public Controller() {
        }

        public Controller(PlayerIndex gamepadIndex) {
            GamePadIndex = gamepadIndex;
        }

        #endregion Constructors

        #region Public Methods

        public PlayerIndex GamePadIndex { get; set; }
        public bool Enabled { get; set; } = true;
        public bool IsUsingGamePad { get; set; }
        public bool IsConnected { get { return !IsUsingGamePad || Input.IsGamepadConnected(GamePadIndex); } }

        #endregion Public Methods

        #region Protected Methods

        protected Dictionary<string, Axis> Axes { get; } = new Dictionary<string, Axis>();
        protected Dictionary<string, Button> Buttons { get; } = new Dictionary<string, Button>();
        protected Dictionary<string, Trigger> Triggers { get; } = new Dictionary<string, Trigger>();

        #endregion Protected Methods

        #region Public Methods

        public virtual void Update(int delta) {
            // connection events
            if (!_wasConnected && IsConnected) {
                OnConnected?.Invoke();
            } else if (_wasConnected && !IsConnected) {
                OnDisconnected?.Invoke();
            }

            _wasConnected = IsConnected;

            if (!Enabled || !IsConnected) {
                return;
            }

            foreach (Button btn in Buttons.Values) {
                btn.Update(delta);
            }

            foreach (Trigger trigger in Triggers.Values) {
                trigger.Update(delta);
            }

            foreach (Axis axis in Axes.Values) {
                axis.Update(delta);
            }
        }

        public Axis Axis(string label) {
            return Axes[label];
        }

        public Axis Axis(System.Enum label) {
            return Axes[label.ToString()];
        }

        public Button Button(string label) {
            return Buttons[label];
        }

        public Button Button(System.Enum label) {
            return Buttons[label.ToString()];
        }

        public Trigger Trigger(string label) {
            return Triggers[label];
        }

        public Trigger Trigger(System.Enum label) {
            return Triggers[label.ToString()];
        }

        public Controller AddButton(string label, Button button) {
            Buttons.Add(label, button);
            return this;
        }

        public Controller AddButton(System.Enum label, Button button) {
            AddButton(label.ToString(), button);
            return this;
        }

        public Controller AddAxis(string label, Axis axis) {
            Axes.Add(label, axis);
            return this;
        }

        public Controller AddAxis(System.Enum label, Axis axis) {
            AddAxis(label.ToString(), axis);
            return this;
        }

        public Controller AddTrigger(string label, Trigger trigger) {
            Triggers.Add(label, trigger);
            return this;
        }

        public Controller AddTrigger(System.Enum label, Trigger trigger) {
            AddTrigger(label.ToString(), trigger);
            return this;
        }

        public override string ToString() {
            string s = $"[Controller | GamePad Index: {GamePadIndex}, Connected? {IsConnected} ";

            s += "| Axes:";
            foreach (KeyValuePair<string, Axis> axis in Axes) {
                s += $" {axis.Key}: {axis.Value} ";
            }

            s += "| Buttons:";
            foreach (KeyValuePair<string, Button> button in Buttons) {
                s += $" {button.Key}: {button.Value}";
            }

            s += "| Triggers:";
            foreach (KeyValuePair<string, Trigger> trigger in Triggers) {
                s += $" {trigger.Key}: {trigger.Value}";
            }

            return s + "]";
        }

        #endregion Public Methods
    }
}
