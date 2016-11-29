using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Raccoon.Input {
    public class XboxController : Controller {
        public enum Label {
            A, B, X, Y,
            LB, RB, LT, RT,
            LeftStick, RightStick, DPad,
            DUp, DRight, DDown, DLeft,
            Back, Start
        }

        public XboxController(int id) : base(id) {
            Initialize();
        }

        public XboxController() : base() {
            Initialize();
        }

        public Axis LeftStick { get { return Axis(Label.LeftStick); } }
        public Axis RightStick { get { return Axis(Label.RightStick); } }
        public Axis DPad { get { return Axis(Label.DPad); } }
        public Button A { get { return Button(Label.A); } }
        public Button B { get { return Button(Label.B); } }
        public Button X { get { return Button(Label.X); } }
        public Button Y { get { return Button(Label.Y); } }
        public Button LB { get { return Button(Label.LB); } }
        public Button RB { get { return Button(Label.RB); } }
        public Trigger LT { get { return (Trigger) Button(Label.LT); } }
        public Trigger RT { get { return (Trigger) Button(Label.RT); } }
        public Button LeftStickButton { get { return Button(Label.LeftStick); } }
        public Button RightStickButton { get { return Button(Label.RightStick); } }
        public Button DUp { get { return Button(Label.DUp); } }
        public Button DDown { get { return Button(Label.DDown); } }
        public Button DLeft { get { return Button(Label.DLeft); } }
        public Button DRight { get { return Button(Label.DRight); } }
        public Button Back { get { return Button(Label.Back); } }
        public Button Start { get { return Button(Label.Start); } }

        public override void Update() {
            if (Enabled && IsConnected) {
                GamePadState gamepadState = GamePad.GetState(Id);
                DUp.ForceState(gamepadState.DPad.Up == ButtonState.Pressed);
                DRight.ForceState(gamepadState.DPad.Right == ButtonState.Pressed);
                DDown.ForceState(gamepadState.DPad.Down == ButtonState.Pressed);
                DLeft.ForceState(gamepadState.DPad.Left == ButtonState.Pressed);
            }

            base.Update();
        }

        public override string ToString() {
            string s = $"[XboxController | Id: {Id}, Connected? {IsConnected} ";
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

        private void Initialize() {
            // axes
            AddAxis(Label.LeftStick, new Axis(Id, 0, 1));
            AddAxis(Label.RightStick, new Axis(Id, 3, 4));

            // dpad
            AddButton(Label.DUp, new Button());
            AddButton(Label.DRight, new Button());
            AddButton(Label.DDown, new Button());
            AddButton(Label.DLeft, new Button());
            AddAxis(Label.DPad, new Axis(Button(Label.DUp), Button(Label.DRight), Button(Label.DDown), Button(Label.DLeft)));

            // buttons
            AddButton(Label.A, new Button(Id, 0));
            AddButton(Label.B, new Button(Id, 1));
            AddButton(Label.X, new Button(Id, 2));
            AddButton(Label.Y, new Button(Id, 3));
            AddButton(Label.LB, new Button(Id, 4));
            AddButton(Label.RB, new Button(Id, 5));
            AddButton(Label.Back, new Button(Id, 6));
            AddButton(Label.Start, new Button(Id, 7));

            // sticks
            AddButton(Label.LeftStick, new Button(Id, 8));
            AddButton(Label.RightStick, new Button(Id, 9));

            // triggers
            AddButton(Label.LT, new Trigger(Id, 2));
            AddButton(Label.RT, new Trigger(Id, 5));
        }
    }
}
