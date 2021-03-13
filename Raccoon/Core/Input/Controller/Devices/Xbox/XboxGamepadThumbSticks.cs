
namespace Raccoon.Input {
    public struct XboxGamepadThumbSticks {
        public Vector2 Left, Right;

        public XboxGamepadThumbSticks(Vector2 left, Vector2 right) {
            Left = left;
            Right = right;
        }

        internal XboxGamepadThumbSticks(Microsoft.Xna.Framework.Input.GamePadThumbSticks xnaThumbSticks) {
            Left = new Vector2(xnaThumbSticks.Left);
            Right = new Vector2(xnaThumbSticks.Right);
        }
        
        public override string ToString() {
            return $"L: {Left}, R: {Right}";
        }
    }
}
