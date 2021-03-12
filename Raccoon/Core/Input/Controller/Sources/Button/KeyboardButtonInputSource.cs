using System.Collections.Generic;

namespace Raccoon.Input {
    public class KeyboardButtonInputSource : ButtonInputSource<KeyboardDevice> {
        public KeyboardButtonInputSource(KeyboardDevice device) : base(device) {
            Keys = new Key[0];
        }

        public KeyboardButtonInputSource(KeyboardDevice device, Key key) : base(device) {
            Keys = new Key[] { key };
        }

        public KeyboardButtonInputSource(KeyboardDevice device, IEnumerable<Key> keys) : base(device) {
            List<Key> keysList = new List<Key>();
            foreach (Key key in keys) {
                keysList.Add(key);
            }

            Keys = keysList.ToArray();
        }

        public Key[] Keys { get; private set; }

        public override void Update(int delta) {
            base.Update(delta);

            IsUp = true;
            foreach (Key key in Keys) {
                if (Input.IsKeyDown(key)) {
                    IsDown = true;
                    break;
                }
            }
        }

        public void Push(Key key) {
            Key[] keys = new Key[Keys.Length + 1];
            Keys.CopyTo(keys, 0);
            keys[keys.Length - 1] = key;
            Keys = keys;
        }

        public void PushRange(IEnumerable<Key> keys) {
            int keysLength = 0;
            foreach (Key key in keys) {
                keysLength += 1;
            }

            Key[] newKeys = new Key[Keys.Length + keysLength];
            Keys.CopyTo(newKeys, 0);

            int i = Keys.Length;
            foreach (Key key in keys) {
                newKeys[i] = key;
            }

            Keys = newKeys;
        }

        public override string ToString() {
            return string.Join(", ", Keys);
        }
    }
}
