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
        public static bool AutoRaiseConsole { get; set; } = true;
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
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine(message, "Critical");
        }

        [Conditional("DEBUG")]
        public static void Warning(string message) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine(message, "Warning");
        }

        [Conditional("DEBUG")]
        public static void Error(string message) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine(message, "Error");
        }

        [Conditional("DEBUG")]
        public static void Error(string message, string detailMessage) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

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
            Game.Instance.Core.DebugSurface.DrawString(Game.Instance.Core.StdFont, message, Util.Math.Floor(position), 0, scale, ImageFlip.None, color ?? Color.White, Vector2.Zero, Vector2.One);
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
        public static void PostString(string text, int duration = 1000, Color? color = null, bool showCount = true) {
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

        #region Line

        [Conditional("DEBUG")]
        public static void DrawLine(Vector2 from, Vector2 to, Color color) {
            from *= (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;
            to *= (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DebugSurface.Projection;
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
            DrawLine(from, to, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Line line, Color color) {
            DrawLine(line.PointA, line.PointB, color);
        }
        
        [Conditional("DEBUG")]
        public static void DrawLine(Camera camera, Vector2 from, Vector2 to, Color color) {
            if (camera == null) {
                DrawLine(from, to, color);
                return;
            }

            float scaleFactor = camera.Zoom * Game.Instance.Scale;
            DrawLine(camera.Position + from / scaleFactor, camera.Position + to / scaleFactor, color);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Camera camera, Vector2 from, Vector2 to) {
            DrawLine(camera, from, to, Color.White);
        }

        #endregion Line

        #region Lines

        [Conditional("DEBUG")]
        public static void DrawLines(IList<Vector2> points, Color color) {
            if (points == null || points.Count == 0) {
                return;
            }

            float correction = (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[points.Count];
            for (int i = 0; i < points.Count; i++) {
                Vector2 point = points[i];
                vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(correction * new Microsoft.Xna.Framework.Vector3(point.X, point.Y, 0f), Color.White);
            }

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DebugSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, points.Count - 1);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
        }

        [Conditional("DEBUG")]
        public static void DrawLines(IList<Vector2> points) {
            DrawLines(points, Color.White);
        }
        
        [Conditional("DEBUG")]
        public static void DrawLines(Camera camera, IList<Vector2> points, Color color) {
            if (camera == null) {
                DrawLines(points, color);
                return;
            }

            float scaleFactor = camera.Zoom * Game.Instance.Scale;
            Vector2[] correctedPoints = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++) {
                correctedPoints[i] = camera.Position + points[i] / scaleFactor;
            }

            DrawLines(correctedPoints, color);
        }

        [Conditional("DEBUG")]
        public static void DrawLines(Camera camera, IList<Vector2> points) {
            DrawLines(camera, points, Color.White);
        }

        #endregion Line

        #region Rectangle

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle, Color color, float rotation = 0) {
            rectangle = new Rectangle(rectangle.Position * (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale, rectangle.Size);

            Game.Instance.Core.BasicEffect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(-rectangle.Width / 2f, -rectangle.Height / 2f, 0f) * Microsoft.Xna.Framework.Matrix.CreateRotationZ(Util.Math.ToRadians(rotation)) * Microsoft.Xna.Framework.Matrix.CreateTranslation(rectangle.X + rectangle.Width / 2f, rectangle.Y + rectangle.Height / 2f, 0f) * Game.Instance.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.DebugSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Game.Instance.Core.BasicEffect.CurrentTechnique.Passes[0].Apply();
            Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                new Microsoft.Xna.Framework.Graphics.VertexPositionColor[5] {
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Width, 0f, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(rectangle.Width, rectangle.Height, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, rectangle.Height, 0f), Color.White),
                    new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f), Color.White)
                }, 0, 4);

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle, float rotation = 0) {
            DrawRectangle(rectangle, Color.White, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Camera camera, Rectangle rectangle, Color color, float rotation = 0) {
            if (camera == null) {
                DrawRectangle(rectangle, color, rotation);
                return;
            }

            DrawRectangle(new Rectangle(camera.Position + rectangle.Position / (camera.Zoom * Game.Instance.Scale), rectangle.Size), color, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Camera camera, Rectangle rectangle, float rotation) {
            DrawRectangle(camera, rectangle, Color.White, rotation);
        }

        #endregion Rectangle

        #region Circle

        [Conditional("DEBUG")]
        public static void DrawCircle(Vector2 center, float radius, int segments, Color color, bool dashed = false, float rotation = 0) {
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            center *= (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            float theta = (float) (2.0 * Util.Math.PI / segments);
            float t, c = (float) Math.Cos(theta), s = (float) Math.Sin(theta); // precalculate the sine and cosine

            float x = radius * Util.Math.Cos(rotation);
            float y = radius * Util.Math.Sin(rotation);

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DebugSurface.Projection;
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
        public static void DrawCircle(Vector2 center, float radius, int segments, bool dashed = false, float rotation = 0) {
            DrawCircle(center, radius, segments, Color.White, dashed, rotation);
        }
        
        [Conditional("DEBUG")]
        public static void DrawCircle(Circle circle, int segments, Color color, bool dashed = false, float rotation = 0) {
            DrawCircle(circle.Center, circle.Radius, segments, color, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Circle circle, int segments, bool dashed = false, float rotation = 0) {
            DrawCircle(circle.Center, circle.Radius, segments, Color.White, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Camera camera, Vector2 center, float radius, int segments, Color color, bool dashed = false, float rotation = 0) {
            if (camera == null) {
                DrawCircle(center, radius, segments, color, dashed, rotation);
                return;
            }

            DrawCircle(camera.Position + center / (camera.Zoom * Game.Instance.Scale), radius, segments, color, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Camera camera, Vector2 center, float radius, int segments, bool dashed = false, float rotation = 0) {
            DrawCircle(camera, center, radius, segments, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Camera camera, Circle circle, int segments, bool dashed = false, float rotation = 0) {
            DrawCircle(camera, circle.Center, circle.Radius, segments, dashed, rotation);
        }

        #endregion Circle

        #region Arc

        [Conditional("DEBUG")]
        public static void DrawArc(Vector2 center, float radius, float startAngle, float arcAngle, int segments, Color color) {
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)
            center *= (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            float theta = Util.Math.ToRadians(arcAngle) / (segments - 1); // theta is now calculated from the arc angle instead, the - 1 bit comes from the fact that the arc is open
            float tangentialFactor = (float) Math.Tan(theta), radialFactor = (float) Math.Cos(theta);

            float x = radius * Util.Math.Cos(startAngle), // we now start at the start angle
                  y = radius * Util.Math.Sin(startAngle);

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DebugSurface.Projection;
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
        public static void DrawArc(Vector2 center, float radius, float startAngle, float arcAngle, int segments) {
            DrawArc(center, radius, startAngle, arcAngle, segments, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawArc(Camera camera, Vector2 center, float radius, float startAngle, float arcAngle, int segments, Color color) {
            if (camera == null) {
                DrawArc(center, radius, startAngle, arcAngle, segments, color);
                return;
            }

            DrawArc(camera.Position + center / (camera.Zoom * Game.Instance.Scale), radius, startAngle, arcAngle, segments, color);
        }

        [Conditional("DEBUG")]
        public static void DrawArc(Camera camera, Vector2 center, float radius, float startAngle, float arcAngle, int segments) {
            DrawArc(camera, center, radius, startAngle, arcAngle, segments, Color.White);
        }

        #endregion Arc

        #region Triangle

        [Conditional("DEBUG")]
        public static void DrawTriangle(Triangle triangle, Color color) {
            triangle *= (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DebugSurface.Projection;
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

        [Conditional("DEBUG")]
        public static void DrawTriangle(Camera camera, Triangle triangle, Color color) {
            if (camera == null) {
                DrawTriangle(triangle, color);
                return;
            }

            DrawTriangle(camera.Position + triangle / (camera.Zoom * Game.Instance.Scale), Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawTriangle(Camera camera, Triangle triangle) {
            DrawTriangle(camera, triangle, Color.White);
        }

        #endregion Triangle

        #region Polygon

        [Conditional("DEBUG")]
        public static void DrawPolygon(Polygon polygon, Color color) {
            float correction = (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            Game.Instance.Core.BasicEffect.World = Game.Instance.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.DebugSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[polygon.VertexCount + 1];
            int i = 0;
            foreach (Vector2 vertexPos in polygon) {
                vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(vertexPos.X * correction, vertexPos.Y * correction, 0f), Color.White);
                i++;
            }

            vertices[i] = vertices[0];

            foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, vertices.Length - 1);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        [Conditional("DEBUG")]
        public static void DrawPolygon(Polygon polygon) {
            DrawPolygon(polygon, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawPolygon(Camera camera, Polygon polygon, Color color) {
            if (camera == null) {
                DrawPolygon(polygon, color);
                return;
            }

            polygon.Translate(camera.Position);
            DrawPolygon(polygon, color);
        }

        [Conditional("DEBUG")]
        public static void DrawPolygon(Camera camera, Polygon polygon) {
            DrawPolygon(camera, polygon, Color.White);
        }

        #endregion Polygon

        #region Grid

        [Conditional("DEBUG")]
        public static void DrawGrid(Size tileSize, int columns, int rows, Vector2 position, Color color) {
            Assert(columns > 0 && rows > 0, "Columns and Rows must be greater than zero.");

            float correction = (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            Game.Instance.Core.BasicEffect.World = Game.Instance.DebugSurface.World * Microsoft.Xna.Framework.Matrix.CreateTranslation(position.X * correction, position.Y * correction, 0f);
            Game.Instance.Core.BasicEffect.View = Game.Instance.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.DebugSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2 * (columns + rows + 2)];

            int id = 0;
            for (int column = 1; column < columns; column++) {
                vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * tileSize.Width * correction, 0, 0), new Color(0x494949ff));
                vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * tileSize.Width * correction, rows * tileSize.Height * correction, 0), new Color(0x494949ff));
                id += 2;
            }

            for (int row = 1; row < rows; row++) {
                vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, row * tileSize.Height * correction, 0), new Color(0x494949ff));
                vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width * correction, row * tileSize.Height * correction, 0), new Color(0x494949ff));
                id += 2;
            }

            // left border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, rows * tileSize.Height * correction, 0), Color.White);
            id += 2;

            // right border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width * correction, 0, 0), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width * correction, rows * tileSize.Height * correction, 0), Color.White);
            id += 2;

            // top border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, 0, 0), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width * correction, 0, 0), Color.White);
            id += 2;

            // bottom border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0, rows * tileSize.Height * correction, 0), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width * correction, rows * tileSize.Height * correction, 0), Color.White);
            id += 2;

            foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, vertices, 0, columns + rows + 2);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
        }

        [Conditional("DEBUG")]
        public static void DrawGrid(Size tileSize, int columns, int rows, Vector2 position) {
            DrawGrid(tileSize, columns, rows, position, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawGrid(Camera camera, Size tileSize, int columns, int rows, Vector2 position, Color color) {
            if (camera == null) {
                DrawGrid(tileSize, columns, rows, position, color);
                return;
            }

            DrawGrid(tileSize, columns, rows, camera.Position + position, color);
        }

        [Conditional("DEBUG")]
        public static void DrawGrid(Camera camera, Size tileSize, int columns, int rows, Vector2 position) {
            DrawGrid(camera, tileSize, columns, rows, position, Color.White);
        }

        #endregion Grid

        #region Bezier Curve

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Vector2[] points, Color color, float step = .1f) {
            float correction = (Camera.Current != null ? Camera.Current.Zoom : 1f) * Game.Instance.Scale;

            Game.Instance.Core.BasicEffect.World = Game.Instance.Core.DebugSurface.World;
            Game.Instance.Core.BasicEffect.View = Game.Instance.Core.DebugSurface.View;
            Game.Instance.Core.BasicEffect.Projection = Game.Instance.Core.DebugSurface.Projection;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            Game.Instance.Core.BasicEffect.Alpha = color.A / 255f;

            // build bezier curve points
            int steps = 1 + (int) Math.Ceiling(1f / step);
            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[steps];
            if (points.Length == 3) {
                float t = 0f;
                for (int i = 0; i < steps; i++) {
                    Vector2 point = correction * Util.Math.BezierCurve(points[0], points[1], points[2], t);
                    vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(point.X, point.Y, 0f), Color.White);
                    t = Util.Math.Approach(t, 1f, step);
                }
            } else if (points.Length == 4) {
                float t = 0f;
                for (int i = 0; i < steps; i++) {
                    Vector2 point = correction * Util.Math.BezierCurve(points[0], points[1], points[2], points[3], t);
                    vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(point.X, point.Y, 0f), Color.White);
                    t = Util.Math.Approach(t, 1f, step);
                }
            }

            foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in Game.Instance.Core.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                Game.Instance.Core.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, steps - 1);
            }

            Game.Instance.Core.BasicEffect.Alpha = 1f;
            Game.Instance.Core.BasicEffect.DiffuseColor = new Microsoft.Xna.Framework.Vector3(1f, 1f, 1f);
        }

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Vector2[] points, float step = .1f) {
            DrawBezierCurve(points, Color.White, step);
        }
        
        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Camera camera, Vector2[] points, Color color, float step = .1f) {
            if (camera == null) {
                DrawBezierCurve(points, color, step);
                return;
            }

            Vector2[] correctedPoints = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++) {
                correctedPoints[i] = camera.Position + points[i] * camera.Zoom * Game.Instance.Scale;
            }

            DrawBezierCurve(correctedPoints, color, step);
        }

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Camera camera, Vector2[] points, float step = .1f) {
            DrawBezierCurve(camera, points, Color.White, step);
        }

        #endregion Bezier Curve

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

        public static Vector2 Transform(Vector2 position) {
            return Game.Instance.DebugSurface.Transform(position, Game.Instance.MainSurface);
        }

        public static Vector2 Transform(Vector2 position, Surface surface) {
            return Game.Instance.DebugSurface.Transform(position, surface);
        }

        public static float Transform(float n) {
            return Game.Instance.DebugSurface.Transform(new Vector2(n), Game.Instance.MainSurface).X;
        }

        public static float Transform(float n, Surface surface) {
            return Game.Instance.DebugSurface.Transform(new Vector2(n), surface).X;
        }

        public static Size Transform(Size size) {
            return new Size(Transform(size.ToVector2()));
        }

        public static Size Transform(Size size, Surface surface) {
            return new Size(Transform(size.ToVector2(), surface));
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
