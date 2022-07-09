using System.Reflection;

namespace Raccoon.Data.Parsers {
    public abstract class ValueToken : Token {
        public ValueToken() : base(TokenKind.Value) {
        }

        public abstract string AsString();
        public abstract void SetPropertyValue(object target, PropertyInfo info);
    }
}
