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
            UniTask<CircleParamsConfig> circleParamsConfigTask) {
            _circleControllerTask = circleControllerTask;
            _container = container;
            _levelConfigProviderTask = levelConfigProviderTask;
            _slideAreaService = slideAreaService;
            _circleRotationService = circleRotationService;
            _slideSegmentService = slideSegmentService;
            _circleParamsConfigTask = circleParamsConfigTask;
        }
        
        public async UniTask Initialize() {
            CircleController circleControllerPrefab = await _circleControllerTask;
            _levelConfigProvider = await _levelConfigProviderTask;
            _circleParamsConfig = await _circleParamsConfigTask;

            LevelData levelData = _levelConfigProvider.LevelDatas[1];
            
            await _slideAreaService.Initialize();
            
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
                float width = _circleParamsConfig.GetWidth(i);
                _circleController.Setup(config, radius, width);
                
                _circleRotationService.Register(_circleController);
                _slideSegmentService.RegisterCircle(_circleController);
            }
        }
    }
}