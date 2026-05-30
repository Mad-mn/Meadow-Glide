using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SlideAreaModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.LevelInitializeModule {
    public class LevelInitializeService : ILevelInitializeService {
        private readonly UniTask<CircleController> _circleControllerTask;
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly IViewService _viewService;
        private readonly ISaveDataService _saveDataService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly DiContainer _container;
        private readonly ISlideAreaService _slideAreaService;
        private readonly ICircleRotationService _circleRotationService;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly ICircleControllerService _circleControllerService;
        private readonly GameCircleModel _circleModel;
        private CircleParamsConfig _circleParamsConfig;
        private LevelConfigProvider _levelConfigProvider;
        
        private readonly List<CircleController> _spawnedCircles = new List<CircleController>();

        public LevelInitializeService(
            UniTask<CircleController> circleControllerTask, 
            DiContainer container, 
            UniTask<LevelConfigProvider> levelConfigProviderTask, 
            ISlideAreaService slideAreaService, 
            ICircleRotationService circleRotationService,
            ISlideSegmentService slideSegmentService,
            ICircleControllerService circleControllerService,
            GameCircleModel circleModel,
            UniTask<CircleParamsConfig> circleParamsConfigTask,
            IViewService viewService,
            ISaveDataService saveDataService,
            ISaveDataModel saveDataModel) {
            _circleControllerTask = circleControllerTask;
            _container = container;
            _levelConfigProviderTask = levelConfigProviderTask;
            _slideAreaService = slideAreaService;
            _circleRotationService = circleRotationService;
            _slideSegmentService = slideSegmentService;
            _circleControllerService = circleControllerService;
            _circleModel = circleModel;
            _circleParamsConfigTask = circleParamsConfigTask;
            _viewService = viewService;
            _saveDataService = saveDataService;
            _saveDataModel = saveDataModel;
        }
        
        public async UniTask Initialize() {
            _viewService.ShowView<GameView>(ViewType.GameView);

            CircleController circleControllerPrefab = await _circleControllerTask;
            _levelConfigProvider = await _levelConfigProviderTask;
            _circleParamsConfig = await _circleParamsConfigTask;
            PlayerProgressData playerProgressData = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);

            LevelData levelData = _levelConfigProvider.LevelDatas[playerProgressData.Level];
            
            await _slideAreaService.Initialize();
            
            SpawnCircles(levelData, circleControllerPrefab);
            _slideAreaService.SpawnSlideAreas(levelData.LevelConfig);
        }

        public async UniTask Dispose() {
            _viewService.HideView(ViewType.GameView);
            _circleRotationService.Clear();
            _slideSegmentService.Clear();
            _slideAreaService.Clear();
            _circleControllerService.Reset();
            _circleModel.Clear();

            foreach (var circle in _spawnedCircles) {
                if (circle != null) {
                    Object.Destroy(circle.gameObject);
                }
            }
            _spawnedCircles.Clear();
            
            await UniTask.CompletedTask;
        }

        private void SpawnCircles(LevelData levelData, CircleController circleControllerPrefab) {
            _circleRotationService.Clear();
            _slideSegmentService.Clear();
            _spawnedCircles.Clear();
            
            var circleConfigs = levelData.LevelConfig.CircleConfigs;
            for (int i = 0; i < circleConfigs.Count; i++) {
                CircleConfig config = circleConfigs[i];
                var circleController = _container.InstantiatePrefabForComponent<CircleController>(circleControllerPrefab);
                circleController.transform.position = Vector3.zero;
                
                float radius = _circleParamsConfig.GetRadius(i);
                float width = _circleParamsConfig.GetWidth(i);
                circleController.Setup(config, radius, width);
                
                _circleRotationService.Register(circleController);
                _slideSegmentService.RegisterCircle(circleController);
                _spawnedCircles.Add(circleController);
            }
        }
    }
}