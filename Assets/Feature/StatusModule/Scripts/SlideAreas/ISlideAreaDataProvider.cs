using Cysharp.Threading.Tasks;

namespace Feature.StatusModule.Scripts.SlideAreas {
    public interface ISlideAreaDataProvider {
        UniTask Initialize();
        SlideAreaData GetSlideAreaData(SlideAreaStatus slideAreaStatus);
    }
}