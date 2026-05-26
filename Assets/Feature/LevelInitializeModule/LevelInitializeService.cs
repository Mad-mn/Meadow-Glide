using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.LevelInitializeModule {
    public class LevelInitializeService : ILevelInitializeService {
        private readonly UniTask<CircleController> _circleControllerTask;
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly DiContainer _container;
        private readonly ISlideAreaService _slideAreaService;
        private readonly ICircleRotationService _circleRotationService;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly UniTask<SpriteMask> _maskTask;
        private readonly UniTask<Transform> _centerTask;
        private CircleParamsConfig _circleParamsConfig;
        private CircleController _circleController;
        private LevelConfigProvider _levelConfigProvider;

        public LevelInitializeService(
            UniTask<CircleController> circleControllerTask, 
            DiContainer container, 
            UniTask<LevelConfigProvider> levelConfigProviderTask, 
            ISlideAreaService slideAreaService, 
            ICircleRotationService circleRotationService,
            ISlideSegmentService slideSegmentService,
            UniTask<SpriteMask> maskTask,
            UniTask<Transform> centerTask,
            UniTask<CircleParamsConfig> circleParamsConfigTask) {
            _circleControllerTask = circleControllerTask;
            _container = container;
            _levelConfigProviderTask = levelConfigProviderTask;
            _slideAreaService = slideAreaService;
            _circleRotationService = circleRotationService;
            _slideSegmentService = slideSegmentService;
            _maskTask = maskTask;
            _centerTask = centerTask;
            _circleParamsConfigTask = circleParamsConfigTask;
        }
        
        public async UniTask Initialize() {
            CircleController circleControllerPrefab = await _circleControllerTask;
            _levelConfigProvider = await _levelConfigProviderTask;
            _circleParamsConfig = await _circleParamsConfigTask;
            SpriteMask maskPrefab = await _maskTask;
            Transform centerPrefab = await _centerTask;

            LevelData levelData = _levelConfigProvider.LevelDatas[1];
            
            await _slideAreaService.Initialize();
            
            float minRadius = _circleParamsConfig.GetRadius(0);
            float maxRadius = _circleParamsConfig.GetRadius(levelData.LevelConfig.CircleConfigs.Count - 1);
            
            // Get line width from the segment prefab's LineRenderer
            float lineWidth = 0.35f; // Default
            if (circleControllerPrefab.SegmentPrefab != null) {
                var lineRenderer = circleControllerPrefab.SegmentPrefab.GetComponent<UnityEngine.LineRenderer>();
                if (lineRenderer != null) {
                    lineWidth = lineRenderer.widthMultiplier * lineRenderer.widthCurve.Evaluate(0);
                }
            }

            // Spawn outer mask
            var mask = _container.InstantiatePrefabForComponent<SpriteMask>(maskPrefab);
            mask.transform.position = Vector3.zero;
            // Formula: radius + width / 2. Scale is diameter.
            float maskRadius = maxRadius + lineWidth / 2f;
            mask.transform.localScale = Vector3.one * maskRadius * 2f;

            // Spawn inner center mask/cover
            var centerCover = _container.InstantiatePrefab(centerPrefab);
            centerCover.transform.position = Vector3.zero;
            // Formula: inner edge of the first circle (minRadius - width / 2)
            float centerRadius = minRadius - lineWidth / 2f;
            centerCover.transform.localScale = Vector3.one * centerRadius * 2f;
            
            // Ensure center cover is visible on top
            var sr = centerCover.GetComponentInChildren<UnityEngine.SpriteRenderer>();
            if (sr != null) {
                sr.sortingOrder = 10;
            }

            SpawnCircles(levelData, circleControllerPrefab);
            _slideAreaService.SpawnSlideAreas(levelData.LevelConfig);
        }

        private void SpawnCircles(LevelData levelData, CircleController circleControllerPrefab) {
            _circleRotationService.Clear();
            _slideSegmentService.Clear();
            
            var circleConfigs = levelData.LevelConfig.CircleConfigs;
            for (int i = 0; i < circleConfigs.Count; i++) {
                CircleConfig config = circleConfigs[i];
                _circleController = _container.InstantiatePrefabForComponent<CircleController>(circleControllerPrefab);
                _circleController.transform.position = Vector3.zero;
                
                float radius = _circleParamsConfig.GetRadius(i);
                _circleController.Setup(config, radius);
                
                _circleRotationService.Register(_circleController);
                _slideSegmentService.RegisterCircle(_circleController);
            }
        }
    }
}