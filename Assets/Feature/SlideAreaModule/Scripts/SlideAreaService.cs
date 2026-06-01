using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using UnityEngine;
using Zenject;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaService : ISlideAreaService {
        private const float OUTER_POINT_OFFSET = 0.2f;
        
        private readonly DiContainer _container;
        private readonly UniTask<SlideArea> _slideAreaTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly ISlideAreaDataProvider _slideAreaDataProvider;
        private CircleParamsConfig _circleParamsConfig;
        
        private readonly List<SlideArea> _spawnedAreas = new List<SlideArea>();
        private SlideArea _slideAreaPrefab;

        public bool IsSliding { get; set; }
        public IReadOnlyList<SlideArea> SpawnedAreas => _spawnedAreas;

        public SlideAreaService(DiContainer container, UniTask<SlideArea> slideAreaTask, UniTask<CircleParamsConfig> circleParamsConfigTask,
            ISlideAreaDataProvider slideAreaDataProvider) {
            _container = container;
            _slideAreaTask = slideAreaTask;
            _circleParamsConfigTask = circleParamsConfigTask;
            _slideAreaDataProvider = slideAreaDataProvider;
        }

        public async UniTask Initialize() {
            _slideAreaPrefab = await _slideAreaTask;
            _circleParamsConfig = await _circleParamsConfigTask;
        }

        public void SpawnSlideAreas(LevelConfig levelConfig) {
            Clear();

            if (levelConfig.SlideAreaConfigs == null) return;

            foreach (SlideAreaConfig config in levelConfig.SlideAreaConfigs) {
                if (config.startCircleIndex < 0 || config.startCircleIndex >= levelConfig.CircleConfigs.Count ||
                    config.endCircleIndex < 0 || config.endCircleIndex >= levelConfig.CircleConfigs.Count) {
                    Debug.LogError($"SlideAreaService: Invalid circle indices in config for sector {config.sectorIndex}");
                    continue;
                }

                float startR = _circleParamsConfig.GetRadius(config.startCircleIndex);
                float startW = _circleParamsConfig.GetWidth(config.startCircleIndex);
                float endR = _circleParamsConfig.GetRadius(config.endCircleIndex);
                float endW = _circleParamsConfig.GetWidth(config.endCircleIndex);
                float dist = _circleParamsConfig.DistanceBetweenCircles;

                float innerBoundary = startR - startW / 2f - dist / 2f;
                float outerBoundary = endR + endW / 2f + dist / 2f;
                
                SlideArea slideArea = _container.InstantiatePrefabForComponent<SlideArea>(_slideAreaPrefab);
                slideArea.transform.position = Vector3.zero;

                SlideAreaData data = _slideAreaDataProvider.GetSlideAreaData(config.SlideAreaStatus);
                slideArea.Initialize(config, data, innerBoundary, outerBoundary);
                
                _spawnedAreas.Add(slideArea);
            }
        }

        public void Clear() {
            foreach (SlideArea area in _spawnedAreas) {
                if (area != null) {
                    Object.Destroy(area.gameObject);
                }
            }
            _spawnedAreas.Clear();
        }
    }
}