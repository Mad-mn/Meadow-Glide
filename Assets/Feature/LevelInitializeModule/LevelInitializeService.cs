using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.LevelInitializeModule {
    public class LevelInitializeService : ILevelInitializeService {
        private readonly UniTask<CircleController> _circleControllerTask;
        
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly IViewService _viewService;
        private readonly ILevelService _levelService;
        private readonly ITutorialService _tutorialService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IStripSpawnService _stripSpawnService;
        private readonly IInstantiator _instantiator;
        private readonly ISlideAreaService _slideAreaService;
        private readonly ICircleRotationService _circleRotationService;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly ICircleControllerService _circleControllerService;
        private readonly GameCircleModel _circleModel;
        private CircleParamsConfig _circleParamsConfig;
        
        private readonly List<CircleController> _spawnedCircles = new List<CircleController>();
        private readonly List<StripController> _spawnedStrips = new List<StripController>();

        public LevelInitializeService(
            UniTask<CircleController> circleControllerTask, 
            ISlideAreaService slideAreaService, 
            ICircleRotationService circleRotationService,
            ISlideSegmentService slideSegmentService,
            ICircleControllerService circleControllerService,
            GameCircleModel circleModel,
            UniTask<CircleParamsConfig> circleParamsConfigTask,
            IViewService viewService,
            ISaveDataService saveDataService,
            ISaveDataModel saveDataModel,
            ILevelService levelService,
            ITutorialService tutorialService,
            MoveTrackModel moveTrackModel, 
            IInstantiator instantiator,
            IStripSpawnService stripSpawnService) {
            _circleControllerTask = circleControllerTask;
            _slideAreaService = slideAreaService;
            _circleRotationService = circleRotationService;
            _slideSegmentService = slideSegmentService;
            _circleControllerService = circleControllerService;
            _circleModel = circleModel;
            _viewService = viewService;
            _levelService = levelService;
            _tutorialService = tutorialService;
            _moveTrackModel = moveTrackModel;
            _stripSpawnService = stripSpawnService;
            _instantiator = instantiator;
        }

        public async UniTask Initialize() {
            LevelData levelData = _levelService.GetLevelDataForCurrentLevel();
            _moveTrackModel.CacheMovesForLevel(levelData);
            _viewService.ShowView<GameView>(ViewType.GameView);

            await _slideAreaService.Initialize();
            await _stripSpawnService.Initialize();
            
            SpawnStrips(levelData);
            _slideAreaService.SpawnSlideAreas(levelData.LevelConfig);

            await _tutorialService.Initialize(_levelService.GetLevelDataForCurrentLevel());

            await UniTask.Delay(1);
            _levelService.LevelStarted();
        }

        public async UniTask Dispose() {
            _tutorialService.Deinitialize();
            _viewService.HideView(ViewType.GameView);
            _circleRotationService.Clear();
            _slideSegmentService.Clear();
            _slideAreaService.Clear();
            _circleControllerService.Reset();
            _circleModel.Clear();
            _tutorialService.Deinitialize();
            _levelService.LevelEnded();
            foreach (var circle in _spawnedCircles) {
                if (circle != null) {
                    Object.Destroy(circle.gameObject);
                }
            }

            _spawnedCircles.Clear();

            await UniTask.CompletedTask;
        }

        public async UniTask ReloadScene() {
            _viewService.HideView(ViewType.WinLevel);
            _viewService.HideView(ViewType.LoseView);
            await Dispose();
            await Initialize();
        }

        private void SpawnStrips(LevelData levelData) {
            _circleRotationService.Clear();
            _slideSegmentService.Clear();
            _spawnedCircles.Clear();

            var circleConfigs = levelData.LevelConfig.CircleConfigs;
            for (int i = 0; i < circleConfigs.Count; i++) {
                CircleConfig config = circleConfigs[i];
                StripController strip = _stripSpawnService.SpawnStrip(config, i);
                _spawnedStrips.Add(strip);
            }
        }
        
        private void SpawnCircles(LevelData levelData, CircleController circleControllerPrefab) {
            _circleRotationService.Clear();
            _slideSegmentService.Clear();
            _spawnedCircles.Clear();
            
            var circleConfigs = levelData.LevelConfig.CircleConfigs;
            for (int i = 0; i < circleConfigs.Count; i++) {
                CircleConfig config = circleConfigs[i];
                var circleController = _instantiator.InstantiatePrefabForComponent<CircleController>(circleControllerPrefab);
                circleController.transform.position = Vector3.zero;
                
                float radius = _circleParamsConfig.GetRadius(i);
                float width = _circleParamsConfig.GetWidth(i);
                circleController.Setup(config, radius, width);
                
                _circleRotationService.Register(circleController);
                _slideSegmentService.RegisterCircle(circleController);
                _circleModel.RegisterCircle(circleController);
                _spawnedCircles.Add(circleController);
            }
        }
    }
}