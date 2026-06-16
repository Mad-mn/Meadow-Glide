using System.Collections.Generic;
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts {
    public class TransactionService : ITransactionService {
        private readonly IResourceStorage _storage;
        private readonly ITransactionConfigsProvider _configsProvider;
        private readonly TransactionValidator _validator;
        private readonly TransactionExecutor _executor;

        public TransactionService(IResourceStorage storage, ITransactionConfigsProvider configsProvider) {
            _storage = storage;
            _configsProvider = configsProvider;
            _validator = new TransactionValidator(storage);
            _executor = new TransactionExecutor();
        }

        public TransactionResult Execute(TransactionConfig config) {
            TransactionResult validation = _validator.Validate(config);
            if (!validation.Success) {
                return validation;
            }

            _executor.ApplyCosts(config, _storage);
            List<ResourceAmount> rewards = _executor.ApplyRewards(config, _storage);

            return TransactionResult.Ok(rewards);
        }

        public TransactionResult Execute(TransactionId transactionId) {
            TransactionConfig config = _configsProvider.GetConfig(transactionId);
            if (config == null) {
                return TransactionResult.Fail(TransactionFailureReason.ProductUnavailable, $"Transaction config not found for {transactionId}");
            }

            return Execute(config);
        }
    }
}