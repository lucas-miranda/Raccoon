using System.Collections.Generic;

namespace Raccoon.Log {
    public class MessageLoggerTokenTree : LoggerToken, System.IEquatable<MessageLoggerTokenTree> {
        public HeaderLoggerToken HeaderToken { get; set; }
        public TextLoggerToken TextToken { get; set; }

        public List<LoggerToken> Decompose() {
            return new List<LoggerToken>() {
                HeaderToken.TimestampToken,
                HeaderToken.CategoryToken,
                HeaderToken.SubjectsToken,
                TextToken
            };
        }

        public override bool Equals(LoggerToken token) {
            if (!(token is MessageLoggerTokenTree messageToken)) {
                return false;
            }

            return Equals(messageToken);
        }

        public virtual bool Equals(MessageLoggerTokenTree token) {
            return token.HeaderToken.Equals(HeaderToken)
                && token.TextToken.Equals(TextToken);
        }
    }
}
