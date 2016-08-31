using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public enum PlayStationLabel {
        Triangle, Square, Circle, Cross,
        L1, R1, L2, R2, L3, R3,
        LeftStick, RightStick, DUp, DRight, DDown, DLeft,
        Select, Start, PS
    }

    public enum ControllerType {
        None,
        Keyboard,
        Xbox,
        PlayStation
    }

    public class Controller {
        private bool connected;
        protected static Dictionary<int, Controller> controllers;
        protected Dictionary<string, Axis> axes;
        protected Dictionary<string, Button> buttons;

        public delegate void EventHandler();
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        static Controller() {
            controllers = new Dictionary<int, Controller>();
        }

        protected Controller(ControllerType type, int id) {
            axes = new Dictionary<string, Axis>();
            buttons = new Dictionary<string, Button>();
            Id = id;

            if (controllers.ContainsKey(Id)) {
                controllers[Id].Disconnect();
            }

            controllers[Id] = this;
            Type = type;
        }

        public Controller(int id) : this(ControllerType.Keyboard, id) {
        }
        
        public ControllerType Type { get; protected set; }
        public int Id { get; protected set; }
        public bool Enabled { get; set; }

        public bool Connected {
            get { return connected; }
            protected set {
                connected = value;
                if (connected) {
                    if (OnConnected != null)
                        OnConnected();
                } else {
                    if (OnDisconnected != null)
                        OnDisconnected();
                }
            }
        }

        public static Controller Get(int id) {
            return controllers[id];
        }
        
        public virtual void Update(int delta) {
            if (!Enabled)
                return;

            KeyboardState state = Keyboard.GetState();
            foreach (KeyValuePair<string, Axis> axis in axes) {
                axis.Value.UpdateKeys(state);
            }

            foreach (KeyValuePair<string, Button> btn in buttons) {
                btn.Value.UpdateKeys(state);
            }
        }

        public void Connect() {
            if (Connected)
                return;

            if (controllers.ContainsKey(Id)) {
                controllers[Id].Disconnect();
            }

            controllers[Id] = this;
            Enabled = true;
            Connected = true;
        }

        public void Disconnect() {
            Enabled = false;
            Connected = false;
        }

        public Axis Axis(string label) {
            return axes[label];
        }

        public Axis Axis(System.Enum label) {
            return axes[label.ToString()];
        }

        public Button Button(string label) {
            return buttons[label];
        }

        public Button Button(System.Enum label) {
            return buttons[label.ToString()];
        }

        public void AddButton(string label, Button button) {
            buttons[label] = button;
        }

        public void AddButton(System.Enum label, Button button) {
            buttons[label.ToString()] = button;
        }

        public void AddAxis(string label, Axis axis) {
            axes[label] = axis;
        }

        public void AddAxis(System.Enum label, Axis axis) {
            axes[label.ToString()] = axis;
        }

        public override string ToString() {
            return $"[Controller | Id: {Id}, Connected? {Connected}]";
        }
    }
}
