using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.LevelModule.Scripts;

namespace Feature.SlideAreaModule.Scripts {
    public interface ISlideAreaService {
        UniTask Initialize();
        void SpawnSlideAreas(LevelConfig levelConfig);
        void Clear();
    }
}