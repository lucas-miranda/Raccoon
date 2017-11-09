using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Util.Tween;

namespace Raccoon {
    public sealed class Debug {
        #region Public Static Members

        public static readonly string LogFileName = "report.log";

        #endregion Public Static Members

        #region Private Static Members

        private const int MessagesSpacing = 5;
        private static readonly Vector2 ScreenMessageStartPosition = new Vector2(15, Game.Instance.WindowHeight - 30);
        private static readonly Lazy<Debug> _lazy = new Lazy<Debug>(() => new Debug());

        #endregion Private Static Members

        #region Private Members

        private bool _useLogToFile;
        private TextWriterTraceListener _textWriterTraceListener = new TextWriterTraceListener(LogFileName, "logger");
        private Vector2 _screenMessagePosition = ScreenMessageStartPosition;
        private List<Message> _messagesList = new List<Message>(), _toRemoveMessages = new List<Message>();

        #endregion Private Members

        #region Constructors

        private Debug() {
            //Trace.Listeners.Add(new ConsoleTraceListener());

#if DEBUG
            Trace.Listeners.Add(Console);
#else
            _useLogToFile = true;
            Trace.Listeners.Add(_textWriterTraceListener);
            Trace.AutoFlush = true;
#endif
        }

        #endregion Constructors

        #region Public Static Properties

        public static Debug Instance { get { return _lazy.Value; } }
        public static int IndentLevel { get { return Trace.IndentLevel; } set { Trace.IndentLevel = value; } }
        public static int IndentSize { get { return Trace.IndentSize; } set { Trace.IndentSize = value; } }

#if DEBUG
        public static bool ShowPerformanceDiagnostics { get; set; }
        public static Console Console { get; private set; } = new Console();

        public static bool UseLogToFile {
            get {
                return Instance._useLogToFile;
            }

            set {
                if (value == Instance._useLogToFile) {
                    return;
                }

                Instance._useLogToFile = value;
                if (value) {
                    Trace.Listeners.Add(Instance._textWriterTraceListener);
                } else {
                    Trace.Listeners.Remove(Instance._textWriterTraceListener);
                }
            }
        }
#else
        public static bool UseLogToFile { get { return true; } }
#endif

        #endregion Public Static Properties

        #region Public Static Methods

        #region Messages

        [Conditional("DEBUG")]
        public static void Write(string message) {
            Trace.Write(message);
        }

        [Conditional("DEBUG")]
        public static void Write(string message, string category) {
#if DEBUG
            Trace.Write(message, category);
#else
            Trace.Write($"{DateTime.Now.ToString()}  [{category}]  {new string(' ', IndentSize * IndentLevel)}{message}", category);
#endif
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message) {
            Trace.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message, string category) {
#if DEBUG
            Trace.WriteLine(message, category);
#else
            Trace.WriteLine($"{DateTime.Now.ToString()}  [{category}]  {new string(' ', IndentSize * IndentLevel)}{message}", category);
#endif
        }

        [Conditional("DEBUG")]
        public static void Critical(string message) {
            WriteLine(message, "Critical");
        }

        [Conditional("DEBUG")]
        public static void Warning(string message) {
            WriteLine(message, "Warning");
        }

        [Conditional("DEBUG")]
        public static void Error(string message) {
            WriteLine(message, "Error");
        }

        [Conditional("DEBUG")]
        public static void Error(string message, string detailMessage) {
            WriteLine($"{message}\n{detailMessage}", "Error");
        }

        [Conditional("DEBUG")]
        public static void Info(string message) {
            WriteLine(message, "Info");
        }

        [Conditional("DEBUG")]
        public static void Fail(string message) {
            Trace.Fail(message);
        }

        [Conditional("DEBUG")]
        public static void Fail(string message, string detailMessage) {
            Trace.Fail(message, detailMessage);
        }

        #endregion Messages

        #region String

        [Conditional("DEBUG")]
        public static void DrawString(Vector2 position, string message, Color? color = null, float scale = 1f) {
            Game.Instance.Core.DebugSurface.DrawString(Game.Instance.Core.StdFont, message, position, color ?? Color.White, 0, Vector2.Zero, scale, ImageFlip.None, Vector2.One);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, Vector2 position, string message, Color? color = null, float scale = 1f) {
            if (camera == null) {
                DrawString(position, message, color, scale);
                return;
            }

            DrawString(camera.Position * camera.Zoom * Game.Instance.Scale + position, message, color, scale);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, string message, Color? color = null) {
            DrawString(camera, Instance._screenMessagePosition, message, color);
            Instance._screenMessagePosition -= new Vector2(0, Game.Instance.Core.StdFont.LineSpacing + MessagesSpacing);
        }

        [Conditional("DEBUG")]
        public static void PostString(string text, int duration, Color? color = null, bool showCount = true) {
            // search for and repeat a message with same text
            Message richMsg = Instance._messagesList.Find(msg => msg.Text == text);
            if (richMsg != null) {
                Instance._messagesList.Remove(richMsg);
                Instance._messagesList.Insert(0, richMsg);

                richMsg.Color = color ?? Color.White;
                richMsg.ShowCount = showCount;
                richMsg.Repeat();
                return;
            }

            richMsg = new Message(text) {
                Color = color ?? Color.White,
                ShowCount = showCount
            };

            // fading effect
            richMsg.RegisterTween(new Tween(richMsg, duration)
                .From(new { Opacity = 1f })
                .To(new { Opacity = .03f }, Ease.SineOut));

            Instance._messagesList.Insert(0, richMsg);
        }

        #endregion String

        #region Primitives

        [Conditional("DEBUG")]
        public static void DrawLine(Vector2 from, Vector2 to, Color color) {
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.MainSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.MainSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.MainSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(from.X, from.Y, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(to.X, to.Y, 0f), Color.White)
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
        public static void DrawLine(Line line, Color color) {
            DrawLine(line.PointA, line.PointB, color);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle, Color color) {
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.MainSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.MainSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.MainSurface.Projection;
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
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle) {
            DrawRectangle(rectangle, Color.Red);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Vector2 center, float radius, int segments, Color color, bool dashed = false, float rotation = 0) {
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)

            float theta = (float) (2.0 * Util.Math.PI / segments);
            float t, c = (float) Math.Cos(theta), s = (float) Math.Sin(theta); // precalculate the sine and cosine

            float x = radius * Util.Math.Cos(rotation);
            float y = radius * Util.Math.Sin(rotation);

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.MainSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.MainSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.MainSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[segments + 1];

            int i;
            for (i = 0; i < segments; i++) {
                vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X + x, center.Y + y, 0f), Color.White);

                // apply the rotation matrix
                t = x;
                x = c * x - s * y;
                y = s * t + c * y;
            }

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X + x, center.Y + y, 0), Color.White); // just to close the last segment

            if (dashed) {
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, vertices, 0, segments / 2);
            } else {
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, segments);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }
        
        [Conditional("DEBUG")]
        public static void DrawArc(Vector2 center, float radius, float startAngle, float arcAngle, int segments, Color color) {
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)

            float theta = Util.Math.ToRadians(arcAngle) / (segments - 1); // theta is now calculated from the arc angle instead, the - 1 bit comes from the fact that the arc is open
            float tangentialFactor = (float) Math.Tan(theta), radialFactor = (float) Math.Cos(theta);

            float x = radius * Util.Math.Cos(startAngle), // we now start at the start angle
                  y = radius * Util.Math.Sin(startAngle);

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.MainSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.MainSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.MainSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[segments];

            int i;
            for (i = 0; i < segments; i++) {
                vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(center.X + x, center.Y + y, 0f), Color.White);

                float tx = -y, ty = x;
                x = (x + tx * tangentialFactor) * radialFactor;
                y = (y + ty * tangentialFactor) * radialFactor;
            }

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, segments - 1);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        [Conditional("DEBUG")]
        public static void DrawTriangle(Triangle triangle, Color color) {
            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.MainSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.MainSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.MainSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[4] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(triangle.A.X, triangle.A.Y, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(triangle.B.X, triangle.B.Y, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(triangle.C.X, triangle.C.Y, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(triangle.A.X, triangle.A.Y, 0f), Color.White)
                }, 0, 3);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        [Conditional("DEBUG")]
        public static void DrawTriangle(Triangle triangle) {
            DrawTriangle(triangle, Color.White);
        }

        #endregion Primitives

        #region Log

        public static void Log(string message) {
            Instance._textWriterTraceListener.WriteLine($"{DateTime.Now.ToString()}  {new string(' ', IndentSize * IndentLevel)}{message}");
        }

        public static void Log(string filename, string message) {
            using (StreamWriter logWriter = new StreamWriter($"{filename}.log", true)) {
                logWriter.WriteLine($"{DateTime.Now.ToString()}  {new string(' ', IndentSize)}{message}");
            }
        }

        #endregion Log

        #region Assert

        [Conditional("DEBUG")]
        public static void Assert(bool b) {
            Trace.Assert(b);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b, string message) {
            Trace.Assert(b, message);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool b, string message, string detailMessage) {
            Trace.Assert(b, message, detailMessage);
        }

        #endregion Assert

        #region Others

        public static void Indent() {
            Trace.Indent();
        }

        public static void Unindent() {
            Trace.Unindent();
        }

        #endregion Others

        #endregion Public Static Methods

        #region Internal Methods

        [Conditional("DEBUG")]
        internal void Initialize() {
            Console.Start();
        }

        [Conditional("DEBUG")]
        internal void Update(int delta) {
            foreach (Message message in _messagesList) {
                message.Update(delta);

                if (message.HasEnded) {
                    _toRemoveMessages.Add(message);
                }
            }

            if (_toRemoveMessages.Count > 0) {
                foreach (Message message in _toRemoveMessages) {
                    _messagesList.Remove(message);
                }

                _toRemoveMessages.Clear();
            }

            Console.Update(delta);
        }

        [Conditional("DEBUG")]
        internal void Render() {
            foreach (Message message in _messagesList) {
                message.Render();
            }
            
            _screenMessagePosition = ScreenMessageStartPosition;

            if (Console.Visible) {
                Console.Render();
            }
        }

        #endregion Internal Static Methods

        #region Class Message

        private class Message {
            public Message(string text, Vector2 position, bool positionRelativeToCamera) {
                Text = text;
                Position = position;
                PositionRelativeToCamera = positionRelativeToCamera;
            }

            public Message(string text, Vector2 position, int duration, bool positionRelativeToCamera) : this(text, position, positionRelativeToCamera) {
                Duration = duration;
            }

            public Message(string text) : this(text, Vector2.Zero, true) {
                AutoPosition = true;
            }

            public Message(string text, int duration) : this(text) {
                Duration = duration;
            }

            public string Text { get; private set; }
            public Vector2 Position { get; set; }
            public int Duration { get; private set; }
            public Color Color { get; set; } = Color.White;
            public float Opacity { get; set; } = 1f;
            public Tween Tween { get; private set; }
            public bool AutoPosition { get; private set; }
            public bool PositionRelativeToCamera { get; private set; }
            public bool HasEnded { get { return Timer >= Duration && (Tween == null || Tween.HasEnded); } }
            public bool ShowCount { get; set; } = true;
            public uint Timer { get; private set; }
            public int Count { get; private set; } = 1;

            public void Update(int delta) {
                Timer += (uint) delta;

                if (Tween == null || Tween.HasEnded) {
                    return;
                }

                Tween.Update(delta);
            }

            public void Render() {
                if (HasEnded) {
                    return;
                }

                if (AutoPosition) {
                    DrawString(Camera.Current, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
                    return;
                }

                if (PositionRelativeToCamera) {
                    DrawString(Camera.Current, Position, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
                    return;
                }

                DrawString(Position, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
            }

            public void RegisterTween(Tween tween) {
                Tween = tween;
                Tween.Play();
            }

            public void Repeat() {
                Count++;
                Timer = 0;

                if (Tween != null) {
                    Tween.Play();
                }
            }
        }

        #endregion Class RichMessage
    }
}
