using System.Collections;

namespace Raccoon {
    public abstract class CoroutineInstruction : IEnumerator {
        public enum Signal {
            None = 0,
            Continue
        };

        public abstract object Current { get; }

        public abstract IEnumerator Retrieve();
        public abstract bool MoveNext();
        public abstract void MoveNextResult(bool moveNextRet);

        public void Reset() {
            throw new System.InvalidOperationException("Can't reset a CoroutineInstruction.");
        }
    }
}
