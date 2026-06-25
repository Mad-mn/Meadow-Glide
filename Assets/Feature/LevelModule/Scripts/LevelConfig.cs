using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using Feature.TutorialModule.Scripts.Configs;
using UnityEngine;

namespace Feature.LevelModule.Scripts {
    [CreateAssetMenu(fileName = "LevelConfig_lvl_", menuName = "Configs/LevelConfigs/LevelConfig")]
    public class LevelConfig : ScriptableObject {
        [SerializeField] private List<CircleConfig> _circleConfigs = new List<CircleConfig>();
        [SerializeField] private List<SlideAreaConfig> _slideAreaConfigs = new List<SlideAreaConfig>();
        [SerializeField] private int _difficulty;
        [SerializeField] private float _difficultyMultiplier = 1;
        [SerializeField] private int _averageMoves;
        [SerializeField] private int _seed;
        [SerializeField] private List<LevelMoveData> _solutionPath = new List<LevelMoveData>();
        [field: SerializeField] public TutorialLevelConfig TutorialLevelConfig { get; private set; }

        public IReadOnlyList<CircleConfig> CircleConfigs => _circleConfigs;
        public IReadOnlyList<SlideAreaConfig> SlideAreaConfigs => _slideAreaConfigs;
        public int Difficulty => _difficulty;
        public int AverageMoves => _averageMoves;
        public int Seed => _seed;
        public IReadOnlyList<LevelMoveData> SolutionPath => _solutionPath;
        public int ShortestSolution => _solutionPath.Count;

        public int MovesForLevel =>
            Mathf.RoundToInt(_averageMoves * _difficultyMultiplier);

        public void SetConfigs(List<CircleConfig> circles, List<SlideAreaConfig> areas, int difficulty,
            int averageMoves, int seed, List<LevelMoveData> solutionPath) {
            _circleConfigs = circles;
            _slideAreaConfigs = areas;
            _difficulty = difficulty;
            _averageMoves = averageMoves;
            _seed = seed;
            _solutionPath = solutionPath ?? new List<LevelMoveData>();
        }
    }

    [Serializable]
    public class LevelMoveData {
        public string MoveType;
        public int Index;
        public int Offset;

        public LevelMoveData() { }

        public LevelMoveData(string moveType, int index, int offset) {
            MoveType = moveType;
            Index = index;
            Offset = offset;
        }

        public override string ToString() => $"{MoveType}({Index}, offset:{Offset})";
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
