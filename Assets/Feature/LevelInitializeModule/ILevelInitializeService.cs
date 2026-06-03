using Cysharp.Threading.Tasks;

namespace Feature.LevelInitializeModule {
    public interface ILevelInitializeService {
        UniTask Initialize();
        UniTask Dispose();
        UniTask ReloadScene();
    }
}