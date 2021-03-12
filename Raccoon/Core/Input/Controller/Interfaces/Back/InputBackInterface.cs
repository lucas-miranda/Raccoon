using System.Collections;

namespace Raccoon.Input {
    public abstract class InputBackInterface<D> : InputBackInterface where D : InputDevice {
        public event System.Action<IInputSource> OnSourceAdded,
                                                 OnSourceModified,
                                                 OnSourceRemoved;

        public InputBackInterface(D device) {
            Device = device;
        }

        public D Device { get; private set; }

        protected S FindSource<S>(ref IEnumerable sources) where S : class, IInputSource<D> {
            foreach (object source in sources) {
                if (source is S s) {
                    return s;
                }
            }

            return null;
        }

        protected void SourceAdded(IInputSource source) {
            OnSourceAdded?.Invoke(source);
        }

        protected void SourceModified(IInputSource source) {
            OnSourceModified?.Invoke(source);
        }

        protected void SourceRemoved(IInputSource source) {
            OnSourceRemoved?.Invoke(source);
        }

        internal abstract void Update(int delta);
    }

    public abstract class InputBackInterface {
    }
}
