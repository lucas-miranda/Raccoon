
namespace Raccoon.Input {
    public interface IInputSource<D> : IInputSource where D : InputDevice {
        D Device { get; }
    }

    public interface IInputSource {
        void Update(int delta);
    }
}
