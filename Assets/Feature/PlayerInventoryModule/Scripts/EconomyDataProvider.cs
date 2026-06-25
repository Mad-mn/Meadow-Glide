using Cysharp.Threading.Tasks;
using Feature.PlayerInventoryModule.Configs;

namespace Feature.PlayerInventoryModule.Scripts {
    public class EconomyDataProvider : IEconomyDataProvider {
        private readonly UniTask<EconomyConfig> _economyConfigTask;
        public EconomyConfig EconomyConfig { get; private set; }
        
        public EconomyDataProvider(UniTask<EconomyConfig> economyConfigTask) {
            _economyConfigTask = economyConfigTask;
        }

        public async UniTask Initialize() {
            EconomyConfig = await _economyConfigTask;
        }
    }
}