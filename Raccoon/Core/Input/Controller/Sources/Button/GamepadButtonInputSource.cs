
namespace Raccoon.Input {
    public abstract class GamepadButtonInputSource<D> : ButtonInputSource<D> where D : GamepadDevice {
        public GamepadButtonInputSource(D device) : base(device) {
        }

        public int ButtonId { get; protected set; }
    }
}
