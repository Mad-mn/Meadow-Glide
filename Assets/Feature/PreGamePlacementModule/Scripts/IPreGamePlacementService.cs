using Cysharp.Threading.Tasks;
using Feature.LevelModule.Scripts;

namespace Feature.PreGamePlacementModule.Scripts {
    public interface IPreGamePlacementService {
        bool HasEmptySlots(LevelConfig levelConfig);
        UniTask StartPlacement(LevelConfig levelConfig, int totalStripCount);
        void Cancel();
    }
}
