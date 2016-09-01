using System;

namespace Raccoon {
    [Flags]
    public enum Direction {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8,
        UpLeft = Up | Left,
        UpRight = Up | Right,
        DownLeft = Down | Left,
        DownRight = Down | Right/*,
        UpLeftRight = Up | Left | Right,
        UpDownRight = Up | Down | Right,
        DownLeftRight = Down | Left | Right,
        UpDownLeft = Up | Down | Left,
        UpDownLeftRight = Up | Down | Left | Right*/
    }

    public static class DirectionExtensions {
        public static Direction RotateClockwise(this Direction direction) {
            return (Direction) (((int) direction << 1 | ((int) direction >> 3)) & 15);
        }

        public static Direction RotateAntiClockwise(this Direction direction) {
            return (Direction) (((int) direction >> 1 | ((int) direction << 3)) & 15);
        }

        public static Vector2 ToVector2(this Direction direction) {
            return new Vector2((((int) direction >> 1) & 1) - (((int) direction >> 3) & 1), ((int) direction & 1) - (((int) direction >> 2) & 1));
        }

        /*public static Coordinate ToCoordinate(this Direction direction) {
            return new Coordinate((((int) direction >> 1) & 1) - (((int) direction >> 3) & 1), ((int) direction & 1) - (((int) direction >> 2) & 1));
        }*/
    }
}
