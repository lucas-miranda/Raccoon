
namespace Raccoon.Input {
    public abstract class GamepadThumbStickInputSource<D> : AxisInputSource<D> where D : GamepadDevice {
        public GamepadThumbStickInputSource(D device) : base(device) {
        }

        public int ThumbStickId { get; protected set; }
    }
}
