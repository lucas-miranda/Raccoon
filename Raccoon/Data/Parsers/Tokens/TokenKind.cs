namespace Raccoon.Data.Parsers {
    public enum TokenKind {
        Undefined = 0,
        Identifier,
        Type,
        Value,
        List,
        InlineListEntries,
        InlineListEntriesClose,
        InlineListEntryEnd,
        ValueAssign,
        Comment,
    }
}
