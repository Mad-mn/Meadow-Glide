using Cysharp.Threading.Tasks;
using Feature.PlayerInventoryModule.Configs;

namespace Feature.PlayerInventoryModule.Scripts {
    public interface IEconomyDataProvider {
        EconomyConfig EconomyConfig { get; }
        UniTask Initialize();
    }
}