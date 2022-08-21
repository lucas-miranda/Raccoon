
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

                //

                state.ResultStack.Push(token);
            }
        }

        #region Reduces

        /// <summary>
        /// Try to reduce tokens from an expression: `type = value`
        /// Try to turn AnyValueToken into DefinedValueToken.
        /// </summary>
        private static bool TryDefineValueTokenType(Token token, ParserState state) {
            if (!Expressions.Match<TypeToken, ValueToken>(
                token,
                state,
                out TypeToken typeToken,
                out ValueToken valueToken
            )) {
                return false;
            }

            // adjust ValueToken to correct type DefinedValueToken<T>
            if (!(valueToken is AnyValueToken anyValueToken)) {
                throw new System.InvalidOperationException(
                    $"Expecting a value token to be an '{nameof(AnyValueToken)}', but it's '{valueToken.GetType().ToString()}'."
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
            if (Expressions.Match<TypedIdentifierToken, ValueToken>(
                token,
                state,
                out TypedIdentifierToken typedIdentToken,
                out ValueToken valueToken
            )) {
                // typed_ident = value

                state.ResultStack.Push(new ValueAssignToken(
                    new IdentifierToken(typedIdentToken.Name),
                    typedIdentToken.Type,
                    valueToken
                ));

                return true;
            } else if (Expressions.Match<IdentifierToken, TypeToken, ValueToken>(
                token,
                state,
                out IdentifierToken identToken,
                out TypeToken typeToken,
                out valueToken
            )) {
                // ident: type = value

                state.ResultStack.Push(new ValueAssignToken(
                    identToken,
                    typeToken,
                    valueToken
                ));

                return true;
            } else if (Expressions.Match<IdentifierToken, ValueToken>(
                token,
                state,
                out identToken,
                out valueToken
            )) {
                // ident = value

                state.ResultStack.Push(new ValueAssignToken(
                    identToken,
                    valueToken
                ));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `ident: type`
        /// </summary>
        private static bool TryTypedIdentifierReduce(Token token, ParserState state) {
            if (!Expressions.Match<IdentifierToken, TypeToken>(
                token,
                state,
                out IdentifierToken identToken,
                out TypeToken typeToken
            )) {
                return false;
            }

            TypedIdentifierToken typedIdentToken = new TypedIdentifierToken(
                identToken.Name,
                typeToken
            );

            state.ResultStack.Push(typedIdentToken);
            return true;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `inline_list (ident[: type] = value);* inline_list_close`
        /// Every enclosed tokens which are of valid types: ValueAssignToken. Will be registered as an inline list entry.
        /// </summary>
        private static bool TryInlineListEntriesReduce(Token token, ParserState state) {
            if (!Expressions.Match<InlineListEntriesToken>(
                token,
                out InlineListEntriesToken inlineListToken
            )) {
                return false;
            }

            // register every valid token as an inline list entry
            // until an entries close marker is found
            while (state.ResultStack.TryPop(out Token resultToken)
             && resultToken.Kind != TokenKind.InlineListEntriesClose
            ) {
                if (resultToken is ValueAssignToken valueAssignToken) {
                    inlineListToken.Entries.Add(valueAssignToken);
                } else {
                    throw new System.InvalidOperationException(
                        $"Invalid inline list entry '{resultToken}'.\nPossible values are: {nameof(ValueAssignToken)} or list close."
                    );
                }
            }

            state.ResultStack.Push(inlineListToken);
            return true;
        }

        /// <summary>
        /// Try to reduce tokens from an expression: `ident inline_list_entries`
        /// </summary>
        private static bool TryNamedListReduce(Token token, ParserState state) {
            if (!Expressions.Match<IdentifierToken, InlineListEntriesToken>(
                token,
                state,
                out IdentifierToken identToken,
                out InlineListEntriesToken inlineListEntriesToken
            )) {
                return false;
            }

            state.ResultStack.Push(new NamedListToken<Token>(
                identToken,
                inlineListEntriesToken.Entries
            ));

            return true;
        }

        #endregion Reduces
    }
}
