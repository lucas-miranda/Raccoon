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

        public static IEnumerator WaitEndOf<T>(Animation<T> animation, T trackName, T alternativeTrackName, bool autoPlay = false) {
            if (animation == null || !animation.ContainsTrack(trackName) || !animation.ContainsTrack(alternativeTrackName)) {
                yield break;
            }

            if (autoPlay) {
                animation.Play(trackName);
            }

            Animation<T>.Track track,
                               primaryTrack = animation[trackName],
                               alternativeTrack = animation[alternativeTrackName];

            int trackTimesPlayed,
                primaryTrackTimesPlayed = primaryTrack.TimesPlayed,
                alternativeTrackTimesPlayed = alternativeTrack.TimesPlayed;

            if (animation.CurrentKey.Equals(trackName)) {
                track = primaryTrack;
                trackTimesPlayed = primaryTrackTimesPlayed;
            } else if (animation.CurrentKey.Equals(alternativeTrackName)) {
                track = alternativeTrack;
                trackTimesPlayed = alternativeTrackTimesPlayed;
            } else {
                yield break;
            }

            while ((track.IsLooping && track.TimesPlayed == trackTimesPlayed) || (!track.IsLooping && !track.HasEnded)) {
                yield return null;

                if (track == alternativeTrack && animation.CurrentKey.Equals(trackName)) {
                    track = primaryTrack;
                } else if (track == primaryTrack && animation.CurrentKey.Equals(alternativeTrackName)) {
                    track = alternativeTrack;
                }
            }
        }

        public static IEnumerator WaitEndOf<T>(Animation<T> animation, T trackName, bool autoPlay = false) {
            if (animation == null) {
                yield break;
            }

            if (!animation.ContainsTrack(trackName)) {
                throw new System.ArgumentException($"Track with name '{trackName}' doesn't exists.");
            }

            if (autoPlay) {
                animation.Play(trackName);
            } else if (animation.CurrentTrack == null
             || !animation.IsPlaying
             || !animation.CurrentKey.Equals(trackName)
             || animation.CurrentTrack.HasEnded
            ) {
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
                while (animation.CurrentKey.Equals(trackName) && !track.HasEnded) {
                    yield return null;
                }
            }
        }

        public static IEnumerator WaitEndOf<T>(Animation<T> animation) {
            yield return WaitEndOf(animation, animation.CurrentKey);
        }

        public static IEnumerator WaitEndOf(Coroutine coroutine) {
            if (coroutine == null) {
                yield break;
            }

            while (!coroutine.HasEnded) {
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
