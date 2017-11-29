using System;
using System.Collections;

namespace Raccoon.Util {
    public static class Routines {
        public static IEnumerator WaitFor(Func<bool> condition) {
            while (!condition()) {
                yield return null;
            }
        }
    }
}
