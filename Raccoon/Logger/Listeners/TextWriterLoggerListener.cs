using System.IO;

namespace Raccoon {
    public class TextWriterLoggerListener : ILoggerListener {
        private TextWriter _writer;

        public TextWriterLoggerListener(TextWriter writer) {
            if (writer == null) {
                throw new System.ArgumentNullException(nameof(writer));
            }

            _writer = writer;
        }

        public TextWriterLoggerListener(string filepath) : this(new StreamWriter(filepath, append: false, System.Text.Encoding.UTF8)) {
        }

        public bool AutoFlush { get; set; } = true;
        public bool IsDisposed { get; private set; }

        public void Write(string context, string message) {
            _writer.Write(message);

            if (AutoFlush) {
                _writer.Flush();
            }
        }

        public void Write(string message) {
            Write(string.Empty, message);
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            _writer.Close();
            _writer.Dispose();

            IsDisposed = true;
        }
    }
}
