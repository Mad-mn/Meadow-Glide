using Cysharp.Threading.Tasks;
using Feature.ChallengeModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.LoseViewModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.StatusModule.Scripts.Segments;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.LevelModule.Scripts {
    public class LevelService : ILevelService {
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ITutorialService _tutorialService;
        private readonly ISegmentStatusService _segmentStatusService;
        private readonly LevelModel _levelModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IViewService _viewService;
        private readonly IChallengeService _challengeService;
        private LevelConfigProvider _levelConfigProvider;

        public LevelService(UniTask<LevelConfigProvider> levelConfigProviderTask, ISaveDataModel saveDataModel, ITutorialService tutorialService,
            ISegmentStatusService segmentStatusService, LevelModel levelModel, MoveTrackModel moveTrackModel, IViewService viewService,
            IChallengeService challengeService) {
            _levelConfigProviderTask = levelConfigProviderTask;
            _saveDataModel = saveDataModel;
            _tutorialService = tutorialService;
            _segmentStatusService = segmentStatusService;
            _levelModel = levelModel;
            _moveTrackModel = moveTrackModel;
            _viewService = viewService;
            _challengeService = challengeService;
        }

        public async UniTask Initialize() {
            _levelConfigProvider = await _levelConfigProviderTask;
            _moveTrackModel.OnMovesChanged += CheckForLose;
        }

        public LevelData GetLevelDataForCurrentLevel() {
            if (_challengeService.IsActive) {
                return _challengeService.GetCurrentLevel();
            }

            PlayerProgressData playerProgressData = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            return _levelConfigProvider.LevelDatas[playerProgressData.Level];
        }

        public void LevelStarted() {
            _tutorialService.TryActivateTutorial();
            _segmentStatusService.UpdateStatus();
            _levelModel.StartLevel();
        }

        public void LevelEnded() {
            _levelModel.EndLevel();
        }
        
        private void CheckForLose() {
            if (_moveTrackModel.MovesLeft == 0) {
                _viewService.ShowView<LoseView>(ViewType.LoseView);
            }
        }
    }
}