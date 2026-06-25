using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts {
    public class PriceDataProvider : IPriceDataProvider {
        private readonly ITransactionConfigsProvider _transactionConfigsProvider;

        public PriceDataProvider(ITransactionConfigsProvider transactionConfigsProvider) {
            _transactionConfigsProvider = transactionConfigsProvider;
        }

        public int GetPrice(TransactionId transactionId) {
            TransactionConfig config = _transactionConfigsProvider.GetConfig(transactionId);
            return config.Costs[0].Amount;
        }
    }
}