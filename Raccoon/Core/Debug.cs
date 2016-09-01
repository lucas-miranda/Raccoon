using System.Diagnostics;

namespace Raccoon {
    public static class Debug {
        public enum Type {
            Critical,
            Warning,
            Error,
            Info,
            Debug
        }

        static Debug() {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [Conditional("DEBUG")]
        public static void Write(object value) {
            Trace.Write(value);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object value) {
            Trace.WriteLine(value);
        }

        [Conditional("TRACE")]
        public static void Crit(string msg) {
            Trace.WriteLine(msg, "Critical");
        }

        [Conditional("TRACE")]
        public static void Warn(string msg) {
            Trace.WriteLine(msg, "Warning");
        }

        [Conditional("TRACE")]
        public static void Error(string msg) {
            Trace.Fail(msg);
        }

        [Conditional("TRACE")]
        public static void Info(string msg) {
            Trace.WriteLine(msg, "Info");
        }

        [Conditional("TRACE")]
        public static void Assert(bool b, string msg) {
            Trace.Assert(b, msg);
        }

        public static void DrawString(string msg, Vector2 position, Graphics.Color color) {
            Game.Instance.Core.SpriteBatch.DrawString(Game.Instance.Core.StdFont.SpriteFont, msg, position, color);
        }
    }
}
