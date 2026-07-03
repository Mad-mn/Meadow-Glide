using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.LevelModule.Scripts;
using Feature.StripsModule.Scripts;

namespace Feature.PreGamePlacementModule.Scripts {
    public interface IPreGamePlacementService {
        bool HasEmptySlots(LevelConfig levelConfig);
        UniTask StartPlacement(LevelConfig levelConfig, int totalStripCount);
        void Cancel();
        IReadOnlyList<StripController> GetPoolPieces();
    }
}
