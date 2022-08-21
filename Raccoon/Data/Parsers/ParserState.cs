using System.Collections.Generic;

namespace Raccoon.Data.Parsers {
    public class ParserState {
        public ParserState() {
        }

        public bool IsDisposed { get; private set; }
        public int? SpacePerLevel { get; set; }
        //public Stack<Token> Stack { get; } = new Stack<Token>();
        public Stack<Token> LineStack { get; } = new Stack<Token>();
        public Stack<Token> ResultStack { get; } = new Stack<Token>();
        public Stack<ListToken<Token>> ContextStack { get; } = new Stack<ListToken<Token>>();

        public void Reset() {
            SpacePerLevel = null;
            //Stack.Clear();
            LineStack.Clear();
            ResultStack.Clear();
            ContextStack.Clear();
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            IsDisposed = true;
            Reset();
        }
    }
}
