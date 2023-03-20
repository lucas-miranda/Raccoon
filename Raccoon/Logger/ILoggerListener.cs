using Raccoon.Log;

namespace Raccoon {
    public interface ILoggerListener : System.IDisposable {
        string Name { get; }

        void WriteTokens(MessageLoggerTokenTree tokens);
    }
}
