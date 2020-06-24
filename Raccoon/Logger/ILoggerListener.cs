using Raccoon.Log;

namespace Raccoon {
    public interface ILoggerListener : System.IDisposable {
        void WriteTokens(in MessageLoggerTokenTree tokens);
    }
}
