using Microsoft.Xna.Framework;

namespace Raccoon.Input {
    public enum GamepadIndex {
        One = 0,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Eleven,
        Twelve,
        Thirteen,
        Fourteen,
        Fifteen,
        Sixteen,
        Other = 255
    }

    public static class GamepadExtensions {
        public static PlayerIndex ToPlayerIndex(this GamepadIndex gamepadIndex) {
            if ((int) gamepadIndex > (int) PlayerIndex.Four) {
                throw new System.InvalidOperationException($"{nameof(GamepadIndex)} '{gamepadIndex}' doesn't contains a valid associated {nameof(PlayerIndex)}.");
            }

            return (PlayerIndex) ((int) gamepadIndex);
        }
    }
}
