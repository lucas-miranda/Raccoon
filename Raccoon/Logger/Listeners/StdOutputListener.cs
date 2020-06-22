using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Raccoon {
    public class StdOutputListener : ILoggerListener {
        #region Private Members

        private TextWriter _out;
        private Dictionary<string, Context> _contexts;

        #endregion Private Members

        #region Constructors

        public StdOutputListener() {
            _out = System.Console.Out;
            System.Console.OutputEncoding = new UTF8Encoding();

            _contexts = new Dictionary<string, Context> {
                { 
                    "timestamp", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.DarkGray
                    } 
                },
                { 
                    "error", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.Red
                    } 
                },
                { 
                    "info", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.Blue
                    } 
                },
                { 
                    "warning", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.Yellow
                    } 
                },
                { 
                    "critical", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.DarkYellow
                    } 
                },
                { 
                    "success", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.Green
                    } 
                },
                { 
                    "subject-name", 
                    new Context() { 
                        ForegroundColor = System.ConsoleColor.White
                    } 
                },
            };
        }

        #endregion Constructors

        #region Public Methods

        public void Write(string context, string message) {
            if (_contexts.TryGetValue(context, out Context c)) {
                HandleContextMessage(c, message);
                return;
            }

            HandleDefaultMessage(message);
        }

        public void Write(string message) {
            Write(string.Empty, message);
        }

        public Context GetContext(string contextIdentifier) {
            if (_contexts.TryGetValue(contextIdentifier, out Context context)) {
                return context;
            }

            return null;
        }

        public void RegisterContext(string contextIdentifier, Context context, bool overrides = false) {
            if (overrides) {
                _contexts[contextIdentifier] = context;
                return;
            }

            if (_contexts.ContainsKey(contextIdentifier)) {
                throw new System.InvalidOperationException($"Context indentifier '{contextIdentifier}' is already registered.");
            }

            _contexts.Add(contextIdentifier, context);
        }

        public bool RemoveContext(string contextIdentifier) {
            return _contexts.Remove(contextIdentifier);
        }

        public void ClearContexts() {
            _contexts.Clear();
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void HandleDefaultMessage(string message) {
            WriteMessageToOutput(message);
        }

        protected virtual void HandleContextMessage(Context category, string message) {
            if (category.ForegroundColor.HasValue) {
                System.Console.ForegroundColor = category.ForegroundColor.Value;
            }

            if (category.BackgroundColor.HasValue) {
                System.Console.BackgroundColor = category.BackgroundColor.Value;
            }

            WriteMessageToOutput(message);
            System.Console.ResetColor();
        }

        protected virtual void WriteMessageToOutput(string message) {
            _out.Write(message);
            _out.Flush();
        }

        #endregion Protected Methods

        #region Context Class

        public class Context {
            public Context() {
            }

            public System.ConsoleColor? ForegroundColor { get; set; }
            public System.ConsoleColor? BackgroundColor { get; set; }
        }

        #endregion Context Class
    }
}
