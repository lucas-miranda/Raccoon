using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.Util.Tween;
using Raccoon.Util.Collections;
using Raccoon.Util;

namespace Raccoon {
    public sealed class Debug {
        #region Public Members

        public static readonly string LogFileName = "report.log";

        #endregion Public Members

        #region Private Members

        private const int MessagesSpacing = 5;
        private static readonly Vector2 ScreenMessageStartPosition = new Vector2(15, -30);

        private static int _indentSize = 4,
                           _indentLevel = 0;
        private static string IndentText = "";

        private bool _useLogToFile;
        private TextWriterTraceListener _textWriterTraceListener = new TextWriterTraceListener(LogFileName, "logger");
        private Vector2 _screenMessagePosition;
        private List<Message> _messagesList = new List<Message>(), _toRemoveMessages = new List<Message>();
        private Locker<Alarm> _alarms = new Locker<Alarm>();

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

        public static Debug Instance { get; private set; }

        public static int IndentSize { 
            get { 
                return _indentSize; 
            } 

            set { 
                //Trace.IndentSize = value; 
                _indentSize = value;
                PrepareIndentText();
            } 
        }

        public static int IndentLevel { 
            get { 
                return _indentLevel; 
            } 

            set { 
                //Trace.IndentLevel = value; 
                _indentLevel = value;
                PrepareIndentText();
            } 
        }

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

        #region Public Methods

        #region Messages

        [Conditional("DEBUG")]
        public static void Write(string message, string context, int level = 0) {
            if (IndentLevel != level) {
                IndentLevel = level;
            }

            Trace.Write($"{IndentText}{message}", context);
        }

        [Conditional("DEBUG")]
        public static void Write(object obj, string context, int level = 0) {
            Write(obj.ToString(), context, level);
        }

        [Conditional("DEBUG")]
        public static void Write(string message, int level = 0) {
            Write(message, null, level);
        }

        [Conditional("DEBUG")]
        public static void Write(object obj, int level = 0) {
            Write(obj.ToString(), null, level);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message, string context, int level = 0) {
            if (IndentLevel != level) {
                IndentLevel = level;
            }

            Trace.WriteLine($"{IndentText}{message}", context);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object obj, string context, int level = 0) {
            WriteLine(obj.ToString(), context, level);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message, int level = 0) {
            WriteLine(message, null, level);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object obj, int level = 0) {
            WriteLine(obj.ToString(), null, level);
        }

        [Conditional("DEBUG")]
        public static void Critical(string message, int level = 0) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine(message, "Critical", level);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message, int level = 0) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine(message, "Warning", level);
        }

        [Conditional("DEBUG")]
        public static void Error(string message, int level = 0) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine(message, "Error", level);
        }

        [Conditional("DEBUG")]
        public static void Error(string message, string detailMessage, int level = 0) {
            if (AutoRaiseConsole && !Console.Visible) {
                Console.Show();
            }

            WriteLine($"{message}\n{detailMessage}", "Error", level);
        }

        [Conditional("DEBUG")]
        public static void Info(string message, int level = 0) {
            WriteLine(message, "Info", level);
        }

        [Conditional("DEBUG")]
        public static void Fail(string message) {
            Trace.Fail(message);
        }

        [Conditional("DEBUG")]
        public static void Fail(string message, string detailMessage) {
            Trace.Fail(message, detailMessage);
        }

        public static void Dump(params object[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach (object var in vars) {
                str.AppendFormat("{0}, ", var.ToString());
            }

            Write(str.ToString());
        }

        public static void Dump(params (string, object)[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach ((string name, object value) in vars) {
                str.AppendFormat("{0}: {1}, ", name, value.ToString());
            }

            Write(str.ToString());
        }

        public static void DumpLine(params object[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach (object var in vars) {
                str.AppendFormat("{0}", var.ToString());
                str.AppendLine();
            }

            Write(str.ToString());
        }

        public static void DumpLine(params (string, object)[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach ((string name, object value) in vars) {
                str.AppendFormat("{0}: {1}", name, value.ToString());
                str.AppendLine();
            }

            Write(str.ToString());
        }

        #endregion Messages

        #region String

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, Vector2 position, string message, Color? color = null, float scale = 1f) {
            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            Game.Instance.DebugRenderer.DrawString(
                Game.Instance.StdFont,
                message,
                zoom * (-cameraPos + position),
                0,
                scale,
                ImageFlip.None,
                color ?? Color.White,
                Vector2.Zero,
                Vector2.One
            );
        }

        [Conditional("DEBUG")]
        public static void DrawString(Camera camera, string message, Color? color = null) {
            DrawString(camera, Game.Instance.MainRenderer.ConvertScreenToWorld(Instance._screenMessagePosition), message, color);
            Instance._screenMessagePosition -= new Vector2(0, Game.Instance.StdFont.LineSpacing + MessagesSpacing);
        }

        [Conditional("DEBUG")]
        public static void DrawString(Vector2 position, string message, Color? color = null, float scale = 1f) {
            DrawString(Camera.Current, position, message, color, scale);
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
        public static void DrawLine(Camera camera, Vector2 from, Vector2 to, Color color) {
            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            Vector2[] points = new Vector2[] {
                (-cameraPos + from) * zoom,
                (-cameraPos + to) * zoom
            };

            Game.Instance.DebugPrimitiveBatch.DrawLines(points, color, 0, new Vector2(1f), Vector2.Zero, cyclic: false);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Camera camera, Vector2 from, Vector2 to) {
            DrawLine(camera, from, to, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Vector2 from, Vector2 to, Color color) {
            DrawLine(Camera.Current, from, to, color); ;
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Vector2 from, Vector2 to) {
            DrawLine(Camera.Current, from, to, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawLine(Line line, Color color) {
            DrawLine(Camera.Current, line.PointA, line.PointB, color);
        }

        #endregion Line

        #region Lines

        [Conditional("DEBUG")]
        public static void DrawLines(Camera camera, IList<Vector2> points, Color color, Vector2? origin = null) {
            if (points == null || points.Count == 0) {
                return;
            }

            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            if (!origin.HasValue) {
                origin = Vector2.Zero;
            }

            List<Vector2> pointList = new List<Vector2>();
            for (int i = 0; i < points.Count; i++) {
                pointList.Add((-cameraPos + points[i]) * zoom);
            }

            Game.Instance.DebugPrimitiveBatch.DrawLines(pointList, color, 0, new Vector2(1f), origin.Value);
        }

        [Conditional("DEBUG")]
        public static void DrawLines(Camera camera, IList<Vector2> points) {
            DrawLines(camera, points, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawLines(IList<Vector2> points, Color color, Vector2? origin = null) {
            DrawLines(Camera.Current, points, color, origin);
        }

        [Conditional("DEBUG")]
        public static void DrawLines(IList<Vector2> points) {
            DrawLines(Camera.Current, points, Color.White);
        }

        #endregion Line

        #region Rectangle

        [Conditional("DEBUG")]
        public static void DrawRectangle(Camera camera, Rectangle rectangle, Color color, float rotation = 0, Vector2? scale = null, Vector2? origin = null) {
            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            if (!origin.HasValue) {
                //origin = rectangle.Size.ToVector2() / 2f;
                //rectangle.Position += origin.Value;
                origin = Vector2.Zero;
            }

            if (!scale.HasValue) {
                scale = Vector2.One;
            }

            Game.Instance.DebugPrimitiveBatch.DrawHollowRectangle((-cameraPos + rectangle.Position) * zoom, rectangle.Size, color, rotation, scale.Value * zoom, origin.Value);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Camera camera, Rectangle rectangle, float rotation, Vector2? scale = null, Vector2? origin = null) {
            DrawRectangle(camera, rectangle, Color.White, rotation, scale, origin);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle, Color color, float rotation = 0, Vector2? scale = null, Vector2? origin = null) {
            DrawRectangle(Camera.Current, rectangle, color, rotation, scale, origin);
        }

        [Conditional("DEBUG")]
        public static void DrawRectangle(Rectangle rectangle, float rotation = 0, Vector2? scale = null, Vector2? origin = null) {
            DrawRectangle(Camera.Current, rectangle, Color.White, rotation, scale, origin);
        }

        #endregion Rectangle

        #region Circle

        [Conditional("DEBUG")]
        public static void DrawCircle(Camera camera, Vector2 center, float radius, Color color, int segments = 0, bool dashed = false, float rotation = 0) {
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)

            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            if (segments <= 0) {
                segments = (int) (radius <= 3 ? (radius * radius * radius) : (radius + radius));
            }

            Game.Instance.DebugPrimitiveBatch.DrawHollowCircle((-cameraPos + center) * zoom, radius, color, zoom, Vector2.Zero);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Camera camera, Vector2 center, float radius, int segments = 0, bool dashed = false, float rotation = 0) {
            DrawCircle(camera, center, radius, segments, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Camera camera, Circle circle, int segments = 0, bool dashed = false, float rotation = 0) {
            DrawCircle(camera, circle.Center, circle.Radius, segments, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Vector2 center, float radius, Color color, int segments = 0, bool dashed = false, float rotation = 0) {
            DrawCircle(Camera.Current, center, radius, color, segments, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Vector2 center, float radius, int segments = 0, bool dashed = false, float rotation = 0) {
            DrawCircle(Camera.Current, center, radius, Color.White, segments, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Circle circle, Color color, int segments = 0, bool dashed = false, float rotation = 0) {
            DrawCircle(Camera.Current, circle.Center, circle.Radius, color, segments, dashed, rotation);
        }

        [Conditional("DEBUG")]
        public static void DrawCircle(Circle circle, int segments = 0, bool dashed = false, float rotation = 0) {
            DrawCircle(Camera.Current, circle.Center, circle.Radius, Color.White, segments, dashed, rotation);
        }

        #endregion Circle

        #region Arc

        [Conditional("DEBUG")]
        public static void DrawArc(Camera camera, Vector2 center, float radius, float startAngle, float arcAngle, Color color, int segments = 0) {
            // implemented using http://slabode.exofire.net/circle_draw.shtml (Just slightly reorganized and I decided to keep the comments)

            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            if (segments <= 1) {
                segments = (int) (radius <= 3 ? (radius * radius * radius) : (radius + radius));
            }

            float theta = Math.ToRadians(arcAngle) / (segments - 1); // theta is now calculated from the arc angle instead, the - 1 bit comes from the fact that the arc is open
            float tangentialFactor = (float) System.Math.Tan(theta), radialFactor = (float) System.Math.Cos(theta);

            float x = radius * Math.Cos(startAngle), // we now start at the start angle
                  y = radius * Math.Sin(startAngle);

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(-cameraPos.X + center.X, -cameraPos.Y + center.Y, 0f)
                * Microsoft.Xna.Framework.Matrix.CreateScale(zoom, zoom, 1f)
                * Game.Instance.DebugRenderer.World;

            bs.View = Game.Instance.DebugRenderer.View;
            bs.Projection = Game.Instance.DebugRenderer.Projection;

            // material
            bs.SetMaterial(color);

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[segments];

            int i;
            for (i = 0; i < segments; i++) {
                vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(x, y, 0f), Color.White);

                float tx = -y, ty = x;
                x = (x + tx * tangentialFactor) * radialFactor;
                y = (y + ty * tangentialFactor) * radialFactor;
            }

            foreach (var pass in bs) {
                Game.Instance.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, segments - 1);
            }

            bs.ResetParameters();
        }

        [Conditional("DEBUG")]
        public static void DrawArc(Camera camera, Vector2 center, float radius, float startAngle, float arcAngle, int segments = 0) {
            DrawArc(camera, center, radius, startAngle, arcAngle, Color.White, segments);
        }

        [Conditional("DEBUG")]
        public static void DrawArc(Vector2 center, float radius, float startAngle, float arcAngle, Color color, int segments = 0) {
            DrawArc(Camera.Current, center, radius, startAngle, arcAngle, color, segments);
        }

        [Conditional("DEBUG")]
        public static void DrawArc(Vector2 center, float radius, float startAngle, float arcAngle, int segments = 0) {
            DrawArc(Camera.Current, center, radius, startAngle, arcAngle, Color.White, segments);
        }

        #endregion Arc

        #region Triangle

        [Conditional("DEBUG")]
        public static void DrawTriangle(Camera camera, Triangle triangle, Color color) {
            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            Vector2[] points = new Vector2[] {
                (-cameraPos + triangle.A) * zoom,
                (-cameraPos + triangle.B) * zoom,
                (-cameraPos + triangle.C) * zoom
            };

            Game.Instance.DebugPrimitiveBatch.DrawLines(points, color, 0, new Vector2(zoom), Vector2.Zero);
        }

        [Conditional("DEBUG")]
        public static void DrawTriangle(Camera camera, Triangle triangle) {
            DrawTriangle(camera, triangle, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawTriangle(Triangle triangle, Color color) {
            DrawTriangle(Camera.Current, triangle, color);
        }

        [Conditional("DEBUG")]
        public static void DrawTriangle(Triangle triangle) {
            DrawTriangle(Camera.Current, triangle, Color.White);
        }

        #endregion Triangle

        #region Polygon

        [Conditional("DEBUG")]
        public static void DrawPolygon(Camera camera, Polygon polygon, Color color) {
            DrawLines(camera, polygon.Vertices, color);
        }

        [Conditional("DEBUG")]
        public static void DrawPolygon(Camera camera, Polygon polygon) {
            DrawPolygon(camera, polygon, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawPolygon(Polygon polygon, Color color) {
            DrawPolygon(Camera.Current, polygon, color);
        }

        [Conditional("DEBUG")]
        public static void DrawPolygon(Polygon polygon) {
            DrawPolygon(Camera.Current, polygon, Color.White);
        }

        #endregion Polygon

        #region Grid

        [Conditional("DEBUG")]
        public static void DrawGrid(Camera camera, Size tileSize, int columns, int rows, Vector2 position, Color color) {
            Assert(columns > 0 && rows > 0, "Columns and Rows must be greater than zero.");

            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(-cameraPos.X + position.X, -cameraPos.Y + position.Y, 0f)
                * Microsoft.Xna.Framework.Matrix.CreateScale(zoom, zoom, 1f)
                * Game.Instance.DebugRenderer.World;

            bs.View = Game.Instance.DebugRenderer.View;
            bs.Projection = Game.Instance.DebugRenderer.Projection;

            // material
            bs.SetMaterial(color);

            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[2 * (columns + rows + 2)];

            int id = 0;
            for (int column = 1; column < columns; column++) {
                vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * tileSize.Width, 0f, 0f), new Color(0x494949FF));
                vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(column * tileSize.Width, rows * tileSize.Height, 0f), new Color(0x494949FF));
                id += 2;
            }

            for (int row = 1; row < rows; row++) {
                vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, row * tileSize.Height, 0f), new Color(0x494949FF));
                vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width, row * tileSize.Height, 0f), new Color(0x494949FF));
                id += 2;
            }

            // left border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(Microsoft.Xna.Framework.Vector3.Zero, Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, rows * tileSize.Height, 0f), Color.White);
            id += 2;

            // right border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width, 0f, 0f), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width, rows * tileSize.Height, 0f), Color.White);
            id += 2;

            // top border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width, 0f, 0f), Color.White);
            id += 2;

            // bottom border
            vertices[id] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(0f, rows * tileSize.Height, 0f), Color.White);
            vertices[id + 1] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(columns * tileSize.Width, rows * tileSize.Height, 0f), Color.White);
            id += 2;

            foreach (var pass in bs) {
                Game.Instance.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, vertices, 0, columns + rows + 2);
            }

            bs.ResetParameters();
        }

        [Conditional("DEBUG")]
        public static void DrawGrid(Camera camera, Size tileSize, int columns, int rows, Vector2 position) {
            DrawGrid(camera, tileSize, columns, rows, position, Color.White);
        }

        [Conditional("DEBUG")]
        public static void DrawGrid(Size tileSize, int columns, int rows, Vector2 position, Color color) {
            DrawGrid(Camera.Current, tileSize, columns, rows, position, color);
        }

        [Conditional("DEBUG")]
        public static void DrawGrid(Size tileSize, int columns, int rows, Vector2 position) {
            DrawGrid(Camera.Current, tileSize, columns, rows, position, Color.White);
        }

        #endregion Grid

        #region Bezier Curve

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Camera camera, Vector2[] points, Color color, float step = .1f) {
            Vector2 cameraPos = Vector2.Zero;
            float zoom = 1f;

            if (camera != null) {
                cameraPos = camera.Position;
                zoom = Game.Instance.PixelScale * camera.Zoom * Game.Instance.KeepProportionsScale;
            }

            BasicShader bs = Game.Instance.BasicShader;

            // transformations
            bs.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(-cameraPos.X, -cameraPos.Y, 0f)
                * Microsoft.Xna.Framework.Matrix.CreateScale(zoom, zoom, 1f)
                * Game.Instance.DebugRenderer.World;

            bs.View = Game.Instance.DebugRenderer.View;
            bs.Projection = Game.Instance.DebugRenderer.Projection;

            // material
            bs.SetMaterial(color);

            // build bezier curve points
            int steps = 1 + (int) Math.Ceiling(1f / step);
            Microsoft.Xna.Framework.Graphics.VertexPositionColor[] vertices = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[steps];
            if (points.Length == 3) {
                float t = 0f;
                for (int i = 0; i < steps; i++) {
                    Vector2 point = Math.BezierCurve(points[0], points[1], points[2], t);
                    vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(point.X, point.Y, 0f), Color.White);
                    t = Math.Approach(t, 1f, step);
                }
            } else if (points.Length == 4) {
                float t = 0f;
                for (int i = 0; i < steps; i++) {
                    Vector2 point = Math.BezierCurve(points[0], points[1], points[2], points[3], t);
                    vertices[i] = new Microsoft.Xna.Framework.Graphics.VertexPositionColor(new Microsoft.Xna.Framework.Vector3(point.X, point.Y, 0f), Color.White);
                    t = Math.Approach(t, 1f, step);
                }
            }

            foreach (var pass in bs) {
                Game.Instance.GraphicsDevice.DrawUserPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, vertices, 0, steps - 1);
            }

            bs.ResetParameters();
        }

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Camera camera, Vector2[] points, float step = .1f) {
            DrawBezierCurve(camera, points, Color.White, step);
        }

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Vector2[] points, Color color, float step = .1f) {
            DrawBezierCurve(Camera.Current, points, color, step);
        }

        [Conditional("DEBUG")]
        public static void DrawBezierCurve(Vector2[] points, float step = .1f) {
            DrawBezierCurve(Camera.Current, points, Color.White, step);
        }

        #endregion Bezier Curve

        #endregion Primitives

        #region Log

        public static void Log(string message) {
            Instance._textWriterTraceListener.WriteLine($"{System.DateTime.Now.ToString()}  {new string(' ', IndentSize * IndentLevel)}{message}");
        }

        public static void Log(string filename, string message) {
            using (StreamWriter logWriter = new StreamWriter($"{filename}.log", true)) {
                logWriter.WriteLine($"{System.DateTime.Now.ToString()}  {new string(' ', IndentSize)}{message}");
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

        #region Time

        public static void Stopwatch(uint interval, System.Action action = null) {
            if (action == null) {
                System.TimeSpan startTime = Game.Instance.Time;
                string stdMessage = $"[Debug] Stopwatch ended.\n  Start: {startTime.ToString(@"hh\:mm\:ss\.fff")}, End: {{0}}\n  Duration: {{1}}";
                action = () => {
                    System.TimeSpan endTime = Game.Instance.Time;
                    Info(string.Format(stdMessage, endTime.ToString(@"hh\:mm\:ss\.fff"), endTime.Subtract(startTime).ToString(@"hh\:mm\:ss\.fff")));
                };
            }

            Instance._alarms.Add(new Alarm(interval, action) {
                RepeatTimes = 0
            });
        }

        #endregion Time

        #region Others

        public static void Indent() {
            Trace.Indent();
        }

        public static void Unindent() {
            Trace.Unindent();
        }

        #endregion Others

        #endregion Public Methods

        #region Private Methods

        private static void PrepareIndentText() {
            IndentText = new string(' ', IndentSize * IndentLevel);
        }

        #endregion Private Methods

        #region Internal Methods

        [Conditional("DEBUG")]
        internal static void Start() {
            if (Instance != null) {
                return;
            }

            Instance = new Debug();
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

            foreach (Alarm alarm in _alarms) {
                alarm.Update(delta);
                if (alarm.TriggeredCount > alarm.RepeatTimes) {
                    _alarms.Remove(alarm);
                }
            }

            Console.Update(delta);
        }

        [Conditional("DEBUG")]
        internal void Render() {
            foreach (Message message in _messagesList) {
                message.Render();
            }

            _screenMessagePosition = new Vector2(ScreenMessageStartPosition.X, Game.Instance.WindowHeight + ScreenMessageStartPosition.Y);

            if (Console.Visible) {
                Console.Render();
            }

            IndentLevel = 0;
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

                if (PositionRelativeToCamera) {
                    if (AutoPosition) {
                        DrawString(Camera.Current, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
                        return;
                    }

                    DrawString(Position, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
                    return;
                }

                if (AutoPosition) {
                    DrawString(null, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
                } else {
                    DrawString(null, Position, Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Color * Opacity);
                }
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
