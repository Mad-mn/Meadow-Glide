using Cysharp.Threading.Tasks;

namespace Feature.ChallengeModule.Scripts {
    public interface IChallengeConfigProvider {
        UniTask Initialize();
        ChallengeConfig GetConfig(ChallengeType type);
    }
}
