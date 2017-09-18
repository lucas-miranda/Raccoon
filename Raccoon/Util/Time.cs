﻿using System.Diagnostics;

namespace Raccoon.Util {
    public static class Time {
        public const float MiliToSec = 1f / 1000f;
        public const float SecToMili = 1000f;

        private static Stopwatch _stopWatch = new Stopwatch();

        public static long Ticks { get { return Game.Instance.Core.Time.Ticks; } }
        public static double Milliseconds { get { return Game.Instance.Core.Time.TotalMilliseconds; } }
        public static double Seconds { get { return Game.Instance.Core.Time.TotalSeconds; } }
        public static double Minutes { get { return Game.Instance.Core.Time.TotalMinutes; } }
        public static double Hours { get { return Game.Instance.Core.Time.TotalHours; } }
        public static double Days { get { return Game.Instance.Core.Time.TotalDays; } }

        public static void StartStopwatch() {
            _stopWatch.Restart();
        }

        public static long EndStopwatch() {
            _stopWatch.Stop();
            return _stopWatch.ElapsedMilliseconds;
        }
    }
}
