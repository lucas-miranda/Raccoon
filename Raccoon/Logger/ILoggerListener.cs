namespace Raccoon {
    public interface ILoggerListener {
        void Write(string context, string message);
        void Write(string message);
    }
}
