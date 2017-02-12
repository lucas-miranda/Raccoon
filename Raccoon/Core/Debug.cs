using System;
using System.IO;
using System.Diagnostics;
using Raccoon.Graphics;

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

        public static bool ShowPerformanceDiagnostics { get; set; }
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
        public static void DrawString(Vector2 position, Color color, string message) {
            Game.Instance.Core.DebugSurface.DrawString(Game.Instance.Core.StdFont, message, position, color);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Vector2 position, string message) {
            DrawString(position, Color.White, message);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Vector2 position, Color color, string format, params object[] args) {
            DrawString(position, color, string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void DrawString(Vector2 position, string format, params object[] args) {
            DrawString(position, Color.White, format, args);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, Vector2 position, Color color, string message) {
            if (camera == null) {
                DrawString(position, color, message);
                return;
            }

            Game.Instance.Core.DebugSurface.DrawString(Game.Instance.Core.StdFont, message, camera.Position * camera.Zoom * Game.Instance.Scale + position, color);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, Vector2 position, string message) {
            if (camera == null) {
                DrawString(position, message);
                return;
            }

            DrawString(camera, position, Color.White, message);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, Vector2 position, Color color, string format, params object[] args) {
            if (camera == null) {
                DrawString(position, color, format, args);
                return;
            }

            DrawString(camera, position, color, string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, Vector2 position, string format, params object[] args) {
            if (camera == null) {
                DrawString(position, format, args);
                return;
            }

            DrawString(camera, position, Color.White, format, args);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Vector2 from, Vector2 to, Color color) {
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DefaultSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DefaultSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DefaultSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(from.X, from.Y, 0), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(to.X, to.Y, 0), Color.White)
                }, 0, 1);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Vector2 from, Vector2 to) {
            DrawLine(from, to, Color.Red);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(float x1, float y1, float x2, float y2, Color color) {
            DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(float x1, float y1, float x2, float y2) {
            DrawLine(x1, y1, x2, y2, Color.Red);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle, Color color) {
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DefaultSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DefaultSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DefaultSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[5] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Left, rectangle.Top, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Right, rectangle.Top, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Right, rectangle.Bottom, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Left, rectangle.Bottom, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Left, rectangle.Top, 0f), Color.White)
                }, 0, 4);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle) {
            DrawRectangle(rectangle, Color.Red);
        }

        [Conditional("DEBUG")]
        public static void Log(string message) {
            if (LogFileWriter == null) {
                LogFileWriter = new StreamWriter(string.Format("{0}/log-{1}.txt", Directory.GetCurrentDirectory(), DateTime.Now.ToString("MMddyy-HHmmss")));
                LogFileWriter.WriteLine($"{DateTime.Now.ToString()}  Log file created");
            }

            LogFileWriter.WriteLine($"{DateTime.Now.ToString()}  {new string(' ', IndentSize * IndentLevel)}{message}");
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
