using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Raccoon {
    public class XboxController : Controller {
        public enum Label {
            A, B, X, Y,
            LB, RB, LT, RT,
            LeftStick, RightStick, DPad,
            DUp, DRight, DDown, DLeft,
            Back, Start
        }

        public XboxController(int joyId) : base(joyId) {
            // axes
            AddAxis(Label.LeftStick, new Axis(JoyId, 0, 1));
            AddAxis(Label.RightStick, new Axis(JoyId, 3, 4));

            // dpad
            AddButton(Label.DUp, new Button());
            AddButton(Label.DRight, new Button());
            AddButton(Label.DDown, new Button());
            AddButton(Label.DLeft, new Button());
            AddAxis(Label.DPad, new Axis(Button(Label.DUp), Button(Label.DRight), Button(Label.DDown), Button(Label.DLeft)));

            // buttons
            AddButton(Label.A, new Button(JoyId, 0));
            AddButton(Label.B, new Button(JoyId, 1));
            AddButton(Label.X, new Button(JoyId, 2));
            AddButton(Label.Y, new Button(JoyId, 3));
            AddButton(Label.LB, new Button(JoyId, 4));
            AddButton(Label.RB, new Button(JoyId, 5));
            AddButton(Label.Back, new Button(JoyId, 6));
            AddButton(Label.Start, new Button(JoyId, 7));

            // sticks
            AddButton(Label.LeftStick, new Button(JoyId, 8));
            AddButton(Label.RightStick, new Button(JoyId, 9));

            // triggers
            AddButton(Label.LT, new Trigger(JoyId, 2));
            AddButton(Label.RT, new Trigger(JoyId, 5));
        }

        public XboxController() : this(0) { }

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
                GamePadState gamepadState = GamePad.GetState(JoyId);
                DUp.ForceState(gamepadState.DPad.Up == ButtonState.Pressed);
                DRight.ForceState(gamepadState.DPad.Right == ButtonState.Pressed);
                DDown.ForceState(gamepadState.DPad.Down == ButtonState.Pressed);
                DLeft.ForceState(gamepadState.DPad.Left == ButtonState.Pressed);
            }

            base.Update();
        }

        public override string ToString() {
            string s = $"[XboxController | Id: {JoyId}, Connected? {IsConnected} ";
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
