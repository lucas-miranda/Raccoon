using System.Collections.Generic;
using System.IO;

using Raccoon.Log;

namespace Raccoon {
    public class TextWriterLoggerListener : ILoggerListener {
        private TextWriter _writer;
        private List<int> _sectionsTextLength = new List<int>();

        public TextWriterLoggerListener(TextWriter writer) {
            if (writer == null) {
                throw new System.ArgumentNullException(nameof(writer));
            }

            _writer = writer;
        }

        public TextWriterLoggerListener(string filepath) : this(new StreamWriter(filepath, false, System.Text.Encoding.UTF8)) {
        }

        public bool AutoFlush { get; set; } = true;
        public bool IsDisposed { get; private set; }

        public void WriteTokens(MessageLoggerTokenTree tokens) {
            if (tokens == null) {
                throw new System.ArgumentNullException(nameof(tokens));
            }

            if (tokens.HeaderToken != null) {
                HeaderLoggerToken header = tokens.HeaderToken;

                if (header.TimestampToken != null) {
                    CalculateLeftPadding(0, header.TimestampToken.Timestamp);
                    Write(header.TimestampToken.Timestamp);
                    Write("  ");
                } else {
                    WritePadding(sectionId: 0);
                }

                if (header.CategoryToken != null) {
                    CalculateLeftPadding(1, header.CategoryToken.CategoryName);
                    Write(header.CategoryToken.CategoryName);
                    Write("  ");
                } else {
                    WritePadding(sectionId: 1);
                }

                if (header.SubjectsToken != null
                 && header.SubjectsToken.Subjects.Count > 0) {
                    string representation = header.SubjectsToken.Subjects[0];

                    for (int i = 1; i < header.SubjectsToken.Subjects.Count; i++) {
                        representation += "->" + header.SubjectsToken.Subjects[i];
                    }

                    Write(representation);
                    Write("  ");
                }
            }

            if (tokens.TextToken != null) {
                Write(tokens.TextToken.Text);
            }

            if (AutoFlush) {
                Flush();
            }
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            _writer.Close();
            _writer.Dispose();

            IsDisposed = true;
        }

        protected virtual void Write(string message) {
            _writer.Write(message);
        }

        protected virtual void Flush() {
            _writer.Flush();
        }

        private void CalculateLeftPadding(int sectionId, string text) {
            if (sectionId < _sectionsTextLength.Count) {
                _sectionsTextLength[sectionId] = text.Length + 2; // 2 => section spacing
            } else {
                _sectionsTextLength.Add(text.Length + 2);
            }
        }

        private void WritePadding(int sectionId) {
            if (_sectionsTextLength.Count > sectionId) {
                Write(new string(' ', _sectionsTextLength[sectionId]));
            }
        }
    }
}
