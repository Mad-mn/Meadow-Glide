using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using UnityEngine;

namespace Feature.LevelModule.Scripts {
    [CreateAssetMenu(fileName = "LevelConfig_lvl_", menuName = "Configs/LevelConfigs/LevelConfig")]
    public class LevelConfig : ScriptableObject {
        [SerializeField] private List<CircleConfig> _circleConfigs = new List<CircleConfig>();
        [SerializeField] private List<SlideAreaConfig> _slideAreaConfigs = new List<SlideAreaConfig>();

        public IReadOnlyList<CircleConfig> CircleConfigs => _circleConfigs;
        public IReadOnlyList<SlideAreaConfig> SlideAreaConfigs => _slideAreaConfigs;
    }

    [Serializable]
    public class SlideAreaConfig {
        public int startCircleIndex;
        public int endCircleIndex;
        public int sectorIndex;
        public int totalSegments;
    }
}