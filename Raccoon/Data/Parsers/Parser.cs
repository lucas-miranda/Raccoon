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

            while (TryReadLine(stream, out string line)) {
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
                {
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

                        // transform previous token, if needed
                        {
                            Token lastToken = context.Entries[context.Entries.Count - 1];

                            if (lastToken is IdentifierToken identToken) {
                                // convert to NamedListToken

                                context.Entries.RemoveAt(context.Entries.Count - 1);
                                NamedListToken<Token> namedListToken = new NamedListToken<Token>(identToken);
                                context.Entries.Add(namedListToken);
                                stack.Push(namedListToken);
                                context = namedListToken;
                            } else {
                                string tokens = string.Empty;

                                foreach (Token t in _globalState.ResultStack) {
                                    tokens += "[" + t.GetType().Name + "]";
                                }

                                throw new System.InvalidOperationException(
                                    $"Unexpected token '{tokens}', at new level, after token '{lastToken.GetType().Name}', at previous level."
                                );
                            }
                        }
                    } else {
                        context = stack.Peek();
                    }

                    // push current result to context
                    while (_globalState.ResultStack.TryPop(out Token t)) {
                        context.Entries.Add(t);
                    }
                }

                _globalState.LineStack.Clear();
                _globalState.ResultStack.Clear();

#if DEBUG
                if (debug) {
                    Logger.Info($"----------------- Line ended -----------------");
                }
#endif
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
