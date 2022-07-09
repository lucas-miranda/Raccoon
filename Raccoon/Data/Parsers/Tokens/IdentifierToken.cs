namespace Raccoon.Data.Parsers {
    public class IdentifierToken : Token {
        public IdentifierToken(string name) : base(TokenKind.Identifier) {
            Name = name;
        }

        public string Name { get; }

        public override string ToString() {
            return $"Identifier {Name}";
        }
    }
}
