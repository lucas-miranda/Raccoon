
namespace Raccoon.Log {
    public class TimestampLoggerToken : LoggerToken, System.IEquatable<TimestampLoggerToken> {
        public TimestampLoggerToken(System.DateTime dateTime) {
            DateTime = dateTime;
            Timestamp = dateTime.ToString("dd/MM/yyyy HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        }

        public TimestampLoggerToken(System.DateTime dateTime, string timestampFormat) {
            DateTime = dateTime;
            Timestamp = dateTime.ToString(timestampFormat, System.Globalization.CultureInfo.InvariantCulture);
        }

        public System.DateTime DateTime { get; private set; }
        public string Timestamp { get; private set; }

        public bool IsEarlier(TimestampLoggerToken timestampToken) {
            return DateTime.CompareTo(timestampToken.DateTime) < 0;
        }

        public bool IsLater(TimestampLoggerToken timestampToken) {
            return DateTime.CompareTo(timestampToken.DateTime) > 0;
        }

        public override bool Equals(LoggerToken token) {
            if (!(token is TimestampLoggerToken timestampToken)) {
                return false;
            }

            return Equals(timestampToken);
        }

        public virtual bool Equals(TimestampLoggerToken token) {
            return token.DateTime.Equals(DateTime);
        }
    }
}
