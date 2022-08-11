

namespace Raccoon.Data.Parsers {
    public static class TokensParser {
        private static readonly char[] IdentifierTrimChars = new char[] { ' ' };

        /// <summary>
        /// Greedily evaluates a line of text and converts into tokens.
        /// No reductions or context transformation is applied, it's just a raw conversion.
        /// </summary>
        public static bool ParseLine(string line, ParserState state, out int level) {
            if (string.IsNullOrWhiteSpace(line)) {
                // ignore whitespace or empty lines
                level = 0;
                return false;
            }

            // current token kind
            TokenKind kind = TokenKind.Identifier;

            // identify whitespaces at begin
            int whitespaceCount = 0;

            for (int i = 0; i < line.Length; i++) {
                if (line[i] != ' ') {
                    break;
                }

                whitespaceCount += 1;
            }

            // defines whitespaces per level
            if (whitespaceCount > 0) {
                if (state.SpacePerLevel == null) {
                    state.SpacePerLevel = whitespaceCount;
                    level = 1;
                } else {
                    if (whitespaceCount % state.SpacePerLevel.Value != 0) {
                        throw new System.InvalidOperationException(
                            $"A wrong amount of spaces was used. Only multiples of {state.SpacePerLevel.Value} is allowed, but {whitespaceCount} was found."
                        );
                    }

                    level = whitespaceCount / state.SpacePerLevel.Value;
                }
            } else {
                level = 0;
            }

            // parse line tokens
            int startIndex = whitespaceCount,
                endIndex = 0;

            while (endIndex < line.Length) {
                kind = TokenKind.Identifier;

                for (endIndex = startIndex; endIndex < line.Length; endIndex++) {
                    // look for symbols
                    char symbol = line[endIndex];

                    TokenKind? tokenStart = null;
                    switch (symbol) {
                        case '(':
                            // item inline list begin
                            tokenStart = TokenKind.InlineListEntries;
                            break;

                        case ')':
                            // item inline list end
                            tokenStart = TokenKind.InlineListEntriesClose;
                            break;

                        case ':':
                            // value type declaration
                            tokenStart = TokenKind.Type;
                            break;

                        case '=':
                            // value declaration
                            tokenStart = TokenKind.Value;
                            break;

                        case '/':
                            {
                                // check next symbol
                                if (endIndex + 1 < line.Length) {
                                    char nextSymbol = line[endIndex + 1];

                                    if (nextSymbol == '/') {
                                        tokenStart = TokenKind.Comment;
                                        break;
                                    }
                                }
                            }
                            break;

                        case ';':
                            // end of entry (mandatory at inline list declarations)
                            tokenStart = TokenKind.Identifier;
                            break;

                        default:
                            break;
                    }

                    if (tokenStart.HasValue) {
                        if (startIndex == endIndex) {
                            kind = tokenStart.Value;

                            if (tokenStart.Value == TokenKind.Comment) {
                                break;
                            }

                            // parse of token just begun
                            endIndex += 1;
                            break;
                        } else if (kind == TokenKind.Identifier) {
                            // identifier can't have any nested tokens
                            // break here to resume later
                            break;
                        } else {
                            throw new System.NotImplementedException(
                                $"Unhandled token start '{tokenStart.Value}' (current kind: {kind})"
                            );
                        }
                    }
                }

                switch (kind) {
                    case TokenKind.Identifier:
                        state.LineStack.Push(new IdentifierToken(
                            line.Substring(startIndex, endIndex - startIndex)
                                .Trim(IdentifierTrimChars)
                        ));
                        break;

                    case TokenKind.InlineListEntries:
                        state.LineStack.Push(new InlineListEntriesToken());
                        break;

                    case TokenKind.InlineListEntriesClose:
                        state.LineStack.Push(
                            new MarkerToken(TokenKind.InlineListEntriesClose)
                        );

                        kind = TokenKind.Identifier; // reset to default type
                        break;

                    case TokenKind.Type:
                        state.LineStack.Push(new TypeToken());
                        break;

                    case TokenKind.Value:
                        state.LineStack.Push(new AnyValueToken());
                        break;

                    case TokenKind.Comment:
                        // ignore the rest of line
                        endIndex = line.Length;
                        break;

                    default:
                        throw new System.NotImplementedException(
                            $"Token kind '{kind}' isn't handled."
                        );
                }

                startIndex = endIndex;
            }

            return true;
        }

        /// <summary>
        /// Tries to combines some patterns of tokens into a single one.
        /// I.e: a (type ident) will turn into a (type).
        /// </summary>
        public static void ReduceLine(ParserState state) {
            // apply one of following reduces:
            //  ident: type ident = value ident => ident: type = value
            //  ident = value ident => ident = value

            while (state.LineStack.TryPop(out Token token)) {
                // value ident => value
                if (ReduceOperations.Value(token, state)) {
                    continue;
                }

                // type ident => type
                if (ReduceOperations.Type(token, state)) {
                    continue;
                }

                state.ResultStack.Push(token);
            }
        }

        private static class ReduceOperations {
            public static bool Value(Token token, ParserState state) {
                if (!(token is ValueToken valueToken)) {
                    return false;
                }

                if (!state.ResultStack.TryPeek(out Token nextToken)
                 || !(nextToken is IdentifierToken identNextToken)
                ) {
                    // there is no next token available
                    // or it isn't a IdentifierToken
                    return false;
                }

                if (!(valueToken is AnyValueToken anyValueToken)) {
                    throw new System.InvalidOperationException(
                        $"Expecting a value token to be an '{nameof(AnyValueToken)}', but it's '{valueToken.GetType().ToString()}'."
                    );
                }

                state.ResultStack.Pop(); // pop peeked IdentifierToken

                anyValueToken.Value = identNextToken.Name;
                state.ResultStack.Push(anyValueToken);
                return true;
            }

            public static bool Type(Token token, ParserState state) {
                if (!(token is TypeToken typeToken)) {
                    return false;
                }

                if (!state.ResultStack.TryPeek(out Token nextToken)
                 || !(nextToken is IdentifierToken identNextToken)
                ) {
                    // there is no next token available
                    // or it isn't a IdentifierToken
                    return false;
                }

                state.ResultStack.Pop(); // pop peeked IdentifierToken

                typeToken.Set(identNextToken.Name);
                state.ResultStack.Push(typeToken);
                return true;
            }
        }
    }
}
