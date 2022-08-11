using System.Collections.Generic;
using System.IO;

namespace Raccoon.Data.Parsers {
    public class Parser : System.IDisposable {
        private ParserState _globalState = new ParserState();

        public Parser() {
        }

        public bool IsDisposed { get; private set; }

#if DEBUG
        public ListToken<Token> Parse(StreamReader stream, bool debug = false) {
#else
        public ListToken<Token> Parse(StreamReader stream) {
#endif
            if (IsDisposed) {
                throw new System.InvalidOperationException("Parser is disposed.");
            }

            if (stream == null) {
                throw new System.InvalidOperationException("Stream isn't defined.");
            }

            _globalState.Reset();

            // root entry
            ListToken<Token> root = new ListToken<Token>();

            // list context stack
            Stack<ListToken<Token>> stack = new Stack<ListToken<Token>>();
            stack.Push(root);

            int lineIndex = 0;

            try {
                while (TryReadLine(stream, out string line)) {
                    lineIndex += 1;

                    // parse tokens from line and store at state
                    if (!TokensParser.ParseLine(line, _globalState, out int level)) {
                        continue;
                    }

#if DEBUG
                    if (debug) {
                        Logger.Info($"Parsing line: '{line}'");

                        Logger.Info($"Parsed line tokens ({_globalState.LineStack.Count}) /\\ :");
                        foreach (Token token in _globalState.LineStack) {
                            Logger.WriteLine($" -> {token}");
                        }
                    }
#endif

                    // try to reduce some tokens by merging them
                    TokensParser.ReduceLine(_globalState);

#if DEBUG
                    if (debug) {
                        Logger.Info($"After TokensParser.ReduceLine ({_globalState.ResultStack.Count}) \\/:");
                        foreach (Token token in _globalState.ResultStack) {
                            Logger.WriteLine($" -> {token}");
                        }
                    }
#endif

                    // reallocate result tokens to line tokens stack
                    while (_globalState.ResultStack.TryPop(out Token token)) {
                        _globalState.LineStack.Push(token);
                    }

                    // try to reduce them by context
                    ContextParser.Reduce(_globalState);

#if DEBUG
                    if (debug) {
                        Logger.Info($"After ContextParser.Reduce ({_globalState.ResultStack.Count}) \\/:");
                        foreach (Token token in _globalState.ResultStack) {
                            Logger.WriteLine($" -> {token}");
                        }
                    }
#endif

                    // retrieve result from every operation and insert at list token
                    // NOTE  tokens, at result stack, are ordered from left to right (at this step)
                    HandleResult(stack, level, debug);

                    //

                    _globalState.LineStack.Clear();
                    _globalState.ResultStack.Clear();

#if DEBUG
                    if (debug) {
                        Logger.Info($"----------------- Line ended -----------------");
                    }
#endif
                }
            } catch (System.Exception e) {
                throw new System.Exception($"At line {lineIndex}.", e);
            }

#if DEBUG
            if (debug) {
                Logger.Info($"Result Tokens ({root.Entries.Count}):");
                foreach (Token token in root.Entries) {
                    ShowTokenEntry(token);
                }
            }
#endif

            return root;
        }

        public void Reset() {
            if (IsDisposed) {
                throw new System.InvalidOperationException("Parser is disposed.");
            }

            _globalState.Reset();
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            IsDisposed = true;

            _globalState.Dispose();
            _globalState = null;
        }

        private bool TryReadLine(StreamReader stream, out string line) {
            line = stream.ReadLine();
            return line != null;
        }

        private ListToken<Token> PrepareContext(Stack<ListToken<Token>> stack, int level, bool debug) {
            // stack operations
            ListToken<Token> context;
            int previousLevel = stack.Count - 1;

            if (level < previousLevel) {
                // pop contexts

                while (stack.Count - 1 > level) {
                    stack.Pop();
                }

                context = stack.Peek();
            } else if (level > previousLevel) {
                // push context

                if (level - previousLevel > 1) {
                    throw new System.InvalidOperationException(
                        $"Illegal stack level incrementing operation. Increments can't be greater than 1, but a increment from {previousLevel} to {level} was tried."
                    );
                }

                context = stack.Peek();

                // transform previous token into a list
                {
                        // last token at list may not be the right one to transform
                    int lastTokenIndex = context.Entries.Count - 1,

                        // used to register inlined tokens at new list token
                        registerAsEntriesStartIndex = lastTokenIndex + 1;

                    // token to be transformed
                    Token lastToken = context.Entries[lastTokenIndex];

                    if (lastToken is InlineListEntriesToken) {
                        // because last line has a InlineListEntriesToken
                        // we should use last token as -2
                        lastTokenIndex -= 1;
                        registerAsEntriesStartIndex = lastTokenIndex + 1;
                        lastToken = context.Entries[lastTokenIndex];
                    }

                    if (lastToken is IdentifierToken identToken) {
                        // convert to NamedListToken

                        if (debug) {
                            Logger.Info($"Converting previous last token '{lastToken}' to an {nameof(NamedListToken<Token>)} (with {context.Entries.Count - registerAsEntriesStartIndex} inlined tokens)");
                        }

                        context.Entries.RemoveAt(lastTokenIndex);
                        NamedListToken<Token> namedListToken = new NamedListToken<Token>(identToken);
                        context.Entries.Insert(lastTokenIndex, namedListToken);

                        // register some inlined tokens as entries
                        for (int i = registerAsEntriesStartIndex; i < context.Entries.Count; i++) {
                            Token token = context.Entries[i];
                            namedListToken.Entries.Add(token);
                            context.Entries.RemoveAt(i);
                            i -= 1;
                        }

                        stack.Push(namedListToken);
                        context = namedListToken;
                    /*
                    } else if (lastToken is TypedIdentifierToken typedIdentToken) {
                        // convert to NamedListToken

                        Logger.Info($"Converting last token '{lastToken}' to an {nameof(NamedListToken<Token>)}");
                        context.Entries.RemoveAt(context.Entries.Count - 1);
                        NamedListToken<Token> namedListToken = new NamedListToken<Token>(typedIdentToken);
                        context.Entries.Add(namedListToken);
                        stack.Push(namedListToken);
                        context = namedListToken;
                        */
                    } else {
                        string tokens = string.Empty;

                        foreach (Token t in _globalState.ResultStack) {
                            tokens += "[" + t.GetType().ToString() + "]";
                        }

                        throw new System.InvalidOperationException(
                            $"Unexpected token '{tokens}', at new level, after token '{lastToken.GetType().ToString()}', at previous level."
                        );
                    }
                }
            } else {
                context = stack.Peek();
            }

            return context;
        }

        /// <summary>
        /// Handling results from ResultStack, apply any final transformation and registers at context's entries.
        /// </summary>
        private void HandleResult(Stack<ListToken<Token>> stack, int level, bool debug) {
            ListToken<Token> context = PrepareContext(stack, level, debug);

            if (_globalState.ResultStack.Count == 1
             && _globalState.ResultStack.TryPeek(out Token singleToken)
             && singleToken is IdentifierToken singleIdentToken
             && context is NamedListToken<Token> namedListContext
             && namedListContext.Identifier is TypedIdentifierToken typedListIdent
            ) {
                // transform a single identifier into a typed value (using list typed identifier)
                // and push to the context list

                switch (typedListIdent.Type.Type) {
                    case TypeKind.Vector:
                        {
                            if (typedListIdent.Type.Nested == null) {
                                throw new System.InvalidOperationException($"Expected 1 nested types at type '{typedListIdent.Type}' (identifier: {typedListIdent}), but it's undefined.");
                            }

                            if (typedListIdent.Type.Nested.Length != 1) {
                                throw new System.InvalidOperationException($"Expected 1 nested type at type '{typedListIdent.Type}' (identifier: {typedListIdent}), but {typedListIdent.Type.Nested.Length} nested types was found.");
                            }

                            TypeToken elementType = typedListIdent.Type.Nested[0];

                            namedListContext.Entries.Add(
                                elementType.CreateValueToken(singleIdentToken.Name)
                            );
                        }
                        return;

                    case TypeKind.Custom:
                        // TODO:  let's user handle this case, since it's a custom type
                        break;

                    default:
                        break;
                }
            }

            // push current result to context
            while (_globalState.ResultStack.TryPop(out Token t)) {
                context.Entries.Add(t);
            }
        }

#if DEBUG
        private void ShowTokenEntry(Token token) {
            Logger.WriteLine($" -> {token}");

            if (token is ListToken<Token> listToken) {
                Logger.Indent();
                foreach (Token subToken in listToken.Entries) {
                    ShowTokenEntry(subToken);
                }
                Logger.Unindent();
            }
        }
#endif
    }
}
