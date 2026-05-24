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
        private readonly DiContainer _container;
        private readonly ISlideAreaService _slideAreaService;
        private CircleController _circleController;
        private LevelConfigProvider _levelConfigProvider;

        public LevelInitializeService(UniTask<CircleController> circleControllerTask, DiContainer container, UniTask<LevelConfigProvider> levelConfigProviderTask, ISlideAreaService slideAreaService) {
            _circleControllerTask = circleControllerTask;
            _container = container;
            _levelConfigProviderTask = levelConfigProviderTask;
            _slideAreaService = slideAreaService;
        }
        
        public async UniTask Initialize() {
            CircleController circleControllerPrefab = await _circleControllerTask;
            _levelConfigProvider = await _levelConfigProviderTask;

            LevelData levelData = _levelConfigProvider.LevelDatas[1];
            
            await _slideAreaService.Initialize();
            SpawnCircles(levelData, circleControllerPrefab);
            _slideAreaService.SpawnSlideAreas(levelData.LevelConfig);
        }

        private void SpawnCircles(LevelData levelData, CircleController circleControllerPrefab) {
            foreach (CircleConfig config in levelData.LevelConfig.CircleConfigs) {
                _circleController = _container.InstantiatePrefabForComponent<CircleController>(circleControllerPrefab);
                _circleController.transform.position = Vector3.zero;
                _circleController.Setup(config);
            }
        }
    }
}