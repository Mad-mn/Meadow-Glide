using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.LevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.LevelInitializeModule {
    public class LevelInitializeService : ILevelInitializeService {
        private readonly UniTask<CircleController> _circleControllerTask;
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly DiContainer _container;
        private CircleController _circleController;
        private LevelConfigProvider _levelConfigProvider;

        public LevelInitializeService(UniTask<CircleController> circleControllerTask, DiContainer container, UniTask<LevelConfigProvider> levelConfigProviderTask) {
            _circleControllerTask = circleControllerTask;
            _container = container;
            _levelConfigProviderTask = levelConfigProviderTask;
        }
        
        public async UniTask Initialize() {
            CircleController circleControllerPrefab = await _circleControllerTask;
            _levelConfigProvider = await _levelConfigProviderTask;

            LevelData levelData = _levelConfigProvider.LevelDatas[1];
            
            SpawnCircles(levelData, circleControllerPrefab);
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