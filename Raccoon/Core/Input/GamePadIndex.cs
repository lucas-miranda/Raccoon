using Microsoft.Xna.Framework;

namespace Raccoon.Input {
    public enum GamePadIndex {
        None = 0,
        One,
        Two,
        Three,
        Four

        /*
        Five,
        Six,
        Seven,
        Eight
        */
    }

    public static class GamePadExtensions {
        public static PlayerIndex ToPlayerIndex(this GamePadIndex gamepadIndex) {
            if (gamepadIndex == GamePadIndex.None) {
                throw new System.InvalidOperationException("GamePadIndex.None doesn't contains a associate PlayerIndex.");
            }

            return (PlayerIndex) ((int) gamepadIndex - 1);
        }
    }
}
