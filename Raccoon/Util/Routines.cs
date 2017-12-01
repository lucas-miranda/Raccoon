using System;
using System.Collections;

using Raccoon.Graphics;

namespace Raccoon.Util {
    public static class Routines {
        public static IEnumerator WaitFor(Func<bool> condition) {
            if (condition == null) {
                yield break;
            }

            while (!condition()) {
                yield return null;
            }
        }

        public static IEnumerator WaitFor(Tween.Tween tween, bool autoPlay = false) {
            if (tween == null) {
                yield break;
            }

            if (autoPlay) {
                tween.Play();
            }

            while (tween.IsPlaying) {
                yield return null;
            }
        }

        public static IEnumerator WaitFor<T>(Animation<T> animation, T trackName, bool autoPlay = false) {
            if (animation == null || !animation.ContainsTrack(trackName)) {
                yield break;
            }

            if (autoPlay) {
                animation.Play(trackName);
            }

            Animation<T>.Track track = animation[trackName];
            while (!track.HasEnded) {
                yield return null;
            }
        }

        public static IEnumerator Repeat(int times, Func<IEnumerator> routine) {
            while (times > 0) {
                yield return routine();
                times--;
            }
        }
    }
}
