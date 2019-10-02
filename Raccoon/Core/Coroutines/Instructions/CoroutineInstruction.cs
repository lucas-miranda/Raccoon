using System.Collections;

namespace Raccoon {
    public abstract class CoroutineInstruction : IEnumerator {
        public enum Signal {
            /// <summary>
            /// Does nothing.
            /// </summary>
            None = 0,

            /// <summary>
            /// Continue the coroutine execution to next "iteration".
            /// </summary>
            Continue
        };

        /// <summary>
        /// Context dependant value.
        /// </summary>
        public abstract object Current { get; }

        /// <summary>
        /// Coroutine asks to retrieve a routine from current instruction.
        /// Multiple routines can be yielded.
        /// An Signal.Continue marks the end of routines' retrieve stream.
        /// </summary>
        /// <returns>Current CoroutineInstruction IEnumerator, where IEnumerator.Current can be either a sub routine IEnumerator or any CoroutineInstruction.Signal value.</returns>
        public abstract IEnumerator RetrieveRoutine();

        /// <summary>
        /// Check if instruction still needs to execute or not.
        /// </summary>
        /// <returns>True, if instruction is still executing, False otherwise.</returns>
        public abstract bool MoveNext();

        /// <summary>
        /// Callback from Coroutine to be able to handle the current sub routine Coroutine.MoveNext() return value.
        /// </summary>
        /// <param name="moveNextRet">Current sub routine Coroutine.MoveNext() return value.</param>
        public abstract void RoutineMoveNextCallback(bool moveNextRet);

        public void Reset() {
            throw new System.InvalidOperationException("Can't reset a CoroutineInstruction.");
        }
    }
}
