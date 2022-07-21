using System.Collections.Generic;

namespace Raccoon.Data.Parsers {
    public class ListToken<T> : Token where T : Token {
        public ListToken() : base(TokenKind.List) {
        }

        public List<T> Entries { get; } = new List<T>();

        public override string ToString() {
            return $"List ({Entries.Count})";
        }
    }

    public class NamedListToken<T> : ListToken<T> where T : Token {
        public NamedListToken(IdentifierToken identifier) : base() {
            Identifier = identifier;
        }

        public IdentifierToken Identifier { get; }

        public override string ToString() {
            return $"List {Identifier} ({Entries.Count})";
        }
    }
}
