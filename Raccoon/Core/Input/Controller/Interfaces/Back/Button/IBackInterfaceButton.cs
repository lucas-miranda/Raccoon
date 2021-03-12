
namespace Raccoon.Input {
    public interface IBackInterfaceButton {
        float Value { get; }
        bool IsDown { get; }
        bool IsUp { get; }
    }
}
