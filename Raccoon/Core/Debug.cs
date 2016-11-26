using System;
using System.IO;
using System.Diagnostics;

namespace Raccoon {
    public static class Debug {
        #region Private Static Members

        private static StreamWriter LogFileWriter;

        #endregion Private Static Members

        #region Static Method

        static Debug() {
            System.Diagnostics.Debug.Listeners.Add(new ConsoleTraceListener());
        }

        #endregion Static Method

        #region Public Static Properties

        public static int IndentLevel { get { return System.Diagnostics.Debug.IndentLevel; } set { System.Diagnostics.Debug.IndentLevel = value; } }
        public static int IndentSize { get { return System.Diagnostics.Debug.IndentSize; } set { System.Diagnostics.Debug.IndentSize = value; } }

        #endregion Public Static Properties

        #region Public Static Methods

        [Conditional("DEBUG")]
        public static void Write(object value) {
            System.Diagnostics.Debug.Write(value);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object value) {
            System.Diagnostics.Debug.WriteLine(value);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args) {
            System.Diagnostics.Debug.WriteLine(format, args);
        }

        [Conditional("DEBUG")]
        public static void Critical(string message) {
            System.Diagnostics.Debug.WriteLine(message, "Critical");
        }

        [Conditional("DEBUG")]
        public static void Critical(string format, params object[] args) {
            Critical(string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void Warning(string message) {
            System.Diagnostics.Debug.WriteLine(message, "Warning");
        }

        [Conditional("DEBUG")]
        public static void Warning(string format, params object[] args) {
            Warning(string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void Error(string message) {
            System.Diagnostics.Debug.Fail(message);
        }

        [Conditional("DEBUG")]
        public static void Error(string message, string detailMessage) {
            System.Diagnostics.Debug.Fail(message, detailMessage);
        }

        [Conditional("DEBUG")]
        public static void Error(string message, string detailMessageFormat, params object[] args) {
            Error(message, string.Format(detailMessageFormat, args));
        }

        [Conditional("DEBUG")]
        public static void Info(string message) {
            System.Diagnostics.Debug.WriteLine(message, "Info");
        }

        [Conditional("DEBUG")]
        public static void Info(string format, params object[] args) {
            Info(string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void DrawString(bool allowCameraScroll, Vector2 position, Graphics.Color color, string message) {
            Game.Instance.Core.SpriteBatch.DrawString(Game.Instance.Core.StdFont.SpriteFont, message, (!allowCameraScroll && Game.Instance.Scene != null ? Game.Instance.Scene.Camera.Position * Game.Instance.Scale * Game.Instance.Scene.Camera.Zoom + position : position * Game.Instance.Scene.Camera.Zoom), color);
        }

        [Conditional("DEBUG")]
        public static void DrawString(bool allowCameraScroll, Vector2 position, string message) {
            DrawString(allowCameraScroll, position, Graphics.Color.White, message);
        }

        [Conditional("DEBUG")]
        public static void DrawString(bool allowCameraScroll, Vector2 position, Graphics.Color color, string format, params object[] args) {
            DrawString(allowCameraScroll, position, color, string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void DrawString(bool allowCameraScroll, Vector2 position, string format, params object[] args) {
            DrawString(allowCameraScroll, position, Graphics.Color.White, format, args);
        }

        [Conditional("DEBUG")]
        public static void Log(string message) {
            if (LogFileWriter == null) {
                LogFileWriter = new StreamWriter(Directory.GetCurrentDirectory() + "/log-" + DateTime.Now.ToString("MMddyyyy-HHmmss") + ".txt");
                LogFileWriter.WriteLine(DateTime.Now.ToString() + "  Log file created");
            }

            LogFileWriter.WriteLine(DateTime.Now.ToString() + "  " + new string(' ', IndentSize * IndentLevel) + message);
            LogFileWriter.Flush();
        }

        [Conditional("DEBUG")]
        public static void Log(string format, params object[] args) {
            Log(string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b) {
            System.Diagnostics.Debug.Assert(b);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b, string message) {
            System.Diagnostics.Debug.Assert(b, message);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b, string message, string detailMessage) {
            System.Diagnostics.Debug.Assert(b, message, detailMessage);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b, string message, string detailMessageFormat, params object[] args) {
            System.Diagnostics.Debug.Assert(b, message, detailMessageFormat, args);
        }

        [Conditional("DEBUG")]
        public static void Indent() {
            System.Diagnostics.Debug.Indent();
        }

        [Conditional("DEBUG")]
        public static void Unindent() {
            System.Diagnostics.Debug.Unindent();
        }

        #endregion Public Static Methods
    }
}
