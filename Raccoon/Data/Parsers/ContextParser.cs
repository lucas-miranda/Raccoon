
namespace Raccoon.Data.Parsers {
    public static class ContextParser {
        public static void Reduce(ParserState state) {
            // ident [inline_list_entries((ident[: type] = value);*)]
            while (state.LineStack.TryPop(out Token token)) {
                // type = any_value => type = defined_value
                if (TryDefineValueTokenType(token, state)) {
                    continue;
                }

                // identifier: type = value
                // identifier = value
                if (TryTypeAssignReduce(token, state)) {
                    continue;
                }

                state.ResultStack.Push(token);
            }
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `type = value`
        /// Try to turn AnyValueToken into DefinedValueToken.
        /// </summary>
        private static bool TryDefineValueTokenType(Token token, ParserState state) {
            if (!(token is TypeToken typeToken)) {
                return false;
            }

            if (!state.ResultStack.TryPeek(out Token nextToken)
             || !(nextToken is ValueToken valueNextToken)
            ) {
                // there is no next token available
                // or it isn't a ValueToken
                return false;
            }

            state.ResultStack.Pop(); // pop peeked ValueToken

            // adjust ValueToken to correct type DefinedValueToken<T>
            if (!(valueNextToken is AnyValueToken anyValueToken)) {
                throw new System.InvalidOperationException(
                    $"Expecting a value token to be an '{nameof(AnyValueToken)}', but it's '{valueNextToken.GetType().Name}'."
                );
            }

            ValueToken definedValueToken = typeToken.CreateValueToken(anyValueToken.Value);
            state.ResultStack.Push(definedValueToken);
            state.ResultStack.Push(typeToken);
            return true;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `ident: type = value`, `ident = value`
        /// </summary>
        private static bool TryTypeAssignReduce(Token token, ParserState state) {
            if (!(token is IdentifierToken identToken)) {
                return false;
            }

            if (!state.ResultStack.TryPeek(out Token nextToken)) {
                // there is no next token available
                return false;
            }

            if (nextToken is TypeToken typeNextToken) {
                // ident: type = value

                state.ResultStack.Pop(); // pop peeked TypeToken

                if (!state.ResultStack.TryPeek(out Token next2Token)) {
                    throw new System.InvalidOperationException(
                        $"Expecting a value token, after an identifier and type, but token list has ended."
                    );
                }

                if (!(next2Token is ValueToken valueNext2Token)) {
                    throw new System.InvalidOperationException(
                        $"Expecting a value token, after an identifier and type, but found '{next2Token.GetType().Name}'."
                    );
                }

                state.ResultStack.Pop(); // pop peeked ValueToken

                ValueAssignToken valueAssignToken = new ValueAssignToken(
                    identToken,
                    typeNextToken,
                    valueNext2Token
                );

                state.ResultStack.Push(valueAssignToken);
            } else if (nextToken is ValueToken valueNextToken) {
                // ident = value

                state.ResultStack.Pop(); // pop peeked ValueToken

                ValueAssignToken valueAssignToken = new ValueAssignToken(
                    identToken,
                    valueNextToken
                );

                state.ResultStack.Push(valueAssignToken);
            } else {
                return false;
            }

            return true;
        }
    }
}
