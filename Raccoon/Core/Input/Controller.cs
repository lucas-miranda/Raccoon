using System.Collections.Generic;
using System;

namespace Raccoon.Input {
    /*public enum PlayStationLabel {
        Triangle, Square, Circle, Cross,
        L1, R1, L2, R2, L3, R3,
        LeftStick, RightStick, DUp, DRight, DDown, DLeft,
        Select, Start, PS
    }*/

    public class Controller {
        public event Action OnConnected, OnDisconnected;

        private bool _wasConnected;

        public Controller(int joyId) {
            Axes = new Dictionary<string, Axis>();
            Buttons = new Dictionary<string, Button>();
            JoyId = joyId;
        }

        public Controller() : this(-1) { }
        
        public int JoyId { get; set; }
        public bool Enabled { get; set; } = true;
        public bool IsConnected { get { return JoyId <= -1 || Input.IsJoystickConnected(JoyId); } }

        protected Dictionary<string, Axis> Axes { get; set; }
        protected Dictionary<string, Button> Buttons { get; set; }
        
        public virtual void Update() {
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
                btn.Update();
            }

            foreach (Axis axis in Axes.Values) {
                axis.Update();
            }
        }

        public Axis Axis(string label) {
            return Axes[label];
        }

        public Axis Axis(Enum label) {
            return Axes[label.ToString()];
        }

        public Button Button(string label) {
            return Buttons[label];
        }

        public Button Button(Enum label) {
            return Buttons[label.ToString()];
        }

        public Controller AddButton(string label, Button button) {
            Buttons.Add(label, button);
            return this;
        }

        public Controller AddButton(Enum label, Button button) {
            AddButton(label.ToString(), button);
            return this;
        }

        public Controller AddAxis(string label, Axis axis) {
            Axes.Add(label, axis);
            return this;
        }

        public Controller AddAxis(Enum label, Axis axis) {
            AddAxis(label.ToString(), axis);
            return this;
        }

        public override string ToString() {
            string s = $"[Controller | Id: {JoyId}, Connected? {IsConnected} ";
            s += "| Axes:";
            foreach (KeyValuePair<string, Axis> axis in Axes) {
                s += " " + axis.Key + ": " + axis.Value;
            }

            s += "| Buttons:";
            foreach (KeyValuePair<string, Button> button in Buttons) {
                s += " " + button.Key + ": " + button.Value;
            }

            return s + "]";
        }
    }
}
