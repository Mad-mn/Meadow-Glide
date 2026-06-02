using Cysharp.Threading.Tasks;
using Feature.LevelModule.Scripts;

namespace Feature.TutorialModule.Scripts {
    public interface ITutorialService {
        UniTask Initialize(LevelData currentLevelData);
        void TryActivateTutorial();
        void Deinitialize();
    }
}