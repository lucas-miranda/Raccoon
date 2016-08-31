using Microsoft.Xna.Framework.Input;

namespace Raccoon.Input {
    public class Axis {
        private bool usingButtons;
        
        public Axis(float deadzone = 0f) {
            DeadZone = deadzone;
            JoystickId = null;
            usingButtons = false;
        }

        public Axis(Button up, Button right, Button down, Button left) : this() {
            Up = up;
            Right = right;
            Down = down;
            Left = left;
            usingButtons = true;
        }

        public Axis(int[] joystickId, float deadzone = 0f) : this(deadzone) {
            JoystickId = joystickId;
        }

        public int[] JoystickId { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float DeadZone { get; set; }
        public Button Up { get; protected set; }
        public Button Right { get; protected set; }
        public Button Down { get; protected set; }
        public Button Left { get; protected set; }

        internal void UpdateKeys(KeyboardState keyboardState) {
            if (!usingButtons)
                return;

            Up.UpdateKeys(keyboardState);
            Right.UpdateKeys(keyboardState);
            Down.UpdateKeys(keyboardState);
            Left.UpdateKeys(keyboardState);
            Update((Right.Down ? 1 : 0) + (Left.Down ? -1 : 0), (Up.Down ? 1 : 0) + (Down.Down ? -1 : 0));
        }

        internal void UpdateJoys(JoystickState joystickState) {
            if (JoystickId == null)
                return;

            Update(joystickState.Axes[JoystickId[0]], joystickState.Axes[JoystickId[1]]);
        }

        internal void Update(float x, float y) {
            if (DeadZone > 0) {
                Vector2 axisVector = new Vector2(x, y);
                if (axisVector.LengthSquared() < DeadZone * DeadZone) {
                    x = 0;
                    y = 0;
                }
            }

            X = x;
            Y = y;
        }

        public override string ToString() {
            return $"[Axis | X: {X}, Y: {Y}]";
        }
    }
}
