using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.LevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaService : ISlideAreaService {
        private const float OUTER_POINT_OFFSET = 0.2f;
        
        private readonly DiContainer _container;
        private readonly UniTask<SlideArea> _slideAreaTask;
        
        private readonly List<SlideArea> _spawnedAreas = new List<SlideArea>();
        private SlideArea _slideAreaPrefab;

        public SlideAreaService(DiContainer container, UniTask<SlideArea> slideAreaTask) {
            _container = container;
            _slideAreaTask = slideAreaTask;
        }

        public async UniTask Initialize() {
            _slideAreaPrefab = await _slideAreaTask;
        }

        public void SpawnSlideAreas(LevelConfig levelConfig) {
            ClearSlideAreas();

            if (levelConfig.SlideAreaConfigs == null) return;

            foreach (var config in levelConfig.SlideAreaConfigs) {
                if (config.startCircleIndex < 0 || config.startCircleIndex >= levelConfig.CircleConfigs.Count ||
                    config.endCircleIndex < 0 || config.endCircleIndex >= levelConfig.CircleConfigs.Count) {
                    Debug.LogError($"SlideAreaService: Invalid circle indices in config for sector {config.sectorIndex}");
                    continue;
                }

                float innerRadius = levelConfig.CircleConfigs[config.startCircleIndex].radius;
                float outerRadius = levelConfig.CircleConfigs[config.endCircleIndex].radius+OUTER_POINT_OFFSET;
                
                SlideArea slideArea = _container.InstantiatePrefabForComponent<SlideArea>(_slideAreaPrefab);
                slideArea.transform.position = Vector3.zero;
                slideArea.Initialize(config, innerRadius, outerRadius);
                
                _spawnedAreas.Add(slideArea);
            }
        }

        private void ClearSlideAreas() {
            foreach (var area in _spawnedAreas) {
                if (area != null) {
                    Object.Destroy(area.gameObject);
                }
            }
            _spawnedAreas.Clear();
        }
    }
}