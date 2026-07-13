using Cysharp.Threading.Tasks;

namespace Feature.WinLevelModule.Scripts {
    public class UnlockProgressConfigProvider : IUnlockProgressConfigProvider {
        private readonly UniTask<UnlockProgressConfig> _configTask;
        private UnlockProgressConfig _config;

        public UnlockProgressConfigProvider(UniTask<UnlockProgressConfig> configTask) {
            _configTask = configTask;
        }

        public async UniTask Initialize() {
            _config = await _configTask;
        }

        public UnlockProgressData GetEntryForLevel(int playerLevelBeforeGame) {
            return _config?.GetEntryForLevel(playerLevelBeforeGame);
        }
    }
}
