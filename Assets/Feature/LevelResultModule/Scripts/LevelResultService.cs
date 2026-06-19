using Feature.ChallengeModule.Scripts;
using Feature.DailyChallengeCompleteViewModule.Scripts;
using Feature.LoseViewModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;

namespace Feature.LevelResultModule.Scripts {
    public class LevelResultService : ILevelResultService {
        private readonly IChallengeService _challengeService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IPlayerInventoryService _inventoryService;
        private readonly IEconomyDataProvider _economyDataProvider;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IViewService _viewService;

        private bool _isWin;
        private bool _isLose;

        public LevelResultService(
            IChallengeService challengeService,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            IPlayerInventoryService inventoryService,
            IEconomyDataProvider economyDataProvider,
            MoveTrackModel moveTrackModel,
            IViewService viewService) {
            _challengeService = challengeService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _inventoryService = inventoryService;
            _economyDataProvider = economyDataProvider;
            _moveTrackModel = moveTrackModel;
            _viewService = viewService;
        }

        public void OnLevelWon() {
            if (_isWin)
                return;

            _isWin = true;

            if (_challengeService.IsActive) {
                int movesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft;
                _challengeService.OnChallengeCompleted(_moveTrackModel.ShortestSolution, _moveTrackModel.AverageMoves, movesUsed);
                _viewService.ShowView<DailyChallengeCompleteView>(ViewType.DailyChallengeCompleteView);
            }
            else {
                _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level++;
                _saveDataService.Save(SaveDataType.PlayerProgress);
                _inventoryService.Add(ResourceType.Coins, _economyDataProvider.EconomyConfig.LevelWinReward);
                _viewService.ShowView<WinLevel>(ViewType.WinLevel);
            }
        }

        public void OnLevelLost() {
            if (_isLose)
                return;

            _isLose = true;

            if (_challengeService.IsActive) {
                int movesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft;
                _challengeService.OnChallengeCompleted(_moveTrackModel.ShortestSolution, _moveTrackModel.AverageMoves, movesUsed);
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
    }
}
