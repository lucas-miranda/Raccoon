using System.IO;

using Raccoon.FileSystem;

namespace Raccoon {
    public class CrashHandler {
        #region Public Members

        public event Game.GameLogEventWriter OnCrash;

        public const string DefaultLogFilename = "crash-report.log";

        #endregion Public Members

        #region Constructors

        public CrashHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public bool HasStarted { get; private set; }
        public bool HasGameCrashed { get; private set; }
        public bool IncludeReportAtLog { get; set; } = true;
        public string LogFilename { get; set; } = DefaultLogFilename;
        public PathBuf LogDirectoryPath { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void Initialize() {
            if (HasStarted) {
                return;
            }

            HasStarted = true;
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Initialized();
        }

        public void Deinitialize() {
            if (!HasStarted) {
                return;
            }

            Deinitialized();
            System.AppDomain.CurrentDomain.UnhandledException -= UnhandledException;
            HasStarted = false;
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Initialized() {
        }

        protected virtual void Deinitialized() {
        }

        protected virtual void Crashed(StreamWriter logWriter) {
        }

        protected string BeautifyBool(bool b) {
            return b ? "yes" : "no";
        }

        /*
        protected string BeautifyObject(object o) {
            return o == null ? "none" : o.ToString();
        }
        */

        protected string BeautifyObjectExists(object o) {
            return o == null ? "no" : "yes";
        }

        #endregion Protected Methods

        #region Private Methods

        private void UnhandledException(object sender, System.UnhandledExceptionEventArgs args) {
            HasGameCrashed = true;
            Logger.ClearSubjects(); // not dangling subjects should remain
            System.Exception e = (System.Exception) args.ExceptionObject;

            // resolve log filepath
            if (string.IsNullOrEmpty(LogFilename)) {
                LogFilename = DefaultLogFilename;
            }

            PathBuf reportLogFilepath, crashLogFilepath;

            if (LogDirectoryPath != null) {
                // using user provided log directory path
                crashLogFilepath = new PathBuf(LogDirectoryPath);
            } else {
                // using base directory as log directory path
                crashLogFilepath = FileSystem.Directories.Base;
            }

            reportLogFilepath = Debug.ReportFilepath;
            crashLogFilepath.Push(LogFilename);

            Logger.Info($"Report log path: {reportLogFilepath}");
            Logger.Info($"Crash log path: {crashLogFilepath}");

            //

            using (StreamWriter logWriter = new StreamWriter(crashLogFilepath.ToString(), false)) {
                logWriter.WriteLine($"Operating System: {System.Environment.OSVersion} ({(System.Environment.Is64BitOperatingSystem ? "x64" : "x86")})");
                logWriter.WriteLine($"CLR Runtime Version: {System.Environment.Version}");
                logWriter.WriteLine($"Command Line: {System.Environment.CommandLine}\n\n");
                logWriter.Flush();

                try {
                    Crashed(logWriter);
                } catch (System.Exception onCrashException) {
                    logWriter.WriteLine($"{GetType().ToString()}.Crashed(StreamWriter) raised an exception: {onCrashException.Message}\n{onCrashException.StackTrace}\n\n");
                } finally {
                    logWriter.Flush();
                }

                logWriter.WriteLine("");

                try {
                    OnCrash?.Invoke(logWriter);
                } catch (System.Exception onCrashException) {
                    logWriter.WriteLine($"CrashHandler.OnCrash raised an exception: {onCrashException.Message}\n{onCrashException.StackTrace}\n\n");
                } finally {
                    logWriter.Flush();
                }

                logWriter.WriteLine($"\n{System.DateTime.Now.ToString()}  {e.Message}\n{e.StackTrace}\n");

                // unroll inner exceptions
                while (e.InnerException != null) {
                    e = e.InnerException;
                    logWriter.WriteLine($"{System.DateTime.Now.ToString()}  InnerException: {e.Message}\n{e.StackTrace}\n");
                }

                if (IncludeReportAtLog) {
                    logWriter.WriteLine($"\n\nGame Report\n-------------\n{reportLogFilepath}\n-------------\n");

                    if (reportLogFilepath.ExistsFile()) {
                        logWriter.WriteLine(File.ReadAllText(reportLogFilepath.ToString()));
                    } else {
                        logWriter.WriteLine($"  No game report file found.");
                    }
                }
            }
        }

        #endregion Private Methods
    }
}
