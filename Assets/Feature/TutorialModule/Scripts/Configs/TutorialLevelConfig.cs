using System.Collections.Generic;
using UnityEngine;

namespace Feature.TutorialModule.Scripts.Configs {
    [CreateAssetMenu(fileName = "TutorialLevelConfig", menuName = "Configs/Tutorial/TutorialLevelConfig")]
    public class TutorialLevelConfig : ScriptableObject {
        [field: SerializeField] public TutorialType TutorialType { get; private set; }
        [SerializeField] private List<TutorialAssetType> _assetsForTutorial;
        
        public IReadOnlyList<TutorialAssetType> AssetsForTutorial => _assetsForTutorial;
    }
}