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

        public static IEnumerator WaitEndOf<T>(Animation<T> animation, T trackName, bool autoPlay = false) {
            if (animation == null || !animation.ContainsTrack(trackName)) {
                yield break;
            }

            if (autoPlay) {
                animation.Play(trackName);
            } else if (!animation.CurrentKey.Equals(trackName)) {
                yield break;
            }

            Animation<T>.Track track = animation[trackName];
            if (track.IsLooping) {
                // play a single time and leave
                int timesPlayed = track.TimesPlayed;
                while (track.TimesPlayed == timesPlayed) {
                    yield return null;
                }
            } else {
                while (!track.HasEnded) {
                    yield return null;
                }
            }
        }

        public static IEnumerator WaitEndOf<T>(Animation<T> animation) {
            yield return WaitEndOf(animation, animation.CurrentKey);
        }

        public static IEnumerator Repeat(int times, Func<IEnumerator> routine) {
            while (times > 0) {
                yield return routine();
                times--;
            }
        }
    }
}
