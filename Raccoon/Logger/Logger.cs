using System.Collections.Generic;
using System.Text;
using System.IO;

using Raccoon.Log;

namespace Raccoon {
    public sealed class Logger {
        #region Private Members

        private List<ILoggerListener> _listeners = new List<ILoggerListener>();
        private List<string> _subjects = new List<string>();
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

            CanWrite = true;
        }

        #endregion Constructors

        #region Public Properties

        public static Logger Instance { get; private set; }
        public static bool IsInitialized { get { return Instance != null; } }
        public static bool CanWrite { get; set; }
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

        public static void Write(string message) {
            if (!CanWrite) {
                return;
            }

            CheckInitialization();

            if (string.IsNullOrEmpty(message)) {
                return;
            }

            MessageLoggerTokenTree tokens = new MessageLoggerTokenTree() {
                TextToken = new TextLoggerToken() {
                    Text = message
                }
            };

            foreach (ILoggerListener listener in Instance._listeners) {
                try {
                    listener.WriteTokens(tokens);
                } catch (System.Exception e) {
                    CanWrite = false;
                    System.Console.WriteLine($"Logger listener ({listener.GetType()}) raised an exception.");
                    LogListenerException(listener, e);
                    Game.Instance.Exit();
                }
            }
        }

        public static void WriteLine(string category, string message) {
            if (!CanWrite) {
                return;
            }

            CheckInitialization();

            MessageLoggerTokenTree tokens = BuildTokens(
                category,
                message,
                appendNewLine: true
            );

            foreach (ILoggerListener listener in Instance._listeners) {
                try {
                    listener.WriteTokens(tokens);
                } catch (System.Exception e) {
                    CanWrite = false;
                    System.Console.WriteLine($"Logger listener ({listener.GetType()}) raised an exception.");
                    LogListenerException(listener, e);
                    Game.Instance.Exit();
                }
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
            MessageLoggerTokenTree messageTokenTree = new MessageLoggerTokenTree();

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

                if (message.Equals(NewLine)) {
                    // just a new line message
                    // don't need to insert header and stuff
                    return messageTokenTree;
                }

                if (IndentLevel > 0) {
                    messageTokenTree.TextToken.Text = Instance._indent + messageTokenTree.TextToken.Text;
                }
            }

            messageTokenTree.HeaderToken = new HeaderLoggerToken() {
                TimestampToken = new TimestampLoggerToken(System.DateTime.Now)
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

            return messageTokenTree;
        }

        private static void LogListenerException(ILoggerListener listener, System.Exception e) {
            using (StreamWriter logWriter = new StreamWriter($"crash-report.log", append: false)) {
                logWriter.WriteLine($"Logger Listener ({listener.GetType()}) raised an exception:");
                logWriter.WriteLine($"\n{System.DateTime.Now.ToString()}  {e.Message}\n{e.StackTrace}\n");

                while (e.InnerException != null) {
                    e = e.InnerException;
                    logWriter.WriteLine($"{System.DateTime.Now.ToString()}  InnerException: {e.Message}\n{e.StackTrace}\n");
                }

                logWriter.WriteLine("\n\nreport.log\n-------------\n");
                if (File.Exists(Debug.LogFileName)) {
                    logWriter.WriteLine(File.ReadAllText(Debug.LogFileName));
                } else {
                    logWriter.WriteLine("  No 'report.log' file");
                }
            }
        }

        #endregion Private Methods
    }
}
