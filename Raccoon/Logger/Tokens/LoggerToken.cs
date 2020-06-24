namespace Raccoon.Log {
    public abstract class LoggerToken : System.IEquatable<LoggerToken> {
        public abstract bool Equals(LoggerToken token);
    }
}
