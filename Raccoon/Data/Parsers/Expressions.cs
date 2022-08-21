
namespace Raccoon.Data.Parsers {
    public static class Expressions {
        public static bool Match<A>(Token token, out A tokenA)
            where A : Token
        {
            if (!(token is A tA)) {
                tokenA = default;
                return false;
            }

            tokenA = tA;
            return true;
        }

        public static bool Match<A, B>(Token token, ParserState state, out A tokenA, out B tokenB)
            where A : Token
            where B : Token
        {
            if (!(token is A tA)
             || !state.ResultStack.TryPeek(out Token nextToken)
             || !(nextToken is B tB)
            ) {
                tokenA = default;
                tokenB = default;
                return false;
            }

            state.ResultStack.Pop(); // pop peeked token B

            tokenA = tA;
            tokenB = tB;
            return true;
        }

        public static bool Match<A, B, C>(Token token, ParserState state, out A tokenA, out B tokenB, out C tokenC)
            where A : Token
            where B : Token
            where C : Token
        {
            if (!(token is A tA)
             || !state.ResultStack.TryPeek(out Token nextToken)
             || !(nextToken is B tB)
            ) {
                tokenA = default;
                tokenB = default;
                tokenC = default;
                return false;
            }

            state.ResultStack.Pop(); // pop peeked token B

            if (!state.ResultStack.TryPeek(out nextToken)
             || !(nextToken is C tC)
            ) {
                state.ResultStack.Push(tB); // return poped token B
                tokenA = default;
                tokenB = default;
                tokenC = default;
                return false;
            }

            state.ResultStack.Pop(); // pop peeked token C

            tokenA = tA;
            tokenB = tB;
            tokenC = tC;
            return true;
        }
    }
}
