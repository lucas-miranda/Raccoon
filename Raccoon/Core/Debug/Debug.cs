#if DEBUG
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.FileSystem;
using Raccoon.Util.Tween;
using Raccoon.Util.Collections;

namespace Raccoon {
    public sealed class Debug {
        #region Public Members

        public static readonly string LogFileName = "report.log";

        public enum LensKind {
            Camera = 0,
            PhysicsBodies,
            PerformanceDiagnostics,
            Pathfinding
        }

        #endregion Public Members

        #region Private Members

        private const int MessagesSpacing = 5;
        private static readonly Vector2 ScreenMessageStartPosition = new Vector2(15, -30);

        private static PathBuf _reportDirectoryPath;

        private List<Message> _messagesList = new List<Message>(),
                              _toRemoveMessages = new List<Message>();

        private Locker<Alarm> _alarms = new Locker<Alarm>();

        // compose message
        //private static StringBuilder _composeMessage = new StringBuilder();

        private PathBuf _reportFilepath;

        #endregion Private Members

        #region Constructors

        private Debug() {
            Logger.RegisterListener(Console);

            if (ReportDirectoryPath != null) {
                if (!ReportDirectoryPath.ExistsDirectory()) {
                    System.IO.Directory.CreateDirectory(ReportDirectoryPath.ToString());
                    Logger.Info("Creating report directory");
                } else {
                    Logger.Info("Report directory already exists");
                }

                _reportFilepath = ReportDirectoryPath + LogFileName;
            } else {
                _reportFilepath = new PathBuf(Directories.Base, LogFileName);
            }

            Logger.RegisterListener(new TextWriterLoggerListener(_reportFilepath.ToString()));

            foreach (System.Enum kind in System.Enum.GetValues(typeof(LensKind))) {
                DebugDrawLens lens = Draw.RegisterLens(kind);
                lens.Enable();
            }
        }

        #endregion Constructors

        #region Public Static Properties

        public static Debug Instance { get; private set; }
        public static bool ShowPerformanceDiagnostics { get; set; }
        public static bool AutoRaiseConsole { get; set; } = true;
        public static PathBuf ReportFilepath { get { return Instance?._reportFilepath; } }
        public static Console Console { get; private set; } = new Console();
        public static DebugDraw Draw { get; } = new DebugDraw();

        public static PathBuf ReportDirectoryPath {
            get {
                return _reportDirectoryPath;
            }

            set {
                if (Instance != null) {
                    throw new System.InvalidOperationException($"Report directory path should be changed before {nameof(Debug)} is initialized.");
                }

                _reportDirectoryPath = value;
            }
        }

        #endregion Public Static Properties

        #region Public Methods

        #region Messages

        [Conditional("DEBUG")]
        public static void Write(string context, string message) {
            Logger.Write(message);
        }

        [Conditional("DEBUG")]
        public static void Write(string context, object obj) {
            //Logger.Write(context, obj.ToString());
            Logger.Write(obj.ToString());
        }

        [Conditional("DEBUG")]
        public static void Write(string message) {
            Logger.Write(message);
        }

        [Conditional("DEBUG")]
        public static void Write(object obj) {
            Logger.Write(obj.ToString());
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string context, string message) {
            Logger.WriteLine(context, message);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string context, object obj) {
            Logger.WriteLine(context, obj.ToString());
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message) {
            Logger.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void WriteLine(object obj, int level = 0) {
            Logger.WriteLine(obj.ToString());
        }

        /*
        [Conditional("DEBUG")]
        public static void ComposeMessage(string message, int level = 0) {
            if (IndentLevel != level) {
                IndentLevel = level;
            }

            _composeMessage.Append($"{IndentText}{message}");
        }

        [Conditional("DEBUG")]
        public static void ComposeMessage(object obj, int level = 0) {
            ComposeMessage(obj.ToString());
        }

        [Conditional("DEBUG")]
        public static void ComposeMessageLine(string message, int level = 0) {
            if (IndentLevel != level) {
                IndentLevel = level;
            }

            _composeMessage.Append($"{IndentText}{message}\n");
        }

        [Conditional("DEBUG")]
        public static void ComposeMessageLine(object obj) {
            ComposeMessageLine(obj.ToString());
        }

        [Conditional("DEBUG")]
        public static void WriteComposedMessage(string context = null) {
            WriteLine(_composeMessage.ToString(), context);
            _composeMessage.Clear();
        }

        [Conditional("DEBUG")]
        public static void ClearComposedMessage() {
            _composeMessage.Clear();
        }

        [Conditional("DEBUG")]
        public static string RetrieveComposedMessage() {
            return _composeMessage.ToString();
        }

        [Conditional("DEBUG")]
        public static string RetrieveComposedMessage(int startIndex, int length) {
            return _composeMessage.ToString(startIndex, length);
        }
        */

        [Conditional("DEBUG")]
        public static void Dump(params object[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach (object var in vars) {
                if (var == null) {
                    str.AppendLine("null, ");
                } else {
                    str.AppendFormat("{0}, ", var.ToString());
                }
            }

            Write(str.ToString());
        }

        [Conditional("DEBUG")]
        public static void Dump(params (string, object)[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach ((string Name, object Value) data in vars) {
                str.AppendFormat(
                    "{0}: {1}, ",
                    data.Name,
                    data.Value == null ? "null" : data.Value.ToString()
                );
            }

            Write(str.ToString());
        }

        [Conditional("DEBUG")]
        public static void DumpLine(params object[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach (object var in vars) {
                if (var == null) {
                    str.Append("null");
                } else {
                    str.AppendFormat("{0}", var.ToString());
                }

                str.AppendLine();
            }

            Write(str.ToString());
        }

        [Conditional("DEBUG")]
        public static void DumpLine(params (string, object)[] vars) {
            StringBuilder str = new StringBuilder(vars.Length);

            foreach ((string Name, object Value) data in vars) {
                str.AppendFormat(
                    "{0}: {1}",
                    data.Name,
                    data.Value == null ? "null" : data.Value.ToString()
                );
                str.AppendLine();
            }

            Write(str.ToString());
        }

        #endregion Messages

        #region String

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
                .To(new { Opacity = 0f }, Ease.QuartIn));

            Instance._messagesList.Insert(0, richMsg);
        }

        #endregion String

        #region Log

        /*
        [Conditional("DEBUG")]
        public static void Log(string message) {
            Instance._textWriterTraceListener.WriteLine($"{System.DateTime.Now.ToString()}  {message}");
        }

        [Conditional("DEBUG")]
        public static void Log(string filename, string message) {
            using (StreamWriter logWriter = new StreamWriter($"{filename}.log", true)) {
                logWriter.WriteLine($"{System.DateTime.Now.ToString()}  {message}");
            }
        }
        */

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

        [Conditional("DEBUG")]
        public static void Stopwatch(uint interval, System.Action action = null) {
            if (action == null) {
                System.TimeSpan startTime = Game.Instance.Time;
                string stdMessage = $"[Debug] Stopwatch ended.\n  Start: {startTime.ToString(@"hh\:mm\:ss\.fff")}, End: {{0}}\n  Duration: {{1}}";
                action = () => {
                    System.TimeSpan endTime = Game.Instance.Time;
                    Logger.Info(string.Format(stdMessage, endTime.ToString(@"hh\:mm\:ss\.fff"), endTime.Subtract(startTime).ToString(@"hh\:mm\:ss\.fff")));
                };
            }

            Instance._alarms.Add(new Alarm(interval, action) {
                RepeatTimes = 0
            });
        }

        #endregion Time

        #region Others

        [Conditional("DEBUG")]
        public static void Indent() {
            Trace.Indent();
        }

        [Conditional("DEBUG")]
        public static void Unindent() {
            Trace.Unindent();
        }

        #endregion Others

        #endregion Public Methods

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

            _alarms.Lock();
            foreach (Alarm alarm in _alarms) {
                alarm.Update(delta);
                if (alarm.TriggeredCount > alarm.RepeatTimes) {
                    _alarms.Remove(alarm);
                }
            }
            _alarms.Unlock();

            Console.Update(delta);
        }

        [Conditional("DEBUG")]
        internal void Render() {
            foreach (Message message in _messagesList) {
                message.Render();
            }

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

                if (PositionRelativeToCamera) {
                    if (AutoPosition) {
                        Draw.String.AtScreen(Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Vector2.Zero, Color * Opacity);
                        return;
                    }

                    Draw.String.AtScreen(Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Position, Color * Opacity);
                    return;
                }

                if (AutoPosition) {
                    Draw.String.AtWindow(Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Vector2.Zero, Color * Opacity);
                } else {
                    Draw.String.AtWindow(Text + (ShowCount && Count > 1 ? $" [{Count}]" : ""), Position, Color * Opacity);
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

        #endregion Class Message
    }
}
#endif
