namespace Raccoon.Util {
    public static class Time {
        public const float MiliToSec = 1f / 1000f;
        public const float SecToMili = 1000f;

        public static long Ticks { get { return Game.Instance.Core.Time.Ticks; } }
        public static int Milliseconds { get { return Game.Instance.Core.Time.Milliseconds; } }
        public static int Seconds { get { return Game.Instance.Core.Time.Seconds; } }
        public static int Minutes { get { return Game.Instance.Core.Time.Minutes; } }
        public static int Hours { get { return Game.Instance.Core.Time.Hours; } }
        public static int Days { get { return Game.Instance.Core.Time.Days; } }
    }
}
