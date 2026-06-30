using System.Collections.Generic;
using Feature.AnalyticsModule.Scripts;
using Feature.ChallengeModule.Scripts;
using Feature.DailyChallengeCompleteViewModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.LoseViewModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;

namespace Feature.LevelResultModule.Scripts {
    public class LevelResultService : ILevelResultService {
        private readonly IChallengeService _challengeService;
        private readonly IMoveEfficiencyService _moveEfficiencyService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IPlayerInventoryService _inventoryService;
        private readonly IEconomyDataProvider _economyDataProvider;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IViewService _viewService;
        private readonly LevelModel _levelModel;
        private readonly IAnalyticsService _analyticsService;

        private bool _isWin;
        private bool _isLose;

        public LevelResultService(
            IChallengeService challengeService,
            IMoveEfficiencyService moveEfficiencyService,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            IPlayerInventoryService inventoryService,
            IEconomyDataProvider economyDataProvider,
            MoveTrackModel moveTrackModel,
            IViewService viewService,
            LevelModel levelModel,
            IAnalyticsService analyticsService) {
            _challengeService = challengeService;
            _moveEfficiencyService = moveEfficiencyService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _inventoryService = inventoryService;
            _economyDataProvider = economyDataProvider;
            _moveTrackModel = moveTrackModel;
            _viewService = viewService;
            _levelModel = levelModel;
            _analyticsService = analyticsService;
        }

        public void OnLevelWon() {
            if (_isWin)
                return;

            _isWin = true;

            if (_challengeService.IsActive) {
                CompleteForDailyChallenge();
            }
            else {
                CompleteForSimpleGame();
            }
        }

        private void CompleteForSimpleGame() {
            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            bool isReplay = _levelModel.ReplayLevel.HasValue;
            int completedLevel = _levelModel.ReplayLevel ?? progress.Level;

            int movesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft;
            MoveEfficiencyResult result = _moveEfficiencyService.Evaluate(movesUsed);
            SaveLevelCompletion(progress, completedLevel, result);

            SendAnalytics(progress, completedLevel, movesUsed);

            if (!isReplay) {
                progress.Level++;
            }
            _saveDataService.Save(SaveDataType.PlayerProgress);
            _inventoryService.Add(ResourceType.Coins, _economyDataProvider.EconomyConfig.LevelWinReward);
            _viewService.ShowView<WinLevel>(ViewType.WinLevel);
        }

        private void SendAnalytics(PlayerProgressData progress, int completedLevel, int movesUsed) {
            int attempts = 0;
            if (progress.CompletedLevels.TryGetValue(completedLevel, out LevelCompletionData completionData)) {
                attempts = completionData.Attempts;
            }
            _analyticsService.LevelCompleted(completedLevel, attempts, movesUsed);
        }

        private void CompleteForDailyChallenge() {
            int movesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft;
            MoveEfficiencyResult result = _moveEfficiencyService.Evaluate(movesUsed);
            _challengeService.OnChallengeCompleted(result);
            _analyticsService.DailyChallengeCompleted(0, result == MoveEfficiencyResult.PerfectClear);
            _viewService.ShowView<DailyChallengeCompleteView>(ViewType.DailyChallengeCompleteView);
        }

        public void OnLevelLost() {
            if (_isLose)
                return;

            _isLose = true;

            if (_challengeService.IsActive) {
                _viewService.ShowView<DailyChallengeCompleteView>(ViewType.DailyChallengeCompleteView);
            }
            else {
                _viewService.ShowView<LoseView>(ViewType.LoseView);
            }
        }

        public void Reset() {
            _isWin = false;
            _isLose = false;
        }

        private void SaveLevelCompletion(PlayerProgressData progress, int level, MoveEfficiencyResult result) {
            if (progress.CompletedLevels == null)
                progress.CompletedLevels = new Dictionary<int, LevelCompletionData>();

            if (!progress.CompletedLevels.TryGetValue(level, out LevelCompletionData existing)) {
                progress.CompletedLevels[level] = new LevelCompletionData { Status = result, MovesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft };
                return;
            }

            if ((int)result > (int)existing.Status) {
                existing.Status = result;
                existing.MovesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft;
            }
        }
    }
}
