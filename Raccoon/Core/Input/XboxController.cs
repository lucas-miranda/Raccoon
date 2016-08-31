using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public class XboxController : Controller {
        public enum Label {
            A, B, X, Y,
            LB, RB, LT, RT, Trigger,
            LeftStick, RightStick, DPad,
            Up, Right, Down, Left,
            Back, Start, Guide
        }

        public XboxController(int id, int port) : base(ControllerType.Xbox, id) {
            AddAxis(Label.LeftStick, new Axis(new int[] { 0, 1 }));
            AddAxis(Label.RightStick, new Axis(new int[] { 3, 4 }));
            AddAxis(Label.Trigger, new Axis(new int[] { 2, 5 }));
            AddButton(Label.Up, new Button());
            AddButton(Label.Right, new Button());
            AddButton(Label.Down, new Button());
            AddButton(Label.Left, new Button());
            AddAxis(Label.DPad, new Axis(Button(Label.Up), Button(Label.Right), Button(Label.Down), Button(Label.Left)));
            AddButton(Label.A, new Button(0));
            AddButton(Label.B, new Button(1));
            AddButton(Label.X, new Button(2));
            AddButton(Label.Y, new Button(3));
            AddButton(Label.LB, new Button(4));
            AddButton(Label.RB, new Button(5));
            AddButton(Label.Back, new Button(6));
            AddButton(Label.Start, new Button(7));
            AddButton(Label.LeftStick, new Button(8));
            AddButton(Label.RightStick, new Button(9));
            AddButton(Label.LT, new Button());
            AddButton(Label.RT, new Button());
            AddButton(Label.Guide, new Button());
        }

        public static int MaxPorts { get { return GamePad.MaximumGamePadCount; } }
        public int Port { get; protected set; }
        public Axis LeftStick { get { return Axis(Label.LeftStick); } }
        public Axis RightStick { get { return Axis(Label.RightStick); } }
        public Axis DPad { get { return Axis(Label.DPad); } }
        public Axis Trigger { get { return Axis(Label.Trigger); } }
        public Button A { get { return Button(Label.A); } }
        public Button B { get { return Button(Label.B); } }
        public Button X { get { return Button(Label.X); } }
        public Button Y { get { return Button(Label.Y); } }
        public Button LB { get { return Button(Label.LB); } }
        public Button RB { get { return Button(Label.RB); } }
        public Button LT { get { return Button(Label.LT); } }
        public Button RT { get { return Button(Label.RT); } }
        public Button LeftStickButton { get { return Button(Label.LeftStick); } }
        public Button RightStickButton { get { return Button(Label.RightStick); } }
        public Button Up { get { return Button(Label.Up); } }
        public Button Down { get { return Button(Label.Right); } }
        public Button Left { get { return Button(Label.Down); } }
        public Button Right { get { return Button(Label.Left); } }
        public Button Back { get { return Button(Label.Back); } }
        public Button Start { get { return Button(Label.Start); } }
        public Button Guide { get { return Button(Label.Guide); } }

        public override void Update(int delta) {
            base.Update(delta);

            GamePadState state = GamePad.GetState(Port);
            if (state.IsConnected) {
                if (!Connected)
                    Connected = true;

                GamePadThumbSticks thumbsticks = state.ThumbSticks;
                LeftStick.Update(thumbsticks.Left.X, thumbsticks.Left.Y);
                RightStick.Update(thumbsticks.Right.X, thumbsticks.Right.Y);

                GamePadDPad dpad = state.DPad;
                Up.Update(dpad.Up == ButtonState.Pressed);
                Right.Update(dpad.Right == ButtonState.Pressed);
                Down.Update(dpad.Down == ButtonState.Pressed);
                Left.Update(dpad.Left == ButtonState.Pressed);
                DPad.Update((dpad.Right == ButtonState.Pressed ? 1 : 0) + (dpad.Left == ButtonState.Pressed ? -1 : 0), (dpad.Up == ButtonState.Pressed ? 1 : 0) + (dpad.Down == ButtonState.Pressed ? -1 : 0));

                GamePadTriggers triggers = state.Triggers;
                Trigger.Update(triggers.Left, triggers.Right);
                LT.Update(triggers.Left > 0);
                RT.Update(triggers.Right > 0);

                GamePadButtons buttons = state.Buttons;
                A.Update(buttons.A == ButtonState.Pressed);
                B.Update(buttons.B == ButtonState.Pressed);
                Y.Update(buttons.Y == ButtonState.Pressed);
                X.Update(buttons.X == ButtonState.Pressed);
                LB.Update(buttons.LeftShoulder == ButtonState.Pressed);
                RB.Update(buttons.RightShoulder == ButtonState.Pressed);
                LeftStickButton.Update(buttons.LeftStick == ButtonState.Pressed);
                RightStickButton.Update(buttons.RightStick == ButtonState.Pressed);
                Back.Update(buttons.Back == ButtonState.Pressed);
                Start.Update(buttons.Start == ButtonState.Pressed);
                Guide.Update(buttons.BigButton == ButtonState.Pressed);
            } else if (Connected) {
                Connected = false;
            }
        }

        public override string ToString() {
            return $"[XboxController | Port: {Port}, Connected? {Connected}]";
        }
    }
}
