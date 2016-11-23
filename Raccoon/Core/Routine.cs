﻿using System.Collections;

namespace Raccoon {
    public static class Routine {
        public static IEnumerator Wait(float seconds) {
            int elapsedTime = 0;
            int miliseconds = (int) (seconds * Util.Time.SecToMili);
            while (elapsedTime < miliseconds) {
                elapsedTime += Game.Instance.DeltaTime;
                yield return 0;
            }
        }

        public static IEnumerator WaitStartOf(int id) {
            Coroutine instance = Coroutine.Instance;
            while (!instance.Exist(id)) {
                yield return 0;
            }
        }

        public static IEnumerator WaitEndOf(int id) {
            Coroutine instance = Coroutine.Instance;
            while (instance.Exist(id)) {
                yield return 0;
            }
        }

        public static IEnumerator WaitForToken(object token) {
            Coroutine instance = Coroutine.Instance;
            while (!instance.HasToken(token)) {
                yield return 0;
            }

            instance.ConsumeToken(token);
        }
    }
}
