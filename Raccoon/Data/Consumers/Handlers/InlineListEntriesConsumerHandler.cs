using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public class InlineListEntriesConsumerHandler : ConsumerHandler {
        #region Private Members

        private static InlineListEntriesConsumerHandler _instance;

        #endregion Private Members

        #region Constructors

        private InlineListEntriesConsumerHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public static InlineListEntriesConsumerHandler Instance {
            get {
                if (_instance == null) {
                    _instance = new InlineListEntriesConsumerHandler();
                }

                return _instance;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override bool TryHandle(
            DataTokensConsumer consumer,
            object target,
            Token token,
            DataContract contract
        ) {
            if (!(token is InlineListEntriesToken inlineListEntriesToken)) {
                return false;
            }

            try {
                foreach (ValueAssignToken valueAssignToken in inlineListEntriesToken.Entries) {
                    ValueAssignConsumerHandler.Instance.TryHandle(consumer, target, valueAssignToken, contract);
                }
            } catch (System.Exception e) {
                throw new System.InvalidOperationException($"Operation failed at {nameof(InlineListEntriesToken)} entry.", e);
            }

            return true;
        }

        #endregion Public Methods
    }
}
