using Raccoon.Log;

namespace Raccoon {
    public interface ILoggerListener : System.IDisposable {
        void WriteTokens(MessageLoggerTokenTree tokens);
    }
}
