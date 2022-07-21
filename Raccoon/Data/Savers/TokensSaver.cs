
namespace Raccoon.Data.Parsers {
    public static class TokensSaver {
        public static void Handle(Token token, SaverState state) {
            if (TryValueAssignHandle(token, state)) {
                return;
            }

            if (TryListHandle(token, state)) {
                return;
            }
        }

        private static bool TryValueAssignHandle(Token token, SaverState state) {
            if (!(token is ValueAssignToken valueAssignToken)) {
                return false;
            }

            state.StringBuilder.Append(valueAssignToken.Identifier.Name);

            if (state.Settings.UseExplicitTypes && valueAssignToken.Type != null) {
                state.StringBuilder.Append(": ");
                state.StringBuilder.Append(valueAssignToken.Type.Type);
            }

            state.StringBuilder.Append(" = ");
            state.StringBuilder.Append(valueAssignToken.Value.AsString());
            return true;
        }

        private static bool TryListHandle(Token token, SaverState state) {
            if (!(token is ListToken<Token> listToken)) {
                return false;
            }

            state.PushLevel();

            if (token is NamedListToken<Token> namedListToken) {
                state.StringBuilder.Append(namedListToken.Identifier.Name);
            }

            return true;
        }
    }
}
