using System.IO;
using System.Text;
using System.Collections.Generic;

using Raccoon.Log;

namespace Raccoon {
    public class StdOutputLoggerListener : ILoggerListener {
        #region Private Members

        private TextWriter _out;
        private Dictionary<System.Type, TextFormatter> _contexts;
        private Dictionary<string, TextFormatter> _categories;
        private int _categorySectionLength;
        private List<int> _sectionsTextLength = new List<int>();

        #endregion Private Members

        #region Constructors

        public StdOutputLoggerListener() {
            _out = System.Console.Out;
            System.Console.OutputEncoding = new UTF8Encoding();

            _contexts = new Dictionary<System.Type, TextFormatter> {
                { 
                    typeof(TimestampLoggerToken), 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.DarkGray
                    } 
                },
                { 
                    typeof(SubjectsLoggerToken), 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.DarkGray
                    } 
                }
            };

            _categories = new Dictionary<string, TextFormatter> {
                { 
                    "error", 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.Red
                    } 
                },
                { 
                    "info", 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.Blue
                    } 
                },
                { 
                    "warning", 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.Yellow
                    } 
                },
                { 
                    "critical", 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.DarkYellow
                    } 
                },
                { 
                    "success", 
                    new TextFormatter() { 
                        ForegroundColor = System.ConsoleColor.Green
                    } 
                }
            };

            // find category section length
            foreach (string key in _categories.Keys) {
                if (key.Length > _categorySectionLength) {
                    _categorySectionLength = key.Length;
                }
            }
        }

        #endregion Constructors

        #region Public Properties

        public bool IsDisposed { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void WriteTokens(MessageLoggerTokenTree tokens) {
            if (tokens == null) {
                throw new System.ArgumentNullException(nameof(tokens));
            }

            if (tokens.HeaderToken != null) {
                HeaderLoggerToken header = tokens.HeaderToken;

                if (header.TimestampToken != null) {
                    CalculateLeftPadding(0, header.TimestampToken.Timestamp);
                    WriteToken<TimestampLoggerToken>(header.TimestampToken.Timestamp);
                    WriteMessage("  ");
                } else {
                    WritePadding(sectionId: 0);
                }

                if (header.CategoryToken != null) {
                    string formatedCategoryName = string.Format($"{{0,{_categorySectionLength}}}", header.CategoryToken.CategoryName);
                    CalculateLeftPadding(1, formatedCategoryName);

                    if (_categories.TryGetValue(header.CategoryToken.CategoryName, out TextFormatter categoryFormatter)) {
                        WriteFormattedMessage(formatedCategoryName, categoryFormatter);
                    } else {
                        WriteToken<CategoryLoggerToken>(formatedCategoryName);
                    }

                    WriteMessage("  ");
                } else {
                    WritePadding(sectionId: 1);
                }

                if (header.SubjectsToken != null
                 && header.SubjectsToken.Subjects.Count > 0) {
                    string representation = header.SubjectsToken.Subjects[0];

                    for (int i = 1; i < header.SubjectsToken.Subjects.Count; i++) {
                        representation += "->" + header.SubjectsToken.Subjects[i];
                    }

                    WriteToken<SubjectsLoggerToken>(representation);
                    WriteMessage("  ");
                }
            }

            if (tokens.TextToken != null) {
                WriteToken<TextLoggerToken>(tokens.TextToken.Text);
            }

            Flush();
        }

        /*
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
        */

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            _out = null;
            _contexts.Clear();

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void WriteToken<T>(string representation) where T : LoggerToken {
            if (_contexts.TryGetValue(typeof(T), out TextFormatter textFormatter)) {
                WriteFormattedMessage(representation, textFormatter);
                return;
            }

            WriteMessage(representation);
        }

        protected virtual void WriteFormattedMessage(string message, TextFormatter formatter) {
            if (formatter.ForegroundColor.HasValue) {
                System.Console.ForegroundColor = formatter.ForegroundColor.Value;
            }

            if (formatter.BackgroundColor.HasValue) {
                System.Console.BackgroundColor = formatter.BackgroundColor.Value;
            }

            WriteMessage(message);
            System.Console.ResetColor();
        }

        protected virtual void WriteMessage(string message) {
            _out.Write(message);
        }

        protected virtual void Flush() {
            _out.Flush();
        }

        #endregion Protected Methods

        #region Private Methods

        private void CalculateLeftPadding(int sectionId, string text) {
            if (sectionId < _sectionsTextLength.Count) {
                _sectionsTextLength[sectionId] = text.Length + 2; // 2 => section spacing
            } else {
                _sectionsTextLength.Add(text.Length + 2);
            }
        }

        private void WritePadding(int sectionId) {
            if (_sectionsTextLength.Count > sectionId) {
                WriteMessage(new string(' ', _sectionsTextLength[sectionId]));
            }
        }

        #endregion Private Methods

        #region TextFormatter Class

        public class TextFormatter {
            public TextFormatter() {
            }

            public System.ConsoleColor? ForegroundColor { get; set; }
            public System.ConsoleColor? BackgroundColor { get; set; }
        }

        #endregion TextFormatter Class
    }
}
