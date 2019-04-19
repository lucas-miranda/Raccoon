using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Raccoon.Input {
    public class XboxController : Controller {
        public enum Label {
            A, B, X, Y,
            LB, RB, LT, RT,
            LeftStick, RightStick, DPad,
            DUp, DRight, DDown, DLeft,
            Back, Start
        }

        #region Constructors

        public XboxController(PlayerIndex gamepadIndex) : base(gamepadIndex) {
            // axes
            AddAxis(Label.LeftStick, new Axis(GamePadIndex, GamePadThumbStick.Left));
            AddAxis(Label.RightStick, new Axis(GamePadIndex, GamePadThumbStick.Right));

            // dpad
            AddButton(Label.DUp, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.DPadUp));
            AddButton(Label.DRight, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.DPadRight));
            AddButton(Label.DDown, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.DPadDown));
            AddButton(Label.DLeft, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.DPadLeft));
            AddAxis(Label.DPad, new Axis(Button(Label.DUp), Button(Label.DRight), Button(Label.DDown), Button(Label.DLeft)));

            // buttons
            AddButton(Label.A, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.A));
            AddButton(Label.B, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.B));
            AddButton(Label.X, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.X));
            AddButton(Label.Y, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.Y));
            AddButton(Label.LB, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.LeftShoulder));
            AddButton(Label.RB, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.RightShoulder));
            AddButton(Label.Back, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.Back));
            AddButton(Label.Start, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.Start));

            // sticks
            AddButton(Label.LeftStick, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.LeftStick));
            AddButton(Label.RightStick, new Button(GamePadIndex, Microsoft.Xna.Framework.Input.Buttons.RightStick));

            // triggers
            AddTrigger(Label.LT, new Trigger(GamePadIndex, GamePadTriggerButton.Left));
            AddTrigger(Label.RT, new Trigger(GamePadIndex, GamePadTriggerButton.Right));
        }

        #endregion Constructors

        #region Public Properties

        public Axis LeftStick { get { return Axis(Label.LeftStick); } }
        public Axis RightStick { get { return Axis(Label.RightStick); } }
        public Axis DPad { get { return Axis(Label.DPad); } }
        public Button A { get { return Button(Label.A); } }
        public Button B { get { return Button(Label.B); } }
        public Button X { get { return Button(Label.X); } }
        public Button Y { get { return Button(Label.Y); } }
        public Button LB { get { return Button(Label.LB); } }
        public Button RB { get { return Button(Label.RB); } }
        public Trigger LT { get { return Trigger(Label.LT); } }
        public Trigger RT { get { return Trigger(Label.RT); } }
        public Button LeftStickButton { get { return Button(Label.LeftStick); } }
        public Button RightStickButton { get { return Button(Label.RightStick); } }
        public Button DUp { get { return Button(Label.DUp); } }
        public Button DDown { get { return Button(Label.DDown); } }
        public Button DLeft { get { return Button(Label.DLeft); } }
        public Button DRight { get { return Button(Label.DRight); } }
        public Button Back { get { return Button(Label.Back); } }
        public Button Start { get { return Button(Label.Start); } }

        #endregion Public Properties

        #region Public Methods

        public override string ToString() {
            string s = $"[XboxController | GamePad Id: {GamePadIndex}, Connected? {IsConnected} ";

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
