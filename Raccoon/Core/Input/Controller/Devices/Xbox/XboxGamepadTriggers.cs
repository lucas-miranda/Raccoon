using Raccoon.Util;

namespace Raccoon.Input {
    public struct XboxGamepadTriggers {
        public float Left, Right;

        public XboxGamepadTriggers(float left, float right) {
            Left = Math.Clamp(left, 0f, 1f);
            Right = Math.Clamp(right, 0f, 1f);
        }

        internal XboxGamepadTriggers(Microsoft.Xna.Framework.Input.GamePadTriggers xnaTriggers) {
            Left = xnaTriggers.Left;
            Right = xnaTriggers.Right;
        }

        public override string ToString() {
            return $"L: {Left}, R: {Right}";
        }
    }
}
