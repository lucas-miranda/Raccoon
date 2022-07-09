using System.Collections.Generic;

namespace Raccoon.Data.Parsers {
    public class InlineListEntriesToken : Token {
        public InlineListEntriesToken() : base(TokenKind.InlineListEntries) {
        }

        public List<ValueAssignToken> Entries { get; } = new List<ValueAssignToken>();

        public override string ToString() {
            return $"InlineListEntries ({Entries.Count})";
        }
    }
}
