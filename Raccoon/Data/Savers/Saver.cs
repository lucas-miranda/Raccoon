using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Raccoon.Data.Parsers {
    public class Saver : System.IDisposable {
        private SaverState _globalState = new SaverState();

        public Saver() {
        }

        public bool IsDisposed { get; private set; }

        public void Save(
            StreamWriter stream,
            ListToken<Token> root,
            SaveSettings? settings = null
        ) {
            if (IsDisposed) {
                throw new System.InvalidOperationException("Saver is disposed.");
            }

            if (stream == null) {
                throw new System.InvalidOperationException("Stream isn't defined.");
            }

            _globalState.Settings = settings.HasValue ? settings.Value : SaveSettings.Default;
            _globalState.StringBuilder.Clear();

            string currentLevelRepresentation = string.Empty;
            Stack<StackContext> stack = new Stack<StackContext>();
            stack.Push(new StackContext(root));

            while (stack.TryPeek(out StackContext context)) {
                if (!context.Enumerator.MoveNext()) {
                    stack.Pop();
                    _globalState.PopLevel();
                    continue;
                }

                Token token = context.Enumerator.Current;

                // update current level representation prefix
                if (_globalState.Level * _globalState.Settings.SpacePerLevel != currentLevelRepresentation.Length) {
                    if (_globalState.Level * _globalState.Settings.SpacePerLevel > 0) {
                        currentLevelRepresentation = new string(' ', _globalState.Level * _globalState.Settings.SpacePerLevel);
                    } else {
                        currentLevelRepresentation = string.Empty;
                    }
                }

                // append level prefix
                if (currentLevelRepresentation.Length > 0) {
                    _globalState.StringBuilder.Append(currentLevelRepresentation);
                }

                // handle context current token
                TokensSaver.Handle(token, _globalState);

                // push inner context
                if (token is ListToken<Token> listToken) {
                    stack.Push(new StackContext(listToken));
                }

                // write to stream
                stream.WriteLine(_globalState.StringBuilder.ToString());
                _globalState.StringBuilder.Clear();
            }
        }

        public void Reset() {
            if (IsDisposed) {
                throw new System.InvalidOperationException("Save is disposed.");
            }

            _globalState.Reset();
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            IsDisposed = true;
        }

        #region StackContext Class

        private class StackContext {
            public StackContext(ListToken<Token> listToken) {
                ListToken = listToken;
                Enumerator = ListToken.Entries.GetEnumerator();
            }

            public ListToken<Token> ListToken { get; }
            public IEnumerator<Token> Enumerator { get; }
        }

        #endregion StackContext Class
    }
}
