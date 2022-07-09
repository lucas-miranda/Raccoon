namespace Raccoon.Data.Parsers {
    public abstract class Token {
        public Token(TokenKind kind) {
            Kind = kind;
        }

        public TokenKind Kind { get; }
    }
}
