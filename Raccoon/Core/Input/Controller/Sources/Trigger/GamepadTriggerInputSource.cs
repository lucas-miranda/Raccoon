
namespace Raccoon.Input {
    public abstract class GamepadTriggerInputSource<D> : TriggerInputSource<D> where D : GamepadDevice {
        public GamepadTriggerInputSource(D device) : base(device) {
        }

        public int TriggerId { get; protected set; }
    }
}
