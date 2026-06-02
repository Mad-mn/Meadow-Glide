using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Feature.TutorialModule.Scripts.Configs {
    [CreateAssetMenu(fileName = "TutorialAssetsConfig", menuName = "Configs/Tutorial/TutorialAssetsConfig")]
    public class TutorialAssetsConfig : ScriptableObject {
        [SerializeField] private List<TutorialAssetData> _assets;
        
        public IReadOnlyList<TutorialAssetData> Assets => _assets;
    }

    [Serializable]
    public class TutorialAssetData {
        public TutorialAssetType TutorialAssetType;
        public AssetReference AssetReference;
    }
}