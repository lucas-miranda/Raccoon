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
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value) {
            System.Diagnostics.Trace.Write(value);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value) {
            System.Diagnostics.Trace.WriteLine(value);
        }

        [System.Diagnostics.Conditional("TRACE")]
        public static void Crit(string msg) {
            System.Diagnostics.Trace.WriteLine(msg, "Critical");
        }

        [System.Diagnostics.Conditional("TRACE")]
        public static void Warn(string msg) {
            System.Diagnostics.Trace.WriteLine(msg, "Warning");
        }

        [System.Diagnostics.Conditional("TRACE")]
        public static void Error(string msg) {
            System.Diagnostics.Trace.Fail(msg);
        }

        [System.Diagnostics.Conditional("TRACE")]
        public static void Info(string msg) {
            System.Diagnostics.Trace.WriteLine(msg, "Info");
        }

        [System.Diagnostics.Conditional("TRACE")]
        public static void Assert(bool b, string msg) {
            System.Diagnostics.Trace.Assert(b, msg);
        }

        public static void DrawText(string msg, Vector2 position, Graphics.Color color) {
            Game.Instance.Core.SpriteBatch.DrawString(Game.Instance.Core.StdFont, msg, position, color);
        }
    }
}
