using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Feature.TutorialModule.Scripts {
    public interface ITutorialAssetProvider {
        UniTask Initialize();
        UniTask PrewarmAssets(IReadOnlyList<TutorialAssetType> tutorialAssets);
        T GetAsset<T>(TutorialAssetType tutorialAssetType);
        void ReleaseAssets();
    }
}