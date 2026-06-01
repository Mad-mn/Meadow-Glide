using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using UnityEngine;

namespace Feature.LevelModule.Scripts {
    [CreateAssetMenu(fileName = "LevelConfig_lvl_", menuName = "Configs/LevelConfigs/LevelConfig")]
    public class LevelConfig : ScriptableObject {
        [SerializeField] private List<CircleConfig> _circleConfigs = new List<CircleConfig>();
        [SerializeField] private List<SlideAreaConfig> _slideAreaConfigs = new List<SlideAreaConfig>();
        [SerializeField] private int _difficulty;

        public IReadOnlyList<CircleConfig> CircleConfigs => _circleConfigs;
        public IReadOnlyList<SlideAreaConfig> SlideAreaConfigs => _slideAreaConfigs;
        public int Difficulty => _difficulty;

        public void SetConfigs(List<CircleConfig> circles, List<SlideAreaConfig> areas, int difficulty) {
            _circleConfigs = circles;
            _slideAreaConfigs = areas;
            _difficulty = difficulty;
        }
    }

    [Serializable]
    public class SlideAreaConfig {
        public int startCircleIndex;
        public int endCircleIndex;
        public int sectorIndex;
        public int totalSegments;
        public SlideAreaStatus SlideAreaStatus;
        public List<CircleColorType> Colors;
    }
}