using Cysharp.Threading.Tasks;

namespace Feature.PlayerInventoryModule.Scripts {
    public class ResourceInfoProvider : IResourceInfoProvider {
        private readonly UniTask<ResourceInfoConfig> _configTask;
        private ResourceInfoConfig _config;

        public ResourceInfoProvider(UniTask<ResourceInfoConfig> configTask) {
            _configTask = configTask;
        }

        public async UniTask Initialize() {
            _config = await _configTask;
        }

        public ResourceInfo GetInfo(ResourceType type) {
            if (_config == null)
                return null;

            return _config.GetByType(type);
        }
    }
}
