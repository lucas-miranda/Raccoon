using System.Collections;

namespace Raccoon {
    public static class Routines {
        public static IEnumerator Wait(int mili) {
            if (mili <= 0) {
                yield break;
            }

            int elapsedTime = 0;
            while (elapsedTime < mili) {
                elapsedTime += Game.Instance.DeltaTime;
                yield return 0;
            }
        }

        public static IEnumerator Wait(float seconds) {
            return Wait((int) (seconds * Util.Time.SecToMili));
        }

        public static IEnumerator WaitEndOf(int id) {
            while (Coroutine.Instance.Exists(id)) {
                yield return 0;
            }
        }

        public static IEnumerator WaitForToken(object token) {
            while (!Coroutine.Instance.HasToken(token)) {
                yield return 0;
            }

            Coroutine.Instance.ConsumeToken(token);
        }
    }
}
