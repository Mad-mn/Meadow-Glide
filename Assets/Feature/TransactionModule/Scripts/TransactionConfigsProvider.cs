using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts
{
    public class TransactionConfigsProvider : ITransactionConfigsProvider
    {
        private readonly UniTask<TransactionConfigs> _configsTask;
        private Dictionary<TransactionId, TransactionConfig> _configsById;

        public TransactionConfigsProvider(UniTask<TransactionConfigs> configsTask)
        {
            _configsTask = configsTask;
        }

        public async UniTask Initialize()
        {
            var configs = await _configsTask;
            _configsById = new Dictionary<TransactionId, TransactionConfig>();

            foreach (TransactionConfig config in configs.Configs)
            {
                if (config != null && config.TransactionId != TransactionId.None)
                {
                    _configsById[config.TransactionId] = config;
                }
            }
        }

        public TransactionConfig GetConfig(TransactionId transactionId)
        {
            if (_configsById != null && _configsById.TryGetValue(transactionId, out var config))
            {
                return config;
            }

            return null;
        }
    }
}
