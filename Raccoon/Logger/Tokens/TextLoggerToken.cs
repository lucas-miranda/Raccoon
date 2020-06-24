
namespace Raccoon.Log {
    public class TextLoggerToken : LoggerToken, System.IEquatable<TextLoggerToken> {
        public string Text { get; set; }

        public override bool Equals(LoggerToken token) {
            if (!(token is TextLoggerToken textToken)) {
                return false;
            }

            return Equals(textToken);
        }

        public virtual bool Equals(TextLoggerToken token) {
            return token.Text.Equals(Text);
        }
    }
}
