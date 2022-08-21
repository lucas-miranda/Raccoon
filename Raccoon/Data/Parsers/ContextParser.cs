
namespace Raccoon.Data.Parsers {
    public static class ContextParser {
        public static void Reduce(ParserState state) {
            // ident [inline_list((ident[: type] = value);*)]
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

                // identifier: type
                if (TryTypedIdentifierReduce(token, state)) {
                    continue;
                }

                // inline_list (ident[: type] = value);* inline_list_close => inline_list
                if (TryInlineListEntriesReduce(token, state)) {
                    continue;
                }

                // ident inline_list_entries => named_list
                if (TryNamedListReduce(token, state)) {
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
                    $"Expecting a value token to be an '{nameof(AnyValueToken)}', but it's '{valueNextToken.GetType().ToString()}'."
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

            if (identToken is TypedIdentifierToken typedIdentToken) {
                if (nextToken is ValueToken valueNextToken) {
                    // typed_ident = value

                    state.ResultStack.Pop(); // pop peeked ValueToken

                    ValueAssignToken valueAssignToken = new ValueAssignToken(
                        new IdentifierToken(typedIdentToken.Name),
                        typedIdentToken.Type,
                        valueNextToken
                    );

                    state.ResultStack.Push(valueAssignToken);
                } else {
                    return false;
                }
            } else{
                if (nextToken is TypeToken typeNextToken) {
                    // ident: type = value

                    state.ResultStack.Pop(); // pop peeked TypeToken

                    if (!state.ResultStack.TryPeek(out Token next2Token)) {
                        // it doesn't has more tokens
                        // and we need at least 1 more to be the value
                        state.ResultStack.Push(nextToken); // return poped TypeToken
                        return false;
                    }

                    if (!(next2Token is ValueToken valueNext2Token)) {
                        throw new System.InvalidOperationException(
                            $"Expecting a value token, after an identifier and type, but found '{next2Token.GetType().ToString()}'."
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
            }

            return true;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `ident: type`
        /// </summary>
        private static bool TryTypedIdentifierReduce(Token token, ParserState state) {
            if (!(token is IdentifierToken identToken)) {
                return false;
            }

            if (!state.ResultStack.TryPeek(out Token nextToken)
             || !(nextToken is TypeToken typeNextToken)
            ) {
                // there is no next token available
                // or it isn't a type token
                return false;
            }

            state.ResultStack.Pop(); // pop peeked TypeToken

            TypedIdentifierToken typedIdentToken = new TypedIdentifierToken(
                identToken.Name,
                typeNextToken
            );

            state.ResultStack.Push(typedIdentToken);
            return true;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `inline_list (ident[: type] = value);* inline_list_close`
        /// Every enclosed tokens which are of valid types: ValueAssignToken. Will be registered as an inline list entry.
        /// </summary>
        private static bool TryInlineListEntriesReduce(Token token, ParserState state) {
            if (!(token is InlineListEntriesToken inlineListToken)) {
                return false;
            }

            while (state.ResultStack.TryPop(out Token resultToken)) {
                if (resultToken.Kind == TokenKind.InlineListEntriesClose) {
                    // end of inline list entries
                    break;
                } else if (resultToken is ValueAssignToken valueAssignToken) {
                    inlineListToken.Entries.Add(valueAssignToken);
                } else {
                    throw new System.InvalidOperationException($"Invalid inline list entry '{resultToken}'.\nPossible values are: {nameof(ValueAssignToken)} or list close.");
                }
            }

            state.ResultStack.Push(inlineListToken);
            return true;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `ident inline_list_entries => named_list`
        /// </summary>
        private static bool TryNamedListReduce(Token token, ParserState state) {
            if (!(token is IdentifierToken identToken)) {
                return false;
            }

            if (!state.ResultStack.TryPeek(out Token nextToken)
             || !(nextToken is InlineListEntriesToken inlineListEntriesNextToken)
            ) {
                // there is no next token available
                // or it isn't a type token
                return false;
            }

            state.ResultStack.Pop(); // pop peeked InlineListEntriesToken

            NamedListToken<Token> namedListToken
                = new NamedListToken<Token>(identToken);

            namedListToken.Entries.AddRange(inlineListEntriesNextToken.Entries);
            state.ResultStack.Push(namedListToken);
            return true;
        }
    }
}
