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

    public class TypedIdentifierToken : IdentifierToken {
        public TypedIdentifierToken(string name, TypeToken type) : base(name) {
            if (type == null) {
                throw new System.ArgumentNullException(nameof(type));
            }

            Type = type;
        }

        public TypeToken Type { get; }

        public override string ToString() {
            return $"TypedIdentifier ({Name}): ({Type})";
        }
    }
}
