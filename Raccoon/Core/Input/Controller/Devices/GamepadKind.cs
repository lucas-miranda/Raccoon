
namespace Raccoon.Input {
    public enum GamepadKind {
        Unknown = 0,
        Gamepad,
        Wheel,
        ArcadeStick,
        FlightStick,
        DancePad,
        Guitar,
        AlternateGuitar,
        DrumKit,
        BigButtonPad
    }

    public static class GamepadKindExtensions {
        public static Microsoft.Xna.Framework.Input.GamePadType ToXNAGamePadType(this GamepadKind gamepadKind) {
            return (Microsoft.Xna.Framework.Input.GamePadType) ((int) gamepadKind + ((int) Microsoft.Xna.Framework.Input.GamePadType.Unknown - (int) GamepadKind.Unknown));
        }
    }
}
