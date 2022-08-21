using System.Collections.Generic;

namespace Raccoon.Data.Parsers {
    public class ListToken<T> : Token where T : Token {
        public ListToken() : base(TokenKind.List) {
            Entries = new List<T>();
        }

        public ListToken(IEnumerable<T> entries) : base(TokenKind.List) {
            Entries = new List<T>(entries);
        }

        public List<T> Entries { get; }

        public override string ToString() {
            return $"List ({Entries.Count})";
        }
    }

    public class NamedListToken<T> : ListToken<T> where T : Token {
        public NamedListToken(IdentifierToken identifier) : base() {
            Identifier = identifier;
        }

        public NamedListToken(IdentifierToken identifier, IEnumerable<T> entries) : base(entries) {
            Identifier = identifier;
        }

        public IdentifierToken Identifier { get; }

        public override string ToString() {
            return $"List {Identifier} ({Entries.Count})";
        }
    }
}
