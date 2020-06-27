using System.Collections.Generic;
using System.Text;

using Raccoon.Log;

namespace Raccoon {
    public sealed class Logger {
        #region Private Members

        private List<ILoggerListener> _listeners = new List<ILoggerListener>();
        private List<string> _subjects = new List<string>();
        private int _previousHeaderLength;
        private string _indent = string.Empty,
                       _lastContext = string.Empty;

        #endregion Private Members

        #region Constructors

        private Logger() {
            switch (System.Environment.OSVersion.Platform) {
                case System.PlatformID.Win32NT:
                case System.PlatformID.Win32S:
                case System.PlatformID.Win32Windows:
                case System.PlatformID.WinCE:
                case System.PlatformID.Xbox:
                    NewLine = "\r\n";
                    break;

                case System.PlatformID.MacOSX:
                case System.PlatformID.Unix:
                default:
                    NewLine = "\n";
                    break;
            }
        }

        #endregion Constructors

        #region Public Properties

        public static Logger Instance { get; private set; }
        public static bool IsInitialized { get { return Instance != null; } }
        public static string NewLine { get; set; }
        public static string LastSubject { get { return Instance._subjects.Count == 0 ? null : Instance._subjects[Instance._subjects.Count - 1]; } }
        public static int SubjectCount { get { return Instance._subjects.Count; } }
        public static int IndentLevel { get; private set; }
        public static string IndentRepresentation { get; set; } = "  ";

        #endregion Public Properties

        #region Public Methods

        public static void Initialize(bool initializeStdOutput = true) {
            if (Instance != null) {
                return;
            }

            Instance = new Logger();

            if (initializeStdOutput) {
                Instance._listeners.Add(new StdOutputLoggerListener());
            }
        }

        public static void Deinitialize() {
            if (Instance == null) {
                return;
            }

            foreach (ILoggerListener listener in Instance._listeners) {
                listener.Dispose();
            }

            Instance._listeners.Clear();
            Instance = null;
        }

        public static void RegisterListener(ILoggerListener listener) {
            CheckInitialization();
            Instance._listeners.Add(listener);
        }

        public static void RemoveListener(ILoggerListener listener) {
            CheckInitialization();
            listener.Dispose();
            Instance._listeners.Remove(listener);
        }

        public static void ClearListeners() {
            CheckInitialization();
            Instance._listeners.Clear();
        }

        public static void Write(string category, string message) {
            CheckInitialization();

            MessageLoggerTokenTree tokens = BuildTokens(
                category,
                message,
                appendNewLine: false
            );

            foreach (ILoggerListener listener in Instance._listeners) {
                listener.WriteTokens(tokens);
            }
        }

        public static void Write(string message) {
            Write(string.Empty, message);
        }

        public static void WriteLine(string category, string message) {
            CheckInitialization();

            MessageLoggerTokenTree tokens = BuildTokens(
                category,
                message,
                appendNewLine: true
            );

            foreach (ILoggerListener listener in Instance._listeners) {
                listener.WriteTokens(tokens);
            }
        }

        public static void WriteLine(string message) {
            WriteLine(string.Empty, message);
        }

        public static void Info(string message) {
            WriteLine("info", message);
        }

        public static void Warning(string message) {
            WriteLine("warning", message);
        }

        public static void Critical(string message) {
            WriteLine("critical", message);
        }

        public static void Error(string message) {
            WriteLine("error", message);
        }

        public static void Success(string message) {
            WriteLine("success", message);
        }

        public static void PushSubject(string subject) {
            CheckInitialization();

            if (subject == null) {
                throw new System.ArgumentNullException(nameof(subject));
            }

            string lastSubject = LastSubject;
            if (lastSubject != null && subject == lastSubject) {
                throw new System.InvalidOperationException($"Can't push subject '{subject}', it should not be equals as last subject.");
            }

            Instance._subjects.Add(subject);
        }

        public static void PushSubjects(params string[] subjects) {
            CheckInitialization();
            foreach (string subject in subjects) {
                PushSubject(subject);
            }
        }

        public static void PopSubject(int amount = 1) {
            CheckInitialization();

            if (amount < 0) {
                throw new System.ArgumentException($"{nameof(amount)} should be a positive integer.");
            }

            if (amount == 0 || Instance._subjects.Count == 0) {
                return;
            }

            if (Instance._subjects.Count - amount <= 0) {
                Instance._subjects.Clear();
            } else {
                Instance._subjects.RemoveRange(Instance._subjects.Count - amount, amount);
            }
        }

        public static void ClearSubjects() {
            CheckInitialization();
            Instance._subjects.Clear();
        }

        public static void Indent(int amount = 1) {
            if (amount == 0) {
                return;
            }

            if (amount < 0) {
                throw new System.ArgumentException("Amount should be a positive integer.");
            }

            IndentLevel += amount;
            StringBuilder indentBuilder = new StringBuilder(IndentLevel * IndentRepresentation.Length);
            for (int i = 0; i < IndentLevel; i++) {
                indentBuilder.Append(IndentRepresentation);
            }

            Instance._indent = indentBuilder.ToString();
        }

        public static void Unindent(int amount = 1) {
            if (amount == 0) {
                return;
            }

            if (amount < 0) {
                throw new System.ArgumentException("Amount should be a positive integer.");
            }

            if (amount >= IndentLevel) {
                Instance._indent = string.Empty;
                IndentLevel = 0;
                return;
            }

            IndentLevel -= amount;
            StringBuilder indentBuilder = new StringBuilder(IndentLevel * IndentRepresentation.Length);
            for (int i = 0; i < IndentLevel; i++) {
                indentBuilder.Append(IndentRepresentation);
            }

            Instance._indent = indentBuilder.ToString();
        }

        public static void UnindentAll() {
            if (IndentLevel == 0) {
                return;
            }

            Instance._indent = string.Empty;
            IndentLevel = 0;
        }

        #endregion Public Methods

        #region Private Methods

        private static void CheckInitialization() {
            if (Instance == null) {
                throw new System.InvalidOperationException("Please, call Initialize() before doing any operation.");
            }
        }

        private static MessageLoggerTokenTree BuildTokens(string category, string message, bool appendNewLine = true) {
            MessageLoggerTokenTree messageTokenTree = new MessageLoggerTokenTree() {
                HeaderToken = new HeaderLoggerToken() {
                    TimestampToken = new TimestampLoggerToken(System.DateTime.Now)
                }
            };

            if (!string.IsNullOrEmpty(category)) {
                messageTokenTree.HeaderToken.CategoryToken = new CategoryLoggerToken(category);
            }

            if (Instance._subjects.Count > 0) {
                messageTokenTree.HeaderToken.SubjectsToken = new SubjectsLoggerToken();
                foreach (string subject in Instance._subjects) {
                    messageTokenTree.HeaderToken.SubjectsToken.AddSubject(subject);
                }
            }

            if (appendNewLine) {
                if (message != null) {
                    message += NewLine;
                } else {
                    message = NewLine;
                }
            }

            if (!string.IsNullOrEmpty(message)) {
                messageTokenTree.TextToken = new TextLoggerToken() {
                    Text = message
                };
            }

            return messageTokenTree;
        }

        /*
        private static void WriteMessageHeader(ILoggerListener listener, string context) {
            int length = 0;
            listener.Write("start-message", "");
            int timestampTextLength = WriteTimestamp(listener, System.DateTime.Now);
            listener.Write("spacing", "  ");

            if (string.IsNullOrEmpty(context)) {
                throw new System.ArgumentException($"Can't write message header with invalid context '{context}'.");
            }

            Instance._lastContext = context;
            listener.Write(context, context);
            listener.Write("spacing", "  ");
            length += timestampTextLength + 2 + context.Length + 2;
            WriteSubjects(listener, out int subjectsLength);
            Instance._previousHeaderLength = length;

            if (Instance._indent.Length > 0) {
                listener.Write(Instance._indent);
            }
        }

        private static void WriteMessageHeader(ILoggerListener listener) {
            int length = 0;
            listener.Write("start-message", "");
            int timestampTextLength = WriteTimestamp(listener, );
            listener.Write("spacing", "  ");
            length += timestampTextLength + 2;

            if (Instance._previousHeaderLength > length) {
                // padding
                listener.Write(new string(' ', Instance._previousHeaderLength - length));
            }

            WriteSubjects(listener, out int subjectsLength);

            if (Instance._indent.Length > 0) {
                listener.Write(Instance._indent);
            }
        }

        private static void WriteSubjects(ILoggerListener listener, out int length) {
            length = 0;
            if (Instance._subjects.Count == 0) {
                return;
            }

            string subject = Instance._subjects[0];
            listener.Write("subject-name", subject);
            length += subject.Length;
            for (int i = 1; i < Instance._subjects.Count; i++) {
                subject = Instance._subjects[i];
                listener.Write("subject-separator", "->");
                listener.Write("subject-name", subject);
                length += subject.Length + 2;
            }

            listener.Write("spacing", "  ");
            length += 2;
        }

        private static int WriteTimestamp(ILoggerListener listener, System.DateTime datetime) {
            string timestamp = ;
            listener.Write("timestamp", timestamp);
            return timestamp.Length;
        }
        */

        #endregion Private Methods
    }
}