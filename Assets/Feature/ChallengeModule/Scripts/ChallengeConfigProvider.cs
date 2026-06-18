using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Feature.ChallengeModule.Scripts {
    public class ChallengeConfigProvider : IChallengeConfigProvider {
        private readonly UniTask<ChallengeConfigs> _configsTask;
        private Dictionary<ChallengeType, ChallengeConfig> _configsByType;

        public ChallengeConfigProvider(UniTask<ChallengeConfigs> configsTask) {
            _configsTask = configsTask;
        }

        public async UniTask Initialize() {
            ChallengeConfigs configs = await _configsTask;
            _configsByType = new Dictionary<ChallengeType, ChallengeConfig>();

            foreach (ChallengeConfig config in configs.Configs) {
                if (config != null) {
                    _configsByType[config.ChallengeType] = config;
                }
            }
        }

        public ChallengeConfig GetConfig(ChallengeType type) {
            if (_configsByType != null && _configsByType.TryGetValue(type, out ChallengeConfig config)) {
                return config;
            }
            return null;
        }
    }
}
