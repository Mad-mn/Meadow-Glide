using Cysharp.Threading.Tasks;

namespace Feature.LevelModule.Scripts {
    public interface ILevelService {
        UniTask Initialize();
        LevelData GetLevelDataForCurrentLevel();

        void LevelStarted();
    }
}