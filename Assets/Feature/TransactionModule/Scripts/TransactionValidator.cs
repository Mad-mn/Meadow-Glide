using Feature.PlayerInventoryModule.Scripts;
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts
{
    public class TransactionValidator
    {
        private readonly IResourceStorage _storage;

        public TransactionValidator(IResourceStorage storage)
        {
            _storage = storage;
        }

        public TransactionResult Validate(TransactionConfig config)
        {
            if (config == null)
            {
                return TransactionResult.Fail(TransactionFailureReason.ProductUnavailable, "Transaction config is null");
            }

            if (config.Costs == null)
            {
                return TransactionResult.Ok();
            }

            foreach (var cost in config.Costs)
            {
                if (!_storage.HasEnough(cost.Type, cost.Amount))
                {
                    string message = config.FailureMessage ?? $"Not enough {cost.Type}";
                    return TransactionResult.Fail(GetFailureReason(cost.Type), message);
                }
            }

            return TransactionResult.Ok();
        }

        private TransactionFailureReason GetFailureReason(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Coins:
                    return TransactionFailureReason.NotEnoughCoins;
                default:
                    return TransactionFailureReason.NotEnoughResource;
            }
        }
    }
}
