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
        private const float OUTER_POINT_OFFSET = 0.2f;
        
        private readonly DiContainer _container;
        private readonly UniTask<SlideArea> _slideAreaTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly ISlideAreaDataProvider _slideAreaDataProvider;
        private readonly GameCircleModel _circleModel;
        private readonly SlideAreaModel _slideAreaModel;
        private CircleParamsConfig _circleParamsConfig;
        
        private readonly List<SlideArea> _spawnedAreas = new List<SlideArea>();
        private SlideArea _slideAreaPrefab;

        public bool IsSliding { get; set; }
        public IReadOnlyList<SlideArea> SpawnedAreas => _spawnedAreas;

        public SlideAreaService(DiContainer container, UniTask<SlideArea> slideAreaTask, UniTask<CircleParamsConfig> circleParamsConfigTask,
            ISlideAreaDataProvider slideAreaDataProvider, GameCircleModel circleModel, SlideAreaModel slideAreaModel) {
            _container = container;
            _slideAreaTask = slideAreaTask;
            _circleParamsConfigTask = circleParamsConfigTask;
            _slideAreaDataProvider = slideAreaDataProvider;
            _circleModel = circleModel;
            _slideAreaModel = slideAreaModel;
        }

        public async UniTask Initialize() {
            _slideAreaPrefab = await _slideAreaTask;
            _circleParamsConfig = await _circleParamsConfigTask;
            UpdateSegmentsInAreas();
        }
        
        public void UpdateSegmentsInAreas() {
            List<CircleController> sortedCircles = _circleModel.Circles.OrderBy(c => c.Radius).ToList();
            HashSet<CircleSegment> segmentsInAreas = new HashSet<CircleSegment>();
            IReadOnlyList<SlideArea> spawnedAreas = SpawnedAreas;

            for (int circleIdx = 0; circleIdx < sortedCircles.Count; circleIdx++) {
                var circle = sortedCircles[circleIdx];
                foreach (var segment in circle.SpawnedSegments) {
                    float worldAngle = (circle.transform.eulerAngles.z + segment.transform.localEulerAngles.z) % 360;
                    worldAngle = (worldAngle + 360) % 360;

                    foreach (var area in spawnedAreas) {
                        if (circleIdx >= area.StartCircleIndex && circleIdx <= area.EndCircleIndex) {
                            float angleStep = 360f / area.TotalSegments;
                            float areaCenterAngle = area.SectorIndex * angleStep;

                            if (Mathf.Abs(Mathf.DeltaAngle(worldAngle, areaCenterAngle)) < 0.1f) {
                                segmentsInAreas.Add(segment);
                                break;
                            }
                        }
                    }
                }
            }

            _slideAreaModel.UpdateSegmentsInAreas(segmentsInAreas);
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