using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using UnityEngine;

namespace Feature.LevelModule.Scripts {
    [CreateAssetMenu(fileName = "LevelConfig_lvl_", menuName = "Configs/LevelConfigs/LevelConfig")]
    public class LevelConfig : ScriptableObject{
        [SerializeField] private List<CircleConfig> _circleConfigs = new List<CircleConfig>();
        
        public IReadOnlyList<CircleConfig> CircleConfigs => _circleConfigs;
    }
}