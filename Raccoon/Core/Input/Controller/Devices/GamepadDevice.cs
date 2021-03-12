
namespace Raccoon.Input {
    public class GamepadDevice : InputDevice {
        public GamepadDevice(int id) {
            Id = id;
        }

        public GamepadDevice(GamepadIndex index) {
            if (index == GamepadIndex.Other) {
                throw new System.InvalidOperationException($"To use other indices, please use {nameof(GamepadDevice)}(int id) constructor directly.");
            }

            Id = (int) index;
        }

        public int Id { get; private set; }

        public GamepadIndex Index { 
            get {  
                if (Id < 0 || Id > (int) GamepadIndex.Eight) {
                    return GamepadIndex.Other;
                }

                return (GamepadIndex) Id;
            } 
        }

        protected Microsoft.Xna.Framework.Input.GamePadState? RawState { get; private set; }

        public override void Update(int delta) {
            base.Update(delta);

            if (Input.TryGetGamepadState((GamepadIndex) Id, out Microsoft.Xna.Framework.Input.GamePadState state)) {
                RawState = state;
            } else if (RawState != null) {
                RawState = null;
            }

            if (IsConnected) {
                if (!RawState.HasValue || !RawState.Value.IsConnected) {
                    Disconnect();
                }
            } else {
                if (RawState.HasValue && RawState.Value.IsConnected) {
                    Connect();
                }
            }
        }
    }
}
