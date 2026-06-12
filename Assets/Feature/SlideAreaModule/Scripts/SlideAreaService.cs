using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using UnityEngine;
using Zenject;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaService : ISlideAreaService {
        private readonly DiContainer _container;
        private readonly UniTask<SlideArea> _slideAreaTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly ISlideAreaDataProvider _slideAreaDataProvider;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly ISlideSegmentService _slideSegmentService;
        private CircleParamsConfig _circleParamsConfig;

        private readonly List<SlideArea> _spawnedAreas = new List<SlideArea>();
        private SlideArea _slideAreaPrefab;

        public SlideAreaService(DiContainer container, UniTask<SlideArea> slideAreaTask, UniTask<CircleParamsConfig> circleParamsConfigTask,
            ISlideAreaDataProvider slideAreaDataProvider, SlideAreaModel slideAreaModel, ISlideSegmentService slideSegmentService) {
            _container = container;
            _slideAreaTask = slideAreaTask;
            _circleParamsConfigTask = circleParamsConfigTask;
            _slideAreaDataProvider = slideAreaDataProvider;
            _slideAreaModel = slideAreaModel;
            _slideSegmentService = slideSegmentService;
        }

        public async UniTask Initialize() {
            _slideAreaPrefab = await _slideAreaTask;
            _circleParamsConfig = await _circleParamsConfigTask;
        }

        public void SpawnSlideAreas(LevelConfig levelConfig) {
            Clear();

            if (levelConfig.SlideAreaConfigs == null) return;

            float stripLoopLength = _circleParamsConfig.StripLoopLength;

            foreach (SlideAreaConfig config in levelConfig.SlideAreaConfigs) {
                if (config.startCircleIndex < 0 || config.startCircleIndex >= levelConfig.CircleConfigs.Count ||
                    config.endCircleIndex < 0 || config.endCircleIndex >= levelConfig.CircleConfigs.Count) {
                    Debug.LogError($"SlideAreaService: Invalid strip indices in config for sector {config.sectorIndex}");
                    continue;
                }

                float innerBoundaryY = _circleParamsConfig.GetStripBoundaryY(config.endCircleIndex, true);
                float outerBoundaryY = _circleParamsConfig.GetStripBoundaryY(config.startCircleIndex, false);

                float segmentSpan = stripLoopLength / config.totalSegments;
                float leftX = config.sectorIndex * segmentSpan - stripLoopLength * 0.5f;
                float rightX = leftX + segmentSpan;

                SlideArea slideArea = _container.InstantiatePrefabForComponent<SlideArea>(_slideAreaPrefab);
                slideArea.transform.position = Vector3.zero;

                SlideAreaData data = _slideAreaDataProvider.GetSlideAreaData(config.SlideAreaStatus);
                slideArea.Initialize(config, data, innerBoundaryY, outerBoundaryY, leftX, rightX);

                _spawnedAreas.Add(slideArea);
            }

            _slideAreaModel.SetupAreas(_spawnedAreas);
            _slideSegmentService.UpdateSegmentsInAreas();
        }

        public void Clear() {
            foreach (SlideArea area in _spawnedAreas) {
                if (area != null)
                    Object.Destroy(area.gameObject);
            }

            _spawnedAreas.Clear();
        }
    }
}
