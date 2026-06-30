using Cysharp.Threading.Tasks;

namespace Feature.PerfectMapViewModule.Scripts.Configs {
    public class PerfectMapRewardConfigProvider : IPerfectMapRewardConfigProvider {
        private readonly UniTask<PerfectMapRewardConfigs> _configsTask;
        private PerfectMapRewardConfigs _configs;

        public PerfectMapRewardConfigProvider(UniTask<PerfectMapRewardConfigs> configsTask) {
            _configsTask = configsTask;
        }

        public async UniTask Initialize() {
            _configs = await _configsTask;
        }

        public PerfectMapRewardConfig GetConfigForLevel(int level) {
            if (_configs == null) return null;

            foreach (PerfectMapRewardConfig config in _configs.Configs) {
                if (level >= config.FromLevel && level <= config.ToLevel) {
                    return config;
                }
            }

            return null;
        }
    }
}
