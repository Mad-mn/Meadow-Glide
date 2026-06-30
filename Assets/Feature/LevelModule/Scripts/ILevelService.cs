using Cysharp.Threading.Tasks;

namespace Feature.LevelModule.Scripts {
    public interface ILevelService {
        UniTask Initialize();
        LevelData GetLevelDataForCurrentLevel();
        bool HasLevel(int level);

        void LevelStarted();
        void LevelEnded();
    }
}