using Cysharp.Threading.Tasks;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.StatusModule.Scripts.Segments;
using Feature.TutorialModule.Scripts;

namespace Feature.LevelModule.Scripts {
    public class LevelService : ILevelService {
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ITutorialService _tutorialService;
        private readonly ISegmentStatusService _segmentStatusService;
        private readonly LevelModel _levelModel;
        private LevelConfigProvider _levelConfigProvider;

        private int _startedLevelId;

        public LevelService(UniTask<LevelConfigProvider> levelConfigProviderTask, ISaveDataModel saveDataModel, ITutorialService tutorialService,
            ISegmentStatusService segmentStatusService, LevelModel levelModel) {
            _levelConfigProviderTask = levelConfigProviderTask;
            _saveDataModel = saveDataModel;
            _tutorialService = tutorialService;
            _segmentStatusService = segmentStatusService;
            _levelModel = levelModel;
        }

        public async UniTask Initialize() {
            _levelConfigProvider = await _levelConfigProviderTask;
        }

        public LevelData GetLevelDataForCurrentLevel() {
            PlayerProgressData playerProgressData = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            if (_levelConfigProvider.LevelDatas.Count <= playerProgressData.Level) {
                return _levelConfigProvider.LevelDatas[19];
            }
            return _levelConfigProvider.LevelDatas[playerProgressData.Level];
        }

        public void LevelStarted() {
            _tutorialService.TryActivateTutorial();
            _segmentStatusService.UpdateStatus();
            _levelModel.StartLevel();
            _startedLevelId = GetLevelDataForCurrentLevel()
                .LevelID;
            TinySauce.OnGameStarted(_startedLevelId);
        }

        public void LevelEnded() {
            _levelModel.EndLevel();
            bool isUserCompleteLvl = GetLevelDataForCurrentLevel()
                .LevelID > _startedLevelId;
            TinySauce.OnGameFinished(isUserCompleteLvl, 0);
        }
    }
}