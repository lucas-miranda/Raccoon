using Raccoon.Data.Parsers;

namespace Raccoon.Data.Consumers {
    public class ValueConsumerHandler : ConsumerHandler {
        #region Private Members

        private static ValueConsumerHandler _instance;

        #endregion Private Members

        #region Constructors

        private ValueConsumerHandler() {
        }

        #endregion Constructors

        #region Public Properties

        public static ValueConsumerHandler Instance {
            get {
                if (_instance == null) {
                    _instance = new ValueConsumerHandler();
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
            if (!(token is ValueToken valueToken)) {
                return false;
            }

            ConsumerContext.Entry currentContext = consumer.Context.Current;

            if (currentContext == null) {
                throw new System.InvalidOperationException($"There is no context available. When handling a {nameof(ValueToken)}.");
            }

            consumer.RegisterValue(null, valueToken.AsObject());
            return true;
        }

        #endregion Public Methods
    }
}
