namespace Raccoon.Data.Parsers {
    public class ValueAssignToken : Token {
        public ValueAssignToken(
            IdentifierToken identifier,
            TypeToken type,
            ValueToken value
        ) : base(TokenKind.ValueAssign) {
            if (identifier == null) {
                throw new System.ArgumentNullException(nameof(identifier));
            }

            if (type == null) {
                throw new System.ArgumentNullException(nameof(type));
            }

            if (value == null) {
                throw new System.ArgumentNullException(nameof(value));
            }

            Identifier = identifier;
            Type = type;
            Value = value;
        }

        public ValueAssignToken(
            IdentifierToken identifier,
            ValueToken value
        ) : base(TokenKind.ValueAssign) {
            if (identifier == null) {
                throw new System.ArgumentNullException(nameof(identifier));
            }

            if (value == null) {
                throw new System.ArgumentNullException(nameof(value));
            }

            Identifier = identifier;
            Type = null;
            Value = value;
        }

        public IdentifierToken Identifier { get; }
        public TypeToken Type { get; }
        public ValueToken Value { get; }

        public override string ToString() {
            if (Type == null) {
                return $"ValueAssign ({Identifier}) = ({Value})";
            }

            return $"ValueAssign ({Identifier}): ({Type}) = ({Value})";
        }
    }
}
