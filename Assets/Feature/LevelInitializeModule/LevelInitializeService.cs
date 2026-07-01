using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.AnalyticsModule.Scripts;
using Feature.ChallengeModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.PreGamePlacementModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
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
        private readonly IMoveEfficiencyService _moveEfficiencyService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IChallengeService _challengeService;
        private readonly LevelModel _levelModel;
        private readonly IAnalyticsService _analyticsService;
        private readonly UniTask<GameBack> _gameBackPrefabTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly IInstantiator _instantiator;

        private readonly List<StripController> _spawnedStrips = new List<StripController>();
        private GameObject _spawnedGameBack;
        private CircleParamsConfig _circleParamsConfig;

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
            IUndoService undoService,
            IMoveEfficiencyService moveEfficiencyService,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            IChallengeService challengeService,
            LevelModel levelModel,
            IAnalyticsService analyticsService,
            UniTask<GameBack> gameBackPrefabTask,
            UniTask<CircleParamsConfig> circleParamsConfigTask,
            IInstantiator instantiator) {
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
            _moveEfficiencyService = moveEfficiencyService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _challengeService = challengeService;
            _levelModel = levelModel;
            _analyticsService = analyticsService;
            _gameBackPrefabTask = gameBackPrefabTask;
            _circleParamsConfigTask = circleParamsConfigTask;
            _instantiator = instantiator;
        }

        public async UniTask Initialize() {
            _interactionStateService.ResetInputBlock();
            _undoService.Clear();
            IncrementAttempt();
            _circleParamsConfig = await _circleParamsConfigTask;

            LevelData levelData = _levelService.GetLevelDataForCurrentLevel();
            _moveTrackModel.CacheMovesForLevel(levelData);
            _moveEfficiencyService.SetMinMoves(levelData.LevelConfig.ShortestSolution);
            _viewService.ShowView<GameView>(ViewType.GameView);

            await _slideAreaService.Initialize();
            await _stripSpawnService.Initialize();

            SpawnStrips(levelData);
            _slideAreaService.SpawnSlideAreas(levelData.LevelConfig, levelData.LevelConfig.CircleConfigs.Count);

            await _tutorialService.Initialize(levelData);
            _preGamePlacementService.StartPlacement(levelData.LevelConfig, levelData.LevelConfig.CircleConfigs.Count).Forget();

            await UniTask.Delay(1);
            _levelService.LevelStarted();

            SendAnalytics(levelData);
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

            if (_spawnedGameBack != null) {
                Object.Destroy(_spawnedGameBack);
                _spawnedGameBack = null;
            }

            _levelService.LevelEnded();

            await UniTask.CompletedTask;
        }

        public async UniTask ReloadScene() {
            int? savedReplayLevel = _levelModel.ReplayLevel;
            _viewService.HideView(ViewType.WinLevel);
            _viewService.HideView(ViewType.LoseView);
            _viewService.HideView(ViewType.DailyChallengeCompleteView);
            await Dispose();
            _levelModel.ReplayLevel = savedReplayLevel;
            await Initialize();
        }

        private void IncrementAttempt() {
            if (_challengeService.IsActive) {
                return;
            }
            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            int currentLevel = _levelModel.ReplayLevel ?? progress.Level;

            if (progress.CompletedLevels == null)
                progress.CompletedLevels = new Dictionary<int, LevelCompletionData>();

            if (!progress.CompletedLevels.TryGetValue(currentLevel, out LevelCompletionData data)) {
                data = new LevelCompletionData();
                progress.CompletedLevels[currentLevel] = data;
            }

            data.Attempts++;
            _saveDataService.Save(SaveDataType.PlayerProgress);
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

            SpawnGameBack(totalStripCount);
        }

        private async void SpawnGameBack(int totalStripCount) {
            if (_circleParamsConfig == null)
                return;

            GameBack prefab = await _gameBackPrefabTask;
            if (prefab == null) return;

            _spawnedGameBack = _instantiator.InstantiatePrefab(prefab);

            float spacing = _circleParamsConfig.GetStripSpacing();
            float stripHeight = _circleParamsConfig.StripHeight;
            float stripLoopLength = _circleParamsConfig.StripLoopLength;

            float totalHeight = (totalStripCount - 1) * spacing + stripHeight;
            float margin = stripHeight;

            SpriteRenderer sr = _spawnedGameBack.GetComponent<SpriteRenderer>();
            float spriteWidth = sr != null && sr.sprite != null ? sr.sprite.rect.width / sr.sprite.pixelsPerUnit : 1f;
            float spriteHeight = sr != null && sr.sprite != null ? sr.sprite.rect.height / sr.sprite.pixelsPerUnit : 1f;

            float desiredWidth = stripLoopLength + margin * 2f;
            float desiredHeight = totalHeight + margin * 1.5f;
            _spawnedGameBack.transform.localScale = new Vector3(desiredWidth / spriteWidth, desiredHeight / spriteHeight, 1f);
        }

        private void SendAnalytics(LevelData levelData) {
            int levelId = levelData.LevelID;
            if (_challengeService.IsActive) {
                _analyticsService.DailyChallengeStarted(levelId);
            }
            else if (_levelModel.ReplayLevel.HasValue) {
                _analyticsService.PerfectMapLevelStarted(levelId);
            }
            else {
                _analyticsService.LevelStarted(levelId);
            }
        }
    }
}
