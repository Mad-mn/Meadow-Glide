using Cysharp.Threading.Tasks;

namespace Feature.PlayerInventoryModule.Scripts {
    public interface IResourceInfoProvider {
        UniTask Initialize();
        ResourceInfo GetInfo(ResourceType type);
    }
}
