
namespace Raccoon.Input {
    public interface IInputButton {
        ButtonState State { get; }
        bool IsPressed { get; }
        bool IsReleased { get; }
        bool IsDown { get; }
        bool IsUp { get; }
        uint HoldDuration { get; }
    }
}
