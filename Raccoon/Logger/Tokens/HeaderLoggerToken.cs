
namespace Raccoon.Log {
    public class HeaderLoggerToken : LoggerToken, System.IEquatable<HeaderLoggerToken> {
        public TimestampLoggerToken TimestampToken { get; set; }
        public CategoryLoggerToken CategoryToken { get; set; }
        public SubjectsLoggerToken SubjectsToken { get; set; }


        public override bool Equals(LoggerToken token) {
            if (!(token is HeaderLoggerToken headerToken)) {
                return false;
            }

            return Equals(headerToken);
        }

        public virtual bool Equals(HeaderLoggerToken token) {
            return token.TimestampToken.Equals(TimestampToken)
                && token.CategoryToken.Equals(CategoryToken)
                && token.SubjectsToken.Equals(SubjectsToken);
        }
    }
}
