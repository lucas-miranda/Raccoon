namespace Raccoon {
    public interface ILoggerListener : System.IDisposable {
        void Write(string context, string message);
        void Write(string message);
    }
}
