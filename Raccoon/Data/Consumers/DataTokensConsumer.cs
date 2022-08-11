using Raccoon.Data.Parsers;

namespace Raccoon.Data {
    public abstract class DataTokensConsumer {
        protected DataTokensConsumer() {
        }

        public abstract void Consume(
            object rootTarget,
            DataContract rootContract,
            ListToken<Token> root
        );
    }
}
