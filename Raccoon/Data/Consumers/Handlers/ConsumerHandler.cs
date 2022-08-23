using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public abstract class ConsumerHandler {
        #region Constructors

        public ConsumerHandler() {
        }

        #endregion Constructors

        #region Public Methods

        public abstract bool TryHandle(
            DataTokensConsumer consumer,
            object target,
            Token token,
            DataContract contract
        );

        #endregion Public Methods
    }
}
