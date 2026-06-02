using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.TutorialModule.Scripts.Configs;
using UnityEngine;

namespace Feature.TutorialModule.Scripts {
    public class TutorialAssetProvider : ITutorialAssetProvider {
        private readonly UniTask<TutorialAssetsConfig> _tutorialAssetsConfigTask;
        private readonly IAddressableService _addressableService;

        private TutorialAssetsConfig _assetsConfig;
        private readonly Dictionary<TutorialAssetType, object> _cachedAssets = new Dictionary<TutorialAssetType, object>();
        
        public TutorialAssetProvider(UniTask<TutorialAssetsConfig> tutorialAssetsConfigTask, IAddressableService addressableService) {
            _tutorialAssetsConfigTask = tutorialAssetsConfigTask;
            _addressableService = addressableService;
        }
        
        public async UniTask Initialize() {
            if (_assetsConfig != null) return;
            _assetsConfig = await _tutorialAssetsConfigTask;
        }

        public async UniTask PrewarmAssets(IReadOnlyList<TutorialAssetType> tutorialAssets) {
            await Initialize();
            
            foreach (TutorialAssetType assetType in tutorialAssets) {
                if (_cachedAssets.ContainsKey(assetType)) {
                    continue;
                }

                var assetData = _assetsConfig.Assets.FirstOrDefault(a => a.TutorialAssetType == assetType);
                if (assetData == null) {
                    Debug.LogWarning($"[TutorialAssetProvider] Asset for type {assetType} not found in config.");
                    continue;
                }

                string key = assetData.AssetReference.RuntimeKey.ToString();
                // We load as GameObject by default for prewarming as most tutorial assets are prefabs to be spawned.
                var asset = await _addressableService.GetAsset<GameObject>(key);
                if (asset != null) {
                    _cachedAssets[assetType] = asset;
                }
                else {
                    // Fallback to object if GameObject load failed (e.g. it's a ScriptableObject or other type)
                    var otherAsset = await _addressableService.GetAsset<object>(key);
                    if (otherAsset != null) {
                        _cachedAssets[assetType] = otherAsset;
                    }
                }
            }
        }

        public T GetAsset<T>(TutorialAssetType tutorialAssetType) {
            if (_cachedAssets.TryGetValue(tutorialAssetType, out object asset)) {
                if (asset is T typedAsset) {
                    return typedAsset;
                }

                // If the cached asset is a GameObject but the requested type is a Component
                if (asset is GameObject gameObject) {
                    var component = gameObject.GetComponent<T>();
                    if (component != null) {
                        return component;
                    }
                }

                try {
                    return (T)asset;
                }
                catch (System.InvalidCastException) {
                    Debug.LogError($"[TutorialAssetProvider] Cannot cast asset {tutorialAssetType} of type {asset?.GetType().Name} to requested type {typeof(T).Name}");
                    throw;
                }
            }
            
            Debug.LogError($"[TutorialAssetProvider] Asset for type {tutorialAssetType} was not prewarmed!");
            return default;
        }

        public void ReleaseAssets() {
            if (_assetsConfig == null) return;

            foreach (TutorialAssetType assetType in _cachedAssets.Keys) {
                var assetData = _assetsConfig.Assets.FirstOrDefault(a => a.TutorialAssetType == assetType);
                if (assetData != null) {
                    _addressableService.ReleaseAsset(assetData.AssetReference.RuntimeKey.ToString());
                }
            }
            _cachedAssets.Clear();
        }
    }
}