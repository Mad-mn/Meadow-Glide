using Cysharp.Threading.Tasks;

namespace Feature.PerfectMapViewModule.Scripts.Configs {
    public interface IPerfectMapRewardConfigProvider {
        UniTask Initialize();
        PerfectMapRewardConfig GetConfigForLevel(int level);
    }
}
