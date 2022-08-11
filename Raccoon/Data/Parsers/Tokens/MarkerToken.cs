namespace Raccoon.Data.Parsers {
    public class MarkerToken : Token {
        public MarkerToken(TokenKind kind) : base(kind) {
        }

        public override string ToString() {
            return $"Marker {Kind}";
        }
    }
}
