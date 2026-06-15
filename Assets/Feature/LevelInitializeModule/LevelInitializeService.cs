using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.PreGamePlacementModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.StripRotationModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.UndoModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.LevelInitializeModule {
    public class LevelInitializeService : ILevelInitializeService {
        private readonly IViewService _viewService;
        private readonly ILevelService _levelService;
        private readonly ITutorialService _tutorialService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IStripSpawnService _stripSpawnService;
        private readonly ISlideAreaService _slideAreaService;
        private readonly IStripRotationService _stripRotationService;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly ICircleControllerService _circleControllerService;
        private readonly StripModel _stripModel;
        private readonly IPreGamePlacementService _preGamePlacementService;
        private readonly IInteractionStateService _interactionStateService;
        private readonly IUndoService _undoService;

        private readonly List<StripController> _spawnedStrips = new List<StripController>();

        public LevelInitializeService(
            ISlideAreaService slideAreaService,
            IStripRotationService stripRotationService,
            ISlideSegmentService slideSegmentService,
            ICircleControllerService circleControllerService,
            StripModel stripModel,
            IViewService viewService,
            ILevelService levelService,
            ITutorialService tutorialService,
            MoveTrackModel moveTrackModel,
            IStripSpawnService stripSpawnService,
            IPreGamePlacementService preGamePlacementService,
            IInteractionStateService interactionStateService,
            IUndoService undoService) {
            _slideAreaService = slideAreaService;
            _stripRotationService = stripRotationService;
            _slideSegmentService = slideSegmentService;
            _circleControllerService = circleControllerService;
            _stripModel = stripModel;
            _viewService = viewService;
            _levelService = levelService;
            _tutorialService = tutorialService;
            _moveTrackModel = moveTrackModel;
            _stripSpawnService = stripSpawnService;
            _preGamePlacementService = preGamePlacementService;
            _interactionStateService = interactionStateService;
            _undoService = undoService;
        }

        public async UniTask Initialize() {
            _interactionStateService.ResetInputBlock();
            _undoService.Clear();

            LevelData levelData = _levelService.GetLevelDataForCurrentLevel();
            _moveTrackModel.CacheMovesForLevel(levelData);
            _viewService.ShowView<GameView>(ViewType.GameView);

            await _slideAreaService.Initialize();
            await _stripSpawnService.Initialize();

            SpawnStrips(levelData);
            _slideAreaService.SpawnSlideAreas(levelData.LevelConfig, levelData.LevelConfig.CircleConfigs.Count);

            await _tutorialService.Initialize(_levelService.GetLevelDataForCurrentLevel());
            _preGamePlacementService.StartPlacement(levelData.LevelConfig, levelData.LevelConfig.CircleConfigs.Count).Forget();

            await UniTask.Delay(1);
            _levelService.LevelStarted();
        }

        public async UniTask Dispose() {
            _preGamePlacementService.Cancel();
            _tutorialService.Deinitialize();
            _viewService.HideView(ViewType.GameView);
            _stripRotationService.Clear();
            _slideSegmentService.Clear();
            _slideAreaService.Clear();
            _circleControllerService.Reset();
            _stripModel.Clear();

            foreach (StripController strip in _spawnedStrips) {
                if (strip != null)
                    Object.Destroy(strip.gameObject);
            }

            _spawnedStrips.Clear();
            _levelService.LevelEnded();

            await UniTask.CompletedTask;
        }

        public async UniTask ReloadScene() {
            _viewService.HideView(ViewType.WinLevel);
            _viewService.HideView(ViewType.LoseView);
            await Dispose();
            await Initialize();
        }

        private void SpawnStrips(LevelData levelData) {
            _stripRotationService.Clear();
            _slideSegmentService.Clear();
            _spawnedStrips.Clear();
            _stripModel.Clear();

            var circleConfigs = levelData.LevelConfig.CircleConfigs;
            int totalStripCount = circleConfigs.Count;
            _slideSegmentService.SetTotalStripCount(totalStripCount);

            for (int i = 0; i < circleConfigs.Count; i++) {
                CircleConfig config = circleConfigs[i];
                StripController strip = _stripSpawnService.SpawnStrip(config, i, totalStripCount);
                _spawnedStrips.Add(strip);
            }
        }
    }
}
